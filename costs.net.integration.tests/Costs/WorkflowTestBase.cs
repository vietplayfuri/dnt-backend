namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using CostType = core.Models.CostTemplate.CostType;
    using dataAccess.Entity;
    using Builders;
    using core.Models.ACL;
    using core.Models.Approvals;
    using core.Models.Response;
    using core.Models.Costs;
    using core.Models.Workflow;
    using Newtonsoft.Json;
    using plugins.PG.Models;
    using plugins.PG.Models.PurchaseOrder;

    [TestFixture]
    public abstract class WorkflowTestBase : BaseCostIntegrationTest
    {
        protected CostUser AdminUser;
        private CostUser _approver;

        protected CostUser Approver => _approver ?? (_approver = CreateUser($"{Guid.NewGuid()}approver", Roles.CostApprover).Result);

        [SetUp]
        public async Task InitWorkflowTest()
        {
            AdminUser = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
        }

        protected async Task SetApproverAuthLimit(int authLimit)
        {
            var user = await EFContext.CostUser.FindAsync(Approver.Id);
            if (user != null)
            {
                user.ApprovalLimit = authLimit;
                await EFContext.SaveChangesAsync();
            }
        }

        protected async Task<Cost> CreateCost(CreateCostModelBuilder createCostModelBuilder, CostUser user, CostType costType = CostType.Production)
        {
            if (!Enum.IsDefined(typeof(CostType), costType))
            {
                throw new ArgumentOutOfRangeException(nameof(costType), "Value should be defined in the CostType enum.");
            }

            if (costType == CostType.Production && CostTemplate == null)
            {
                CostTemplate = await CreateTemplate(user, costType);
            }
            if (costType == CostType.Buyout && UsageCostTemplate == null)
            {
                UsageCostTemplate = await CreateTemplate(user, costType);
            }

            var costTemplateId = costType == CostType.Production ? CostTemplate.Id : UsageCostTemplate.Id;
            createCostModelBuilder
                .WithTemplateId(costTemplateId)
                .WithAgency(user.Agency.AbstractTypes.FirstOrDefault());

            var createCostResult = await CreateCost(user, createCostModelBuilder.Build());

            var cost = Deserialize<Cost>(createCostResult, HttpStatusCode.Created);

            var latestStage = await GetCostLatestStage(cost.Id, user);
            await GetCostLatestRevision(cost.Id, latestStage.Id, user);

            return cost;
        }

        protected async Task AddApprover(Cost cost, CostUser owner, CostUser approver)
        {
            var updateModel = new UpdateCostModel
            {
                ProductionDetails = new ProductionDetail
                {
                    Data = new Dictionary<string, dynamic>
                        {
                            { "type", "AIPE" },
                            { "talentCompanies", new[] { "TalentCo1", "TalentCo2" } }
                        }
                }
            };

            // Cost update triggers generation of Approval sections
            Deserialize<OperationResponse>(await Browser.Put($"/v1/costs/{cost.Id}", w =>
            {
                w.User(owner);
                w.JsonBody(updateModel);
            }), HttpStatusCode.OK);



            // Now add approval member
            var latestStage = await GetCostLatestStage(cost.Id, owner);
            var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, owner);
            var currency = await GetDefaultCurrency();
            var lineItemSection = await EFContext.CostLineItemSectionTemplate.FirstOrDefaultAsync(ts => ts.Name == plugins.Constants.CostSection.Production);
            await CreateCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, new List<CostLineItemModel>
                {
                    new CostLineItemModel
                    {
                        Name = "Item1",
                        Value = 1000000,
                        LocalCurrencyId = currency.Id,
                        TemplateSectionId = lineItemSection.Id
                    }
                },
                User);

            var approvalsUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);

            var approvals = new[]
            {
                    new ApprovalModel
                    {
                        Status = ApprovalStatus.New,
                        Type = ApprovalType.IPM,
                        ValidBusinessRoles = new[] { "s" },
                        ApprovalMembers = new List<ApprovalModel.Member>
                        {
                            new ApprovalModel.Member
                            {
                                Id = approver.Id,
                                Email = approver.Email
                            }
                        }
                    },
                    new ApprovalModel
                    {
                        Status = ApprovalStatus.New,
                        Type = ApprovalType.Brand,
                        ApprovalMembers = new List<ApprovalModel.Member>(),
                        Requisitioners = new List<RequisitionerModel>
                        {
                            new RequisitionerModel
                            {
                                Id = approver.Id,
                                Email = approver.Email
                            }
                        }
                    }
                };

            await Browser.Post(approvalsUrl, r =>
            {
                r.User(owner);
                r.JsonBody(approvals);
            });
        }
        
        protected async Task<ExchangeRate> CreateExchangeRate(Currency currency, decimal rate = (decimal)1.0)
        {
            // Update line items
            var defaultCurrency = await GetDefaultCurrency();
            var exchangeRate = new ExchangeRate
            {
                AbstractTypeId = (await GetRootModule()).Id,
                EffectiveFrom = DateTime.UtcNow,
                FromCurrency = currency.Id,
                ToCurrency = defaultCurrency.Id,
                Rate = rate,
                RateName = "Test",
                RateType = "Test"
            };
            EFContext.ExchangeRate.Add(exchangeRate);
            await EFContext.SaveChangesAsync();

            return exchangeRate;
        }

        protected new async Task ExecuteAction(Guid costId, CostAction action, CostUser user)
        {
            var url = $"{CostWorkflowUrl(costId)}/actions";
            var browserResponse = await Browser.Post(url, w =>
            {
                w.User(user);
                w.JsonBody(new ExecuteActionModel
                {
                    Action = action
                });
            });
            Deserialize<object>(browserResponse, HttpStatusCode.OK);
        }

        protected async Task AddPONumber(Cost cost, string poNumber = "PO_defaul_123")
        {
            EFContext.CustomObjectData.Add(new CustomObjectData
            {
                ObjectId = cost.LatestCostStageRevisionId.Value,
                Name = CustomObjectDataKeys.PgPurchaseOrderResponse,
                Data = JsonConvert.SerializeObject(new PgPurchaseOrderResponse
                {
                    PoNumber = poNumber
                })
            });

            await EFContext.SaveChangesAsync();
        }

        protected Guid GetLatestRevisionId(Guid costId)
        {
            var dbCost = EFContext.Cost.Find(costId);
            EFContext.Entry(dbCost).Reload();
            return dbCost.LatestCostStageRevisionId.Value;
        }
    }
}
