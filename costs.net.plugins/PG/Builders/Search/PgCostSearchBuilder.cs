namespace costs.net.plugins.PG.Builders.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core;
    using core.Builders;
    using core.Builders.Response;
    using core.Builders.Search;
    using core.Extensions;
    using core.Models;
    using core.Services;
    using dataAccess;
    using dataAccess.Entity;
    using Form;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query;
    using Models;
    using Nest;
    using Newtonsoft.Json;
    using Serilog;

    public class PgCostSearchBuilder : ICostSearchBuilder
    {
        private readonly IEnumerable<Lazy<ICostBuilder, PluginMetadata>> _costBuilders;
        private readonly EFContext _efContext;
        private readonly ILogger _logger = Log.ForContext<PgCostSearchBuilder>();
        private readonly IMapper _mapper;
        private readonly IPermissionService _permissionService;

        public PgCostSearchBuilder(IMapper mapper,
            EFContext efContext,
            IEnumerable<Lazy<ICostBuilder, PluginMetadata>> costBuilder,
            IPermissionService permissionService)
        {
            _mapper = mapper;
            _efContext = efContext;
            _costBuilders = costBuilder;
            _permissionService = permissionService;
        }

        private static Func<CreateIndexDescriptor, ICreateIndexRequest> CostsSelector =>
            i => i.Settings(s => s
                    .Analysis(a => a
                        .Normalizers(n => n
                            .Custom("ads_lowercase_normalizer", d => d.Filters("lowercase", "asciifolding")))
                    )
                )
                .Mappings(m =>
                    m.Map<CostSearchItem>(mm =>
                        mm.Properties(p => p
                            .Keyword(k => k.Name(n => n.Id))
                            .Keyword(k => k.Name(n => n.Title))
                            .Number(k => k.Name(n => n.Budget).Type(NumberType.Double))
                            .Keyword(k => k.Name(n => n.AgencyTrackingNumber))
                            .Keyword(k => k.Name(n => n.ContentType))
                            .Text(k => k.Name(n => n.ContentTypeValue))
                            .Text(k => k.Name(n => n.AgencyProducer))
                            .Text(am => am.Name(s => s.CostOwner).Fielddata().Fields(SetMultiField))
                            .Keyword(k => k.Name(n => n.Stage))
                            .Keyword(k => k.Name(n => n.StageKey))
                            .Keyword(k => k.Name(n => n.ApprovalStatus))
                            .Keyword(k => k.Name(n => n.ProjectId))
                            .Text(k => k.Name(n => n.IoNumber).Fields(SetMultiField))
                            .Keyword(k => k.Name(n => n.BrandId))
                            .Keyword(k => k.Name(n => n.CostNumber))
                            .Keyword(k => k.Name(n => n.Campaign))
                            .Keyword(k => k.Name(n => n.CostType))
                            .Keyword(k => k.Name(n => n.Initiatives))
                            .Keyword(k => k.Name(n => n.ProductionType))
                            .Text(k => k.Name(n => n.ProductionTypeValue))
                            .Keyword(k => k.Name(n => n.AgencyId))
                            .Text(k => k.Name(n => n.AgencyName).Fields(SetMultiField))
                            .Keyword(k => k.Name(n => n.BudgetRegion))
                            .Text(k => k.Name(n => n.BudgetRegionName))
                            .Keyword(k => k.Name(n => n.Country))
                            .Keyword(k => k.Name(n => n.City))
                            .Keyword(k => k.Name(n => n.CreatedBy))
                            .Date(k => k.Name(n => n.CreatedDate))
                            .Date(k => k.Name(n => n.ModifiedDate))
                            .Date(k => k.Name(n => n.UserModifiedDate))
                            .Keyword(k => k.Name(n => n.UserGroups))
                            .Keyword(d => d.Name(s => s.Status))
                            .Keyword(d => d.Name(s => s.UsageBuyoutType))
                            .Keyword(d => d.Name(s => s.LatestRevisionId))
                            .Keyword(k => k.Name(n => n.OwnerId))
                            .Nested<Approver>(a => a
                                .Name(c => c.ApprovalMembers)
                                .Properties(ps => ps
                                    .Keyword(am => am.Name(s => s.CostUserId))
                                    .Text(am => am.Name(s => s.Name).Fielddata().Fields(SetMultiField))
                                    .Keyword(am => am.Name(s => s.Role))
                                    .Keyword(am => am.Name(s => s.Status))
                                )
                            )
                        )
                    )
                );

        public IEnumerable<IndexDescriptor> GetIndexDescriptors()
        {
            return new List<IndexDescriptor>
            {
                new IndexDescriptor
                {
                    Alias = Constants.ElasticSearchIndices.CostsIndexName,
                    Selector = CostsSelector
                }
            };
        }

        public async Task<CostSearchItem> GetCostSearchItem(Guid costId)
        {
            var costBuilder = _costBuilders.First(s => s.Metadata.BuType == BuType.Pg).Value;
            var cost = await GetCostDataQuery()
                .FirstOrDefaultAsync(c => c.Id == costId);

            if (cost == null)
            {
                return null;
            }
            try
            {
                return await CostSearchItem(cost, costBuilder);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error building cost:{cost.Id}");
                throw;
            }
        }

        public async Task<List<CostSearchItem>> GetCostSearchItems(List<Guid> costIds)
        {
            var costSearchItems = new List<CostSearchItem>();

            var costBuilder = _costBuilders.First(s => s.Metadata.BuType == BuType.Pg).Value;
            var costs = await GetCostDataQuery()
                .Where(c => costIds.Contains(c.Id)).ToListAsync();

            if (!costs.Any())
            {
                return costSearchItems;
            }

            await costs.ForEachAsync(async cost =>
            {
                try
                {
                    var costSearchItem = await CostSearchItem(cost, costBuilder);
                    costSearchItems.Add(costSearchItem);
                }
                catch (Exception e)
                {
                    _logger.Error(e, $"Error building cost:{cost.Id}");
                }
            });

            return costSearchItems;
        }

        private async Task<CostSearchItem> CostSearchItem(Cost cost, ICostBuilder costBuilder)
        {
            Currency currency = null;
            var customObjectFormData =
                await _efContext.CustomObjectData.FirstOrDefaultAsync(
                    cofd => cofd.ObjectId == cost.LatestCostStageRevision.Id && cofd.Name == CustomObjectDataKeys.PgPaymentDetails);

            var stageDetailsForm = JsonConvert.DeserializeObject<PgStageDetailsForm>(cost.LatestCostStageRevision.StageDetails.Data);

            var productionDetailsForm = new PgProductionDetailsForm();
            if (cost.LatestCostStageRevision.ProductDetails != null)
            {
                productionDetailsForm = JsonConvert.DeserializeObject<PgProductionDetailsForm>(cost.LatestCostStageRevision.ProductDetails.Data);
            }

            PgPaymentDetails paymentDetails = null;
            if (customObjectFormData?.Data != null)
            {
                paymentDetails = JsonConvert.DeserializeObject<PgPaymentDetails>(customObjectFormData.Data);
            }

            var approvers =
                cost.LatestCostStageRevision.Approvals
                    .SelectMany(a => a.ApprovalMembers)
                    .Where(am => !am.IsExternal)
                    .Select(am => new Approver
                    {
                        CostUserId = am.CostUser.Id,
                        Name = $"{am.CostUser.FirstName} {am.CostUser.LastName}",
                        Role = am.CostUser.UserBusinessRoles?.FirstOrDefault()?.BusinessRole?.Value ?? "Sap Approver",
                        Status = am.Status.ToString()
                    });

            if (!string.IsNullOrEmpty(stageDetailsForm.AgencyCurrency))
            {
                currency = await _efContext.Currency.FirstOrDefaultAsync(c => c.Code == stageDetailsForm.AgencyCurrency);
            }

            if (productionDetailsForm?.DirectPaymentVendor?.CurrencyId != null)
            {
                currency = await _efContext.Currency.FirstOrDefaultAsync(c => c.Id == productionDetailsForm.DirectPaymentVendor.CurrencyId);
            }

            if (currency == null)
            {
                currency = await _efContext.Currency.FirstOrDefaultAsync(c => c.DefaultCurrency);
            }

            var costSearchItem = _mapper.Map<CostSearchItem>(cost);
            var totals = await costBuilder.GetRevisionTotals(cost.LatestCostStageRevision.Id);
            costSearchItem.GrandTotal = totals.totalInLocalCurrency;
            costSearchItem.GrandTotalDefaultCurrency = totals.total;
            costSearchItem.ApprovalMembers = new List<Approver>();
            costSearchItem.ApprovalMembers.AddRange(approvers);
            costSearchItem.LatestRevisionId = cost.LatestCostStageRevision.Id;

            costSearchItem.UserGroups = (await _permissionService.GetObjectUserGroups(cost.Id, null)).ToList();
            costSearchItem.BrandId = cost.Project.BrandId.ToString();
            costSearchItem.AgencyId = cost.ParentId.ToString();
            costSearchItem.IoNumber = paymentDetails?.IoNumber;
            costSearchItem.Initiatives = cost.LatestCostStageRevision.ExpectedAssets?.Select(a => a.Initiative).Distinct().ToList();
            _mapper.Map(stageDetailsForm, costSearchItem);
            costSearchItem.Stage = cost.LatestCostStageRevision.CostStage.Name;
            costSearchItem.StageKey = cost.LatestCostStageRevision.CostStage.Key;

            return _mapper.Map(currency, costSearchItem);
        }

        private IIncludableQueryable<Cost, Project> GetCostDataQuery()
        {
            return _efContext.Cost
                .Include(c => c.CreatedBy)
                .ThenInclude(c => c.Agency)
                .Include(c => c.Owner)
                .ThenInclude(c => c.Agency)
                .Include(c => c.LatestCostStageRevision.CostStage)
                .Include(c => c.LatestCostStageRevision.ProductDetails)
                .Include(c => c.LatestCostStageRevision.StageDetails)
                .Include(c => c.LatestCostStageRevision.ExpectedAssets)
                .Include(c => c.LatestCostStageRevision.Approvals)
                .ThenInclude(a => a.ApprovalMembers)
                .ThenInclude(am => am.CostUser)
                .ThenInclude(u => u.UserBusinessRoles)
                .ThenInclude(ubr => ubr.BusinessRole)
                .Include(c => c.Project);
        }

        private static PropertiesDescriptor<T> SetMultiField<T>(PropertiesDescriptor<T> f) where T : class
        {
            return f.Text(tt => tt.Name("row").Fielddata())
                .Keyword(kk => kk.Name("keyword"));
        }
    }
}
