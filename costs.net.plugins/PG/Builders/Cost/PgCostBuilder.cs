namespace costs.net.plugins.PG.Builders.Cost
{
    using core.Builders;
    using core.Builders.Request;
    using core.Builders.Response.Cost;
    using core.Exceptions;
    using core.Extensions;
    using core.Models.Costs;
    using core.Models.Workflow;
    using core.Services.Agency;
    using core.Services.Costs;
    using core.Services.Project;
    using core.Services.Rules;
    using dataAccess;
    using dataAccess.Entity;
    using Extensions;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Models.Rules;
    using Models.Stage;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models;
    using core.Models.Response;
    using core.Models.User;
    using core.Services;
    using core.Services.CostTemplate;
    using core.Services.Currencies;
    using Models.Payments;
    using Payments;
    using Workflow;
    using ApprovalModel = core.Builders.Response.ApprovalModel;
    using CostFormDetails = dataAccess.Entity.CostFormDetails;
    using CostStageModel = core.Builders.Response.Cost.CostStageModel;
    using Services;
    using Services.Costs;

    public class PgCostBuilder : ICostBuilder
    {
        private readonly IPgStageBuilder _pgStageBuilder;
        private readonly IRuleService _ruleService;
        private readonly ICostStageRevisionService _costStageRevisionService;
        private readonly IAgencyService _agencyService;
        private readonly IProjectService _projectService;
        private readonly EFContext _efContext;

        private readonly ICostNumberGeneratorService _costNumberGeneratorService;
        private readonly IPgCurrencyService _currencyService;
        private readonly IPgLedgerMaterialCodeService _pgLedgerMaterialCodeService;
        private readonly ICostLineItemService _costLineItemService;
        private readonly ICostTemplateVersionService _costTemplateVersionService;
        private readonly IPermissionService _permissionService;
        private readonly IPgCostService _pgCostService;
        private readonly IPgCostSectionTotalsBuilder _pgTotalsBuilder;
        private readonly IPgPaymentService _pgPaymentService;
        private readonly IExchangeRateService _exchangeRateService;

        public PgCostBuilder(IPgStageBuilder pgStageBuilder,
            IRuleService ruleService,
            ICostStageRevisionService costStageRevisionService,
            IAgencyService agencyService,
            IProjectService projectService,
            EFContext efContext,
            ICostNumberGeneratorService costNumberGeneratorService,
            IPgCurrencyService currencyService,
            IPgLedgerMaterialCodeService pgLedgerMaterialCodeService,
            ICostLineItemService costLineItemService,
            ICostTemplateVersionService costTemplateVersionService,
            IPermissionService permissionService,
            IPgCostService pgCostService,
            IPgCostSectionTotalsBuilder pgTotalsBuilder,
            IPgPaymentService pgPaymentService,
            IExchangeRateService exchangeRateService)
        {
            _pgStageBuilder = pgStageBuilder;
            _ruleService = ruleService;
            _costStageRevisionService = costStageRevisionService;
            _agencyService = agencyService;
            _projectService = projectService;
            _efContext = efContext;
            _costNumberGeneratorService = costNumberGeneratorService;
            _currencyService = currencyService;
            _pgLedgerMaterialCodeService = pgLedgerMaterialCodeService;
            _costLineItemService = costLineItemService;
            _costTemplateVersionService = costTemplateVersionService;
            _permissionService = permissionService;
            _pgCostService = pgCostService;
            _pgTotalsBuilder = pgTotalsBuilder;
            _pgPaymentService = pgPaymentService;
            _exchangeRateService = exchangeRateService;
        }

        public async Task<ICreateCostResponse> CreateCost(CostUser user, CreateCostModel createCostModel)
        {
            if (createCostModel == null)
            {
                throw new ArgumentNullException(nameof(createCostModel));
            }

            if (createCostModel.StageDetails == null)
            {
                throw new Exception("StageDetails is missing");
            }

            var stageDetailsForm = createCostModel.StageDetails.Data.ToModel<PgStageDetailsForm>();

            if (string.IsNullOrEmpty(stageDetailsForm.ProjectId))
            {
                throw new Exception("Project is missing");
            }

            var templateVersion = await _costTemplateVersionService.GetLatestTemplateVersion(createCostModel.TemplateId);
            if (templateVersion == null)
            {
                throw new Exception("Template version missing");
            }

            var project = await _projectService.GetByGadmid(stageDetailsForm.ProjectId);
            if (project == null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "ProjectId is invalid!");
            }

            var agencyAbstractType = _efContext.AbstractType
                .Include(at => at.Agency)
                .FirstOrDefault(a =>
                    a.Agency != null &&
                    a.Parent.Agency.Id == project.AgencyId &&
                    user.Agency.Id == a.ObjectId);
            if (agencyAbstractType == null)
            {
                throw new Exception("AgencyAbstractType is missing");
            }

            await _permissionService.CheckAccess(user.Id, agencyAbstractType.Id, AclActionType.Create, typeof(Cost).Name.ToLower());

            var isExternalPurchasesEnabled = !agencyAbstractType.Agency.IsCyclone() || stageDetailsForm.BudgetRegion?.Key != Constants.BudgetRegion.NorthAmerica;

            var cost = new CostBuilderModel
            {
                CostType = templateVersion.CostTemplate.CostType,
                CostTemplateVersionId = templateVersion.Id,
                Stages = new[]
                {
                    await BuildCostFirstStageModel(stageDetailsForm, user.Id, templateVersion.CostTemplate.CostType, createCostModel.StageDetails)
                },
                ParentId = agencyAbstractType.Id,
                ProjectId = project.Id,
                IsExternalPurchasesEnabled = isExternalPurchasesEnabled,
                ContentType = stageDetailsForm.ContentType?.Value
            };

            var response = new CreateCostResponse
            {
                Cost = cost
            };

            return response;
        }

        public Task<string> CreateCostNumber(Guid projectId, CostType costType, string contentType)
        {
            return _costNumberGeneratorService.Generate(projectId, costType.ToString(), contentType);
        }

        public Task<OperationResponse> IsValidForSubmittion(Guid costId)
        {
            return _pgCostService.IsValidForSubmittion(costId);
        }

        public async Task SubmitCost(Guid costId)
        {
            var cost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .ThenInclude(r => r.CostStage)
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (!cost.LatestCostStageRevision.IsPaymentCurrencyLocked)
            {
                await SetPaymentCurrencyRate(cost);
                await _costLineItemService.RefreshDefaultCurrencyValues(costId);
            }

            cost.LatestCostStageRevision.IsPaymentCurrencyLocked = true;
            cost.LatestCostStageRevision.IsLineItemSectionCurrencyLocked = true;

            await _efContext.SaveChangesAsync();
            await _pgLedgerMaterialCodeService.UpdateLedgerMaterialCodes(cost.LatestCostStageRevision.Id);
        }

        public Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(Guid revisionId)
        {
            return _pgCostService.GetRevisionTotals(revisionId);
        }

        public Task<(decimal total, decimal totalInLocalCurrency)> GetRevisionTotals(CostStageRevision revision)
        {
            return _pgCostService.GetRevisionTotals(revision);
        }

        public async Task<IUpdateCostResponse> UpdateCost(UserIdentity userIdentity, Guid costId, Guid latestRevisionId, CostType costType, IStageDetails stageDetails,
            IProductionDetails productionDetails)
        {
            var currentCost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(c => c.CostStage)
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(x => x.StageDetails)
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(x => x.ProductDetails)
                .Where(c => c.Id == costId)
                .FirstAsync();

            var oldStageForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(currentCost.LatestCostStageRevision.StageDetails.Data);
            var oldProductionForm = JsonConvert.DeserializeObject<PgProductionDetailsForm>(currentCost.LatestCostStageRevision.ProductDetails.Data);
            var newStageDetailsForm = stageDetails.Data.ToModel<PgStageDetailsForm>();
            var newProductionDetailsForm = productionDetails.Data.ToModel<PgProductionDetailsForm>();

            var newProductionDetails = new CustomFormData { Data = JsonConvert.SerializeObject(productionDetails.Data) };
            var newStageDetails = new CustomFormData { Data = JsonConvert.SerializeObject(stageDetails.Data) };
            var costStageModel = currentCost.LatestCostStageRevision.CostStage.StageOrder == 1
                ? await BuildCostFirstStageModel(newStageDetailsForm, userIdentity.Id, costType, stageDetails)
                : null;

            //var newCurrency = await _currencyService.GetCurrency(stageDetailsForm, productionDetailsForm);
            var newCurrency = await _currencyService.GetCurrencyIfChanged(oldStageForm, oldProductionForm, newStageDetailsForm, newProductionDetailsForm);

            var response = new UpdateCostResponse
            {
                Approvals = await GetApprovals(costType, stageDetails, userIdentity.Id, latestRevisionId, costId),
                ProductionDetails = newProductionDetails,
                StageDetails = newStageDetails,
                // Re-generate first stage model only. It can't be changed later
                CurrentCostStageModel = costStageModel,
                NewCurrency = newCurrency,
                DpvSelected = newProductionDetailsForm.DirectPaymentVendor != null && oldProductionForm?.DirectPaymentVendor == null,
                AipeSelected = newStageDetailsForm.IsAIPE && (oldStageForm == null || !oldStageForm.IsAIPE)
            };
            return response;
        }

        public async Task<bool> CanCreateVersion(Guid costId)
        {
            var currentCost = await _efContext.Cost
                .Include(c => c.LatestCostStageRevision)
                    .ThenInclude(s => s.CostStage)
                .Where(c => c.Id == costId)
                .FirstAsync();
            return currentCost.LatestCostStageRevision.CostStage.Key == CostStages.OriginalEstimateRevision.ToString() ||
                   currentCost.LatestCostStageRevision.CostStage.Key == CostStages.FirstPresentationRevision.ToString();
        }

        public CostStageRevision UpdateCostStageDetails(CostStage costStage, CostStageRevision revision)
        {
            var stageDetailsForm = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(revision.StageDetails.Data);
            if (costStage.Cost.CostType == CostType.Buyout && costStage.Key == CostStages.FinalActual.ToString())
            {
                stageDetailsForm[nameof(PgStageDetailsForm.ApprovalStage)] = CostStages.FinalActual.ToString();
            }

            var serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            revision.StageDetails.Data = JsonConvert.SerializeObject(stageDetailsForm, serializationSettings);
            return revision;
        }

        public bool GetIsCostSectionCurrencyLocked(int currentStageOrder, CustomFormData stageDetails)
        {
            // this is called on CreateVersion to determine if the cost sections should have their currencies locked
            // if we are creating a version of the first stage with cost line items (Original Estimate)
            var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(stageDetails.Data);
            return !(currentStageOrder == 1 || (currentStageOrder == 2 && stageDetailsForm.IsAIPE));
        }

        public async Task<IUpdateCostFormResponse> UpdateCostForm(UserIdentity userIdentity, Guid costId, Guid latestRevisionId, CostType costType, IStageDetails stageDetails,
            ICostFormDetails costFormDetails, CustomFormDefinition formDefinition, FormSectionDefinition formSectionDefinition)
        {
            var costFormJson = JObject.FromObject(costFormDetails.Data);
            var response = new UpdateCostFormResponse();

            // validate json

            var sectionFound = false;
            foreach (var node in costFormJson)
            {
                if (node.Value.Type == JTokenType.Object && formSectionDefinition.Label.Equals(node.Key, StringComparison.OrdinalIgnoreCase))
                {
                    sectionFound = true;
                    break;
                }
            }

            if (!sectionFound)
            {
                var errMsg = $"section '{formSectionDefinition.Name}' is missing from form";
                response.Errors.Add(errMsg);
            }


            if (response.Errors.Count == 0)
            {
                if (formDefinition.CostFormDetails.Id == Guid.Empty)
                {
                    response.Details = new CostFormDetails
                    {
                        FormDefinitionId = formSectionDefinition.FormDefinitionId
                    };
                }
                else
                {
                    response.Details = formDefinition.CostFormDetails;
                }

                response.Form = formDefinition.FormDataDetailsId != Guid.Empty ? formDefinition.CustomFormData : new CustomFormData();
            }

            response.Approvals = await GetApprovals(costType, stageDetails, userIdentity.Id, latestRevisionId, costId);

            return response;
        }

        public async Task<List<SupportingDocumentModel>> BuildSupportingDocuments(IStageDetails stageDetails, CostType costType, IEnumerable<string> stageKeys,
            Guid costStageRevisionId, bool totalCostIncreased = false)
        {
            var stageDetailsForm = stageDetails.Data.ToModel<PgStageDetailsForm>();

            var rules = (await _ruleService.GetCompiledByRuleType<SupportingDocumentRule>(RuleType.SupportingDocument)).ToArray();
            var supportingDocs = new List<SupportingDocumentModel>();
            var previousRevision = await _costStageRevisionService.GetPreviousRevision(costStageRevisionId);
            foreach (var stage in stageKeys)
            {
                var supportingDocRule = new SupportingDocumentRule
                {
                    BudgetRegion = stageDetailsForm.BudgetRegion?.Key,
                    ContentType = stageDetailsForm.ContentType?.Key,
                    CostStage = stage,
                    ProductionType = stageDetailsForm.ProductionType?.Key,
                    CostType = costType.ToString(),
                    TotalCostIncreased = totalCostIncreased,
                    PreviousCostStage = previousRevision != null ? previousRevision?.Name : string.Empty
                };
                List<SupportingDocumentModel> output;
                Func<SupportingDocumentRule, Rule, List<SupportingDocumentModel>> matched = (supportingDocumentRule, rule) =>
                {
                    var ruleDefinition = JsonConvert.DeserializeObject<SupportingDocumentRuleDefinition>(rule.Definition);
                    return new List<SupportingDocumentModel>
                    {
                        new SupportingDocumentModel
                        {
                            CanManuallyUpload = ruleDefinition.CanManuallyUpload,
                            Name = ruleDefinition.Name,
                            Key = ruleDefinition.Key ?? string.Empty,
                            Generated = true,
                            Required = ruleDefinition.Mandatory
                        }
                    };
                };

                Func<List<SupportingDocumentModel>, List<SupportingDocumentModel>, List<SupportingDocumentModel>> aggregator = (total, result) =>
                {
                    total.AddRange(result);
                    return total;
                };

                _ruleService.TryMatchRule(rules, supportingDocRule, matched, aggregator, out output);

                if (output != null)
                {
                    supportingDocs.AddRange(output);
                }
            }

            return supportingDocs;
        }

        public async Task<List<ApprovalModel>> GetApprovals(CostType costType, IStageDetails stageDetails, Guid userId, Guid costStageRevisionId, Guid costId)
        {
            var costStageKey = await _efContext.CostStageRevision
                .Where(csr => csr.Id == costStageRevisionId)
                .Select(c => c.CostStage.Key)
                .FirstAsync();

            var stageDetailsForm = stageDetails.Data.ToModel<PgStageDetailsForm>();
            var currentTotals = await GetTotals(costStageRevisionId);
            var costTotalIncreased = true;
            var previousRevision = await _costStageRevisionService.GetPreviousRevision(costStageRevisionId);

            // Always need brand approval if prvious stage is aipe  https://jira.adstream.com/browse/ADC-1585
            if (previousRevision != null && previousRevision.Name != CostStages.Aipe.ToString())
            {
                var previousTotals = await GetTotals(previousRevision.Id);
                // rounding to account for decimal precision differences during currency conversion
                if (Math.Round(currentTotals.TotalCostAmountTotal, 2) <= Math.Round(previousTotals.TotalCostAmountTotal, 2))
                {
                    costTotalIncreased = false;
                }
            }

            // we only perform a "soft" calculation, withput saving it to the DB
            //var totalPayments = await _paymentService.GetPaymentAmount(costStageRevisionId, false);
            var brandApprovalEnabled = costTotalIncreased;

            var agency = await _agencyService.GetAgencyByCostId(costId);
            var agencyIsCyclone = agency?.IsCyclone() ?? false;

            var approvalRule = new PgApprovalRule
            {
                ProductionType = stageDetailsForm.ProductionType?.Key,
                CostType = costType.ToString(),
                BudgetRegion = stageDetailsForm.BudgetRegion?.Key,
                ContentType = stageDetailsForm.ContentType?.Key,
                TotalCostAmount = currentTotals.TotalCostAmountTotal,
                IsCyclone = agencyIsCyclone
            };

            var rules = await _ruleService.GetCompiledByRuleType<PgApprovalRule>(RuleType.ApprovalRule);
            var costStage = (CostStages)Enum.Parse(typeof(CostStages), costStageKey);
            var isContractCostOnNa = stageDetailsForm?.UsageBuyoutType?.Key == Constants.UsageBuyoutType.Contract &&
                                     stageDetailsForm.BudgetRegion?.Key == Constants.BudgetRegion.NorthAmerica;
            Func<PgApprovalRule, Rule, ApprovalRule> matchFunc = (t, r) =>
            {
                var ruleFlags = JsonConvert.DeserializeObject<PgApprovalRuleDefinition>(r.Definition);

                // https://jira.adstream.com/browse/ADC-1152
                var ipmEnabled = false;
                var ccEnabled = false;
                if (costStage != CostStages.Aipe && !isContractCostOnNa)
                {
                    ipmEnabled = ruleFlags.IpmApprovalEnabled;
                    ccEnabled = ruleFlags.CostConsultantIpmAllowed;
                }

                // https://jira.adstream.com/browse/ADC-812
                var brandApprovalRequired = false;
                if (brandApprovalEnabled)
                {
                    brandApprovalRequired = ruleFlags.BrandApprovalEnabled;
                }

                var res = new ApprovalRule
                {
                    CostType = costType,
                    ContentType = t.ContentType,
                    BudgetRegion = t.BudgetRegion,
                    TotalCostAmount = t.TotalCostAmount,
                    BrandApprovalEnabled = brandApprovalRequired,
                    CostConsultantIpmAllowed = ccEnabled,
                    IpmApprovalEnabled = ipmEnabled,
                    HasExternalIntegration = ruleFlags.HasExternalIntegration
                };

                return res;
            };

            _ruleService.TryMatchRule(rules, approvalRule, matchFunc, out var fResult);

            return fResult != null ? fResult.Approvals() : new List<ApprovalModel>();
        }

        public async Task<Decimal?> GetTargetBudget(CostStageRevision revision)
        {
            var stageDetails = await _efContext.CustomFormData.FirstOrDefaultAsync(form => form.Id == revision.StageDetailsId);
            var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(stageDetails.Data);
            return stageDetailsForm.InitialBudget;
        }

        public async Task UpdatePaymentCurrency(Cost cost)
        {
            Debug.Assert(cost.LatestCostStageRevisionId != null, "cost.LatestCostStageRevisionId != null");

            var paymentCurrency = await _pgPaymentService.GetPaymentCurrency(cost.LatestCostStageRevisionId.Value);
            cost.PaymentCurrencyId = paymentCurrency.Id;
        }

        private async Task<CostStageModel> BuildCostFirstStageModel(PgStageDetailsForm stageDetailsForm, Guid userId, CostType costType, IStageDetails stageDetails)
        {
            var stages = await GetStages(stageDetailsForm, costType);
            var firstStage = GetFirstStage(stageDetails, stageDetailsForm, stages);

            var stagesToBuildDocs = GetStagesToBuildDocs(stages, firstStage.Key, firstStage.Key);

            return new CostStageModel
            {
                Key = firstStage.Key,
                Name = firstStage.Name,
                Order = 1,
                Revisions = new[]
                {
                    new CostStageRevisionModel
                    {
                        Name = firstStage.Key,
                        Status = CostStageRevisionStatus.Draft,
                        StageDetails = JsonConvert.SerializeObject(stageDetails.Data),
                        SupportingDocuments = await BuildSupportingDocuments(stageDetails, costType, stagesToBuildDocs, Guid.Empty)
                    }
                }
            };
        }

        private static StageModel GetFirstStage(IStageDetails stageDetails, PgStageDetailsForm stageDetailsForm, Dictionary<string, StageModel> stages)
        {
            // if AIPE is applicable & selected, the initial stage is AIPE (1), otherwise - Original Estimate (2)
            var firstStageKey = stageDetailsForm.IsAIPE
                ? CostStages.Aipe.ToString()
                : !string.IsNullOrEmpty(stageDetailsForm.ApprovalStage)
                    ? stageDetailsForm.ApprovalStage
                    : CostStages.OriginalEstimate.ToString();

            if (!ValidateStage(stages, CostStages.New.ToString(), firstStageKey))
            {
                throw new ValidationException($"{firstStageKey} can't be first stage for {JsonConvert.SerializeObject(stageDetails.Data)}");
            }

            return stages[firstStageKey];
        }

        private async Task<Dictionary<string, StageModel>> GetStages(PgStageDetailsForm stageDetailsForm, CostType costType)
        {
            var testStageRule = new PgStageRule
            {
                BudgetRegion = stageDetailsForm.BudgetRegion?.Key,
                ContentType = stageDetailsForm.ContentType?.Key,
                ProductionType = stageDetailsForm.ProductionType?.Key,
                CostType = costType.ToString(),
                TargetBudgetAmount = stageDetailsForm.InitialBudget.GetValueOrDefault()
            };
            var stages = await _pgStageBuilder.GetStages(testStageRule);
            return stages;
        }

        private static List<string> GetStagesToBuildDocs(Dictionary<string, StageModel> stages, string firstStageKey, string currentStageKey)
        {
            var stagesToBuildDocs = new List<string>();
            if (currentStageKey == firstStageKey)
            {
                //ADC-533 - Fast Track to Final Actual Approval Stage
                // Get documents for all skipped stages
                stagesToBuildDocs.AddRange(stages.TakeWhile(kv => kv.Key != currentStageKey).Where(x => x.Value.IsRequired).Select(kv => kv.Key));
            }

            //Add current stage
            stagesToBuildDocs.Add(currentStageKey);

            return stagesToBuildDocs;
        }

        private static bool ValidateStage(Dictionary<string, StageModel> stages, string currentStage, string targetStage)
        {
            return stages.ContainsKey(currentStage) && stages[currentStage].Transitions.ContainsKey(targetStage);
        }

        private async Task<CostSectionTotals> GetTotals(Guid revisionId)
        {
            var revision = await _efContext.CostStageRevision.Include(x => x.CostStage)
                .FirstOrDefaultAsync(x => x.Id == revisionId);

            var costLineItems = await _costStageRevisionService.GetCostLineItems(revisionId);

            var stageDetailsForm = await _costStageRevisionService.GetStageDetails<PgStageDetailsForm>(revisionId);
            return _pgTotalsBuilder.Build(stageDetailsForm, costLineItems, revision.CostStage.Key);
        }

        private async Task SetPaymentCurrencyRate(Cost cost)
        {
            Debug.Assert(cost.PaymentCurrencyId != null, "cost.PaymentCurrencyId != null");

            var currentExchangeRate = await _exchangeRateService.GetCurrentRate(cost.PaymentCurrencyId.Value);
            cost.ExchangeRateDate = DateTime.UtcNow;
            cost.ExchangeRate = currentExchangeRate.Rate;
        }
    }
}
