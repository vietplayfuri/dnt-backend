
namespace costs.net.integration.tests.MaterialLedgerCodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Builders;
    using core.Models.Costs;
    using core.Services.CustomData;
    using core.Services.PostProcessing;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services;

    [TestFixture]
    public abstract class BaseMaterialLedgerCodeTest : BaseCostIntegrationTest
    {
        protected PgLedgerMaterialCodeService LedgerMaterialCodeService;

        [SetUp]
        public void Setup()
        {
            var actionPostProcessors = new List<Lazy<IActionPostProcessor, PluginMetadata>>();
            var customObjectDataService = new CustomObjectDataService(EFContext, actionPostProcessors);
            LedgerMaterialCodeService = new PgLedgerMaterialCodeService(EFContext, customObjectDataService);
        }

        protected async Task<Cost> CreateProductionCost(CostUser owner, string costTitle, string contentType, string productionType)
        {
            if (CostTemplate == null)
            {
                CostTemplate = await CreateTemplate(owner);
            }

            var ct = await GetContentType(contentType);
            DictionaryEntry pt = null;

            if (!string.IsNullOrEmpty(productionType))
            {
                pt = await GetProductionType(productionType);
            }

            var createCostResult = await CreateCost(owner, new CreateCostModel
            {
                TemplateId = CostTemplate.Id,
                StageDetails = new StageDetails
                {
                    Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(JsonConvert.SerializeObject(new PgStageDetailsForm
                    {
                        Agency = new PgStageDetailsForm.AbstractTypeAgency
                        {
                            Id = owner.AgencyId,
                            AbstractTypeId = owner.Agency.AbstractTypes.First().Id,
                            Name = owner.Agency.Name
                        },
                        ApprovalStage = "OriginalEstimate",
                        BudgetRegion = new AbstractTypeValue
                        {
                            Key = plugins.Constants.BudgetRegion.AsiaPacific,
                            Name = plugins.Constants.BudgetRegion.AsiaPacific
                        },
                        ContentType = new DictionaryValue { Id = ct.Id, Value = ct.Value, Key = ct.Key },
                        ProductionType = pt != null ? new DictionaryValue { Id = pt.Id, Value = pt.Value, Key = pt.Key } : null,
                        ProjectId = "123456789",
                        Title = costTitle
                    }))
                }
            });

            return Deserialize<Cost>(createCostResult, HttpStatusCode.Created);
        }

        protected async Task<Cost> CreateUsageCost(CostUser owner, string costTitle = "Usage Buyout", string usageType = plugins.Constants.UsageType.Celebrity)
        {
            if (UsageCostTemplate == null)
            {

                UsageCostTemplate = await CreateUsageTemplate(owner);
            }

            var ut = await GetUsageType(usageType);

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
                        { "usageType", new DictionaryValue { Key = ut.Key, Value = ut.Value, Id = ut.Id }},
                        { "usageBuyoutType", new DictionaryValue { Key = "Buyout" } },
                        { "title", costTitle},
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

        protected async Task CreateExpectedAsset(Guid costId, Guid stageId, Guid revisionId, CreateExpectedAssetModel model)
        {
            var url = ExpectedAssetsUrl(costId, stageId, revisionId);

            await Browser.Post(url, with =>
            {
                with.User(User);
                with.JsonBody(model);
            });
        }

        protected async Task<DictionaryEntry> GetContentType(string contentType = null)
        {
            return await GetDictionaryEntry("ContentType", contentType ?? plugins.Constants.ContentType.Video);
        }

        protected async Task<DictionaryEntry> GetProductionType(string productionType = null)
        {
            return await GetDictionaryEntry("ProductionType", productionType ?? plugins.Constants.ProductionType.FullProduction);
        }

        protected async Task<Guid> GetMediaTypeId(string mediaType = null)
        {
            return await GetDictionaryEntryId("MediaType/TouchPoints", mediaType ?? plugins.Constants.MediaType.Cinema);
        }

        protected async Task<Guid> GetOvalTypeId(string ovalType = null)
        {
            return await GetDictionaryEntryId("OvalType", ovalType ?? "Original");
        }

        protected async Task<DictionaryEntry> GetUsageType(string usageType = null)
        {
            return await GetDictionaryEntry("UsageType", usageType ?? plugins.Constants.UsageType.Celebrity);
        }

        protected async Task CreateMediaTypeMappingIfNotCreated(Guid mediaTypeId, string contentType = plugins.Constants.ContentType.Video)
        {
            var contentTypeId = await EFContext.DictionaryEntry
                .Where(de =>
                    de.Key == contentType
                    && de.Dictionary.Name == plugins.Constants.DictionaryNames.ContentType)
                .Select(de => de.Id)
                .SingleAsync();

            if (!EFContext.DependentItem.Any(di => di.ChildId == mediaTypeId && di.ParentId == contentTypeId))
            {
                EFContext.DependentItem.Add(new DependentItem
                {
                    ChildId = mediaTypeId,
                    ParentId = contentTypeId
                });
                await EFContext.SaveChangesAsync();
            }
        }

        private async Task<Guid> GetDictionaryEntryId(string dictionaryName, string value)
        {
            return (await GetDictionaryEntry(dictionaryName, value)).Id;
        }

        private async Task<DictionaryEntry> GetDictionaryEntry(string dictionaryName, string value)
        {
            var root = await GetRootModule();
            var dictionary = await EFContext.Dictionary.FirstOrDefaultAsync(d => d.Name == dictionaryName);
            if (dictionary == null)
            {
                dictionary = new Dictionary
                {
                    Name = dictionaryName,
                    AbstractTypeId = root.Id
                };
                EFContext.Add(dictionary);
                await EFContext.SaveChangesAsync();
            }
            var entry = await EFContext.DictionaryEntry.FirstOrDefaultAsync(de => de.DictionaryId == dictionary.Id && de.Key == value);
            if (entry == null)
            {
                entry = new DictionaryEntry
                {
                    DictionaryId = dictionary.Id,
                    Key = value
                };
                EFContext.DictionaryEntry.Add(entry);
                await EFContext.SaveChangesAsync();
            }
            return entry;
        }
    }
}
