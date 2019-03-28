namespace costs.net.integration.tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Browser;
    using core.Builders;
    using core.Models.ACL;
    using core.Models.Costs;
    using core.Models.CostTemplate;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using CostType = core.Models.CostTemplate.CostType;
    using core.Models.Response;
    using core.Models.Workflow;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Models.Stage;

    [TestFixture]
    public class BaseCostIntegrationTest : BaseIntegrationTest
    {
        protected CostUser User { get; set; }
        //protected AbstractType UserAgency { get; set; }
        protected string CostWorkflowUrl(Guid costId) => $"/v1/costs/{costId}/workflow";
        protected string CostRevisionWorkflowUrl(Guid costId, Guid revisionId) => $"/v1/costs/{costId}/revisions/{revisionId}/workflow";
        protected CostTemplateDetailsModel CostTemplate { get; set; }
        protected CostTemplateDetailsModel TrafficCostTemplate { get; set; }
        protected CostTemplateDetailsModel UsageCostTemplate { get; set; }

        [SetUp]
        public async Task Init()
        {
            User = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);
        }

        protected async Task<Cost> CreateCostEntity(CostUser owner)
        {
            if (CostTemplate == null)
            {
                CostTemplate = await CreateTemplate(owner);
            }

            var videoContentTypeId = EFContext.DictionaryEntry
                .First(de =>
                    de.Dictionary.Name == plugins.Constants.DictionaryNames.ContentType
                    && de.Key == plugins.Constants.ContentType.Video).Id;

            var createCostResult = await CreateCost(owner, new CreateCostModel
            {
                TemplateId = CostTemplate.Id,
                StageDetails = new StageDetails
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        BudgetRegion = new AbstractTypeValue
                        {
                            Key = plugins.Constants.BudgetRegion.AsiaPacific,
                            Name = plugins.Constants.BudgetRegion.AsiaPacific
                        },
                        ContentType = new DictionaryValue { Id = videoContentTypeId, Value = "Video", Key = plugins.Constants.ContentType.Video },
                        ProductionType = new DictionaryValue { Id = Guid.NewGuid(), Value = "Full Production", Key = plugins.Constants.ProductionType.FullProduction },
                        ProjectId = "123456789",
                        ApprovalStage = "OriginalEstimate",
                        Agency = new PgStageDetailsForm.AbstractTypeAgency
                        {
                            Id = owner.AgencyId,
                            AbstractTypeId = owner.Agency.AbstractTypes.First().Id,
                            Name = owner.Agency.Name
                        }
                    }))
                }
            });

            return Deserialize<Cost>(createCostResult, HttpStatusCode.Created);
        }

        protected async Task<Cost> CreateDistributionCostEntity(CostUser owner, string costTitle = "Trafficking")
        {
            TrafficCostTemplate = await CreateTrafficTemplate(owner);

            var createCostResult = await CreateCost(owner, new CreateCostModel
            {
                TemplateId = TrafficCostTemplate.Id,
                StageDetails = new StageDetails
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        BudgetRegion = new AbstractTypeValue { Key = plugins.Constants.BudgetRegion.IndiaAndMiddleEastAfrica },
                        CostType = CostType.Trafficking.ToString(),
                        ProjectId = "123456789",
                        ApprovalStage = CostStages.OriginalEstimate.ToString(),
                        InitialBudget = 50000m,
                        Description = CostType.Trafficking.ToString(),
                        IsAIPE = false,
                        AgencyCurrency = "USD",
                        AirInsertionDate = DateTime.Now.AddDays(30),
                        Agency = new PgStageDetailsForm.AbstractTypeAgency
                        {
                            Id = owner.AgencyId,
                            AbstractTypeId = owner.Agency.AbstractTypes.First().Id,
                            Name = owner.Agency.Name
                        },
                        Title = costTitle
                    }))
                }
            });

            return Deserialize<Cost>(createCostResult, HttpStatusCode.Created);
        }

        protected async Task<Cost> CreateUsageCostEntity(CostUser owner)
        {
            if (UsageCostTemplate == null)
            {

                UsageCostTemplate = await CreateUsageTemplate(owner);
            }

            var createCostResult = await CreateCost(owner, new CreateCostModel
            {
                TemplateId = UsageCostTemplate.Id,
                StageDetails = new StageDetails
                {
                    Data = new Dictionary<string, dynamic>
                    {
                        { "budgetRegion", new AbstractTypeValue {Key = "AAK (Asia)" }},
                        { "isNewBuyout", "true" },
                        { "isAIPE", false },
                        { "initialBudgetCurrencySymbol", "$" },
                        { "IsCurrencyChanged", false },
                        { "initialBudget", 12312313},
                        { "agencyTrackingNumber", "Creating Buyout"},
                        { "targetBudget", "<10000" },
                        { "projectId", "123456789" },
                        { "approvalStage", "OriginalEstimate" },
                        { "organisation", new DictionaryValue { Key = "Other" } },
                        { "usageType", new { key = "Celebrity" }},
                        { "usageBuyoutType", new DictionaryValue { Key = "Buyout" } },
                        { "title", "Creating Buyout"},
                        { "description", "Creating Buyout" },
                        { "agencyProducer",new [] { "Agency Producer 2" } },
                        { "agency", new PgStageDetailsForm.AbstractTypeAgency
                            {
                                Id = owner.AgencyId,
                                AbstractTypeId = owner.Agency.AbstractTypes.First().Id,
                                Name = owner.Agency.Name
                            }
                        }
                    }
                }
            });

            return Deserialize<Cost>(createCostResult, HttpStatusCode.Created);
        }

        protected async Task<CostTemplateDetailsModel> CreateUsageTemplate(CostUser user)
        {
            var basePath = AppContext.BaseDirectory;
            var filePath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}UsageTemplate.json";
            var usageTemplate = await JsonReader.GetObject<CreateCostTemplateModel>(filePath);

            var result = await Browser.Post("/v1/costtemplate/create", with =>
            {
                with.User(user);
                with.JsonBody(usageTemplate);
            });

            return Deserialize<CostTemplateDetailsModel>(result, HttpStatusCode.Created);
        }

        protected async Task<BrowserResponse> CreateCost(CostUser user, CreateCostModel costModel)
        {
            return await Browser.Post("/v1/costs", w =>
            {
                w.User(user);
                w.JsonBody(costModel);
            });
        }

        protected async Task<CostStage> GetCostLatestStage(Guid costId, CostUser user)
        {
            var url = $"/v1/costs/{costId}/stage/latest";
            var response = await Browser.Get(url, w => w.User(user));
            return Deserialize<CostStage>(response, HttpStatusCode.OK);
        }

        protected async Task<CostStageRevision> GetCostLatestRevision(Guid costId, Guid costStageId, CostUser user)
        {
            var url = $"/v1/costs/{costId}/stage/{costStageId}/revision/latest";
            var response = await Browser.Get(url, w => w.User(user));
            return Deserialize<CostStageRevision>(response, HttpStatusCode.OK);
        }

        protected async Task<IEnumerable<CostLineItem>> GetCostLineItems(Guid costId, Guid costStageId, Guid costStageRevisionId, CostUser user)
        {
            var url = CostLineItemsUrl(costId, costStageId, costStageRevisionId);
            var response = await Browser.Get(url, w => w.User(user));
            return Deserialize<IEnumerable<CostLineItem>>(response, HttpStatusCode.OK);
        }

        protected async Task<OperationResponse> CreateCostLineItems(Guid costId, Guid costStageId, Guid costStageRevisionId, IEnumerable<CostLineItemModel> lineItems, CostUser owner)
        {
            var url = CostLineItemsUrl(costId, costStageId, costStageRevisionId);
            var updateLineItemsResult = await Browser.Put(url, w =>
            {
                w.User(owner);
                w.JsonBody(new UpdateCostLineItemsModel
                {
                    CostLineItemData = new List<CostLineItemModel>(lineItems)
                });
            });

            var updateLineItemsResponse = Deserialize<OperationResponse>(updateLineItemsResult, HttpStatusCode.OK);

            return updateLineItemsResponse;
        }

        protected Task<dataAccess.Entity.Currency> GetCurrencyByCode(string code)
        {
            return EFContext.Currency.FirstOrDefaultAsync(c => c.Code == code);
        }

        protected Task<dataAccess.Entity.Currency> GetDefaultCurrency()
        {
            return EFContext.Currency.FirstOrDefaultAsync(c => c.DefaultCurrency);
        }

        protected string SaveCustomDataUrl(Guid revisionId, string name) => $"/v1/custom-data/{revisionId}/{name}";

        protected string CostLineItemsUrl(Guid costId, Guid costStageId, Guid costStageRevisionId) => $"/v1/costs/{costId}/stage/{costStageId}/revision/{costStageRevisionId}/line-item";

        protected string ExpectedAssetsUrl(Guid costId, Guid costStageId, Guid costStageRevisionId) => $"/v1/costs/{costId}/stage/{costStageId}/revision/{costStageRevisionId}/expected-assets";

        protected string ApprovalsUrl(Guid costId, Guid costStageId, Guid costStageRevisionId) => $"/v1/costs/{costId}/stage/{costStageId}/revision/{costStageRevisionId}/approvals";

        protected string ActionUrl(Guid costId) => $"v1/costs/{costId}/workflow/actions";

        protected string SupportingDocumentsUrl(Guid costId, Guid costStageId, Guid costStageRevisionId) => $"/v1/costs/{costId}/stage/{costStageId}/revision/{costStageRevisionId}/supporting-documents";


        protected async Task<CostTemplateDetailsModel> CreateTrafficTemplate(CostUser user, CostType costType = CostType.Trafficking)
        {
            var model = new CreateCostTemplateModel
            {
                Name = "costDetails",
                Type = costType,
                CostDetails = new CostTemplateModel
                {
                    Name = "costDetails",
                    Label = "Cost Details",
                    Fields = new List<FormFieldDefintionModel>
                    {
                        new FormFieldDefintionModel
                        {
                            Label = "Agency Name",
                            Name = "agencyName",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Agency Location",
                            Name = "agencyLocation",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Agency producer/art buyer",
                            Name = "agencyProducerArtBuyer",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Budget Region",
                            Name = "budgetRegion",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Target Budget",
                            Name = "targetBudget",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Agency Tracking Number",
                            Name = "contentType",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Organisation",
                            Name = "organisation",
                            Type = "string"
                        },
                        new FormFieldDefintionModel
                        {
                            Label = "Agency Currency",
                            Name = "agencyCurrency",
                            Type = "string"
                        }
                    }
                },
                ProductionDetails =
                    new[]
                    {
                        new ProductionDetailsTemplateModel
                        {
                            Type = "Trafficking",
                            Forms = new List<ProductionDetailsFormDefinitionModel>
                            {
                                new ProductionDetailsFormDefinitionModel
                                {
                                    Name = "Trafficking",
                                    Label = "Trafficking",
                                    Fields = new List<FormFieldDefintionModel>(),
                                    CostLineItemSections = new List<CostLineItemSectionTemplateModel>
                                    {
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "distributionCosts",
                                            Label = "Distribution",
                                            CurrencyLabel = "Trafficking Distribution Currency",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Trafficking/Distribution Costs",
                                                    Name = "distributionCosts"
                                                }
                                            }
                                        },
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "OtherCosts",
                                            Label = "Other",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Tax (if applicable)",
                                                    Name = "taxIfApplicable"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Technical Fee (when applicable)",
                                                    Name = "technicalFee",
                                                    ReadOnly = true
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "FX (Loss) and Gain",
                                                    Name = "foreignExchange"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },

            };

            var result = await Browser.Post("/v1/costtemplate/create", with =>
            {
                with.User(user);
                with.JsonBody(model);
            });

            return Deserialize<CostTemplateDetailsModel>(result, HttpStatusCode.Created);
        }
        protected async Task<CostTemplateDetailsModel> CreateTemplate(CostUser user, CostType costType = CostType.Production)
        {
            var model = new CreateCostTemplateModel
            {
                Name = "Production Cost",
                Type = costType,

                CostDetails = new CostTemplateModel
                {
                    Name = "cost",
                    Label = "Cost Details",
                    Fields = new List<FormFieldDefintionModel>
                    {
                        new FormFieldDefintionModel
                        {
                            Label = "Cost Number",
                            Name = "costNumber",
                            Type = "string"
                        }
                    }
                },
                Forms = new[]
                {
                    new FormDefinitionModel
                    {
                        Name = "Test",
                        Label = "test",
                        Fields = new List<FormFieldDefintionModel>
                        {
                            new FormFieldDefintionModel
                            {
                                Name = "Test field one",
                                Label = "testFieldOne",
                                Type = "string"
                            }
                        },
                        Sections = new List<FormSectionDefinitionModel>
                        {
                            new FormSectionDefinitionModel
                            {
                                Name = "Section",
                                Label = "section",
                                Fields = new List<FormFieldDefintionModel>
                                {
                                    new FormFieldDefintionModel
                                    {
                                        Name = "Section one",
                                        Label = "sectionOne",
                                        Type = "string"
                                    }
                                }
                            }
                        }
                    }
                },
                ProductionDetails =
                    new[]
                    {
                        new ProductionDetailsTemplateModel
                        {
                            Type = "video",
                            Forms = new List<ProductionDetailsFormDefinitionModel>
                            {
                                new ProductionDetailsFormDefinitionModel
                                {
                                    Name = "fullProductionWithShoot",
                                    Label = "Full Production",
                                    Fields = new List<FormFieldDefintionModel>
                                    {
                                        new FormFieldDefintionModel
                                        {
                                            Label = "Shoot Date",
                                            Name = "shootDate",
                                            Type = "string"
                                        }
                                    },
                                    CostLineItemSections = new List<CostLineItemSectionTemplateModel>
                                    {
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "production",
                                            Label = "Production",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Art Department (Location /studio)",
                                                    Name = "artDepartmentStudio"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Art Department (set dressing)",
                                                    Name = "artDepartmentSetDressing"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Casting",
                                                    Name = "casting"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Crew Costs",
                                                    Name = "crewCost"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Equipment",
                                                    Name = "equiment"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Photographer",
                                                    Name = "photographer"
                                                }
                                            }
                                        },
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "postProduction",
                                            Label = "Post Production",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Retouching",
                                                    Name = "retouching"
                                                }
                                            }
                                        },
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "agencyCosts",
                                            Label = "Agency Costs",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Foreign exchange loss/gain",
                                                    Name = "foreignExchangeLossGain"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Insurance",
                                                    Name = "insurance"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Offline costs",
                                                    Name = "offlineCosts"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Online costs",
                                                    Name = "onlineCosts"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Tax/importation tax",
                                                    Name = "taxImportationTax"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },

            };

            var result = await Browser.Post("/v1/costtemplate/create", with =>
            {
                with.User(user);
                with.JsonBody(model);
            });

            return Deserialize<CostTemplateDetailsModel>(result, HttpStatusCode.Created);
        }

        protected Task<AbstractType> GetRootModule()
        {
            return EFContext.AbstractType.FirstOrDefaultAsync(t => t.Id == t.ParentId);
        }

        protected async Task SetCostStatus(Cost cost, CostStageRevisionStatus status)
        {
            var dbCost = await EFContext.Cost
                .Include(c => c.LatestCostStageRevision)
                .FirstOrDefaultAsync(c => c.Id == cost.Id);

            EFContext.Entry(dbCost).Reload();

            dbCost.Status = status;
            dbCost.LatestCostStageRevision.Status = status;

            await EFContext.SaveChangesAsync();
        }

        protected async Task ExecuteActionAndValidateResponse(Guid costId, CostAction action, CostUser user)
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

        protected Task<BrowserResponse> ExecuteAction(Guid costId, CostAction action, CostUser user)
        {
            var url = $"{CostWorkflowUrl(costId)}/actions";
            return Browser.Post(url, w =>
            {
                w.User(user);
                w.JsonBody(new ExecuteActionModel
                {
                    Action = action
                });
            });
        }
    }
}
