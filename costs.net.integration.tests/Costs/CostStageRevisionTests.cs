namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Browser;
    using core.Messaging.Messages;
    using core.Models.Approvals;
    using core.Models.Costs;
    using core.Models.CostTemplate;
    using core.Models.Response;
    using core.Models.Workflow;
    using core.Services;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using plugins.PG.Models.Stage;
    using HttpMethod = Elasticsearch.Net.HttpMethod;

    public class CostStageRevisionTests
    {
        public abstract class CostStageRevisionTest : BaseCostIntegrationTest
        {
            protected async Task<Guid> GetMediaTypeId(string value = null)
            {
                return await GetDictionaryEntryId("MediaType/TouchPoints", value ?? plugins.Constants.MediaType.Cinema);
            }

            protected async Task<Guid> GetOvalTypeId(string value = null)
            {
                return await GetDictionaryEntryId("OvalType", value ?? "Original");
            }

            private async Task<Guid> GetDictionaryEntryId(string dictionaryName, string value)
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
                return entry.Id;
            }

            protected void CreateMediaTypeMappingIfNotCreated(Guid mediaTypeId)
            {
                var contentTypeId = EFContext.DictionaryEntry
                    .First(de =>
                        de.Key == plugins.Constants.ContentType.Video
                        && de.Dictionary.Name == plugins.Constants.DictionaryNames.ContentType).Id;

                if (!EFContext.DependentItem.Any(di => di.ChildId == mediaTypeId && di.ParentId == contentTypeId))
                {
                    EFContext.DependentItem.Add(new DependentItem
                    {
                        ChildId = mediaTypeId,
                        ParentId = contentTypeId
                    });
                    EFContext.SaveChanges();
                }
            }
        }

        [TestFixture]
        public class UpdateCostLineItemsShould : CostStageRevisionTest
        {
            [Test]
            public async Task UpdateLineItems()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var costLineItems = await GetCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, User);
                costLineItems.Should().BeEmpty();

                // Update line items
                var templateSection = CostTemplate.Versions.Last().ProductionDetails.First().Forms.First().CostLineItemSections.Single(x => x.Name == "production");

                var defaultCurrency = await GetDefaultCurrency();
                var currency = await GetCurrencyByCode("CAD");
                var exchangeRate = new ExchangeRate
                {
                    AbstractTypeId = (await GetRootModule()).Id,
                    EffectiveFrom = DateTime.UtcNow,
                    FromCurrency = currency.Id,
                    ToCurrency = defaultCurrency.Id,
                    Rate = (decimal)1.5,
                    RateName = "Test",
                    RateType = "Test"
                };
                EFContext.ExchangeRate.Add(exchangeRate);
                await EFContext.SaveChangesAsync();

                var url = CostLineItemsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var updateLineItemsResult = await Browser.Put(url, w =>
                {
                    w.User(User);
                    w.JsonBody(new UpdateCostLineItemsModel
                    {
                        CostLineItemData = new List<CostLineItemModel>
                        {
                            new CostLineItemModel
                            {
                                Name = "newVal",
                                Value = 12,
                                LocalCurrencyId = currency.Id,
                                TemplateSectionId = templateSection.Id
                            }
                        }
                    });
                });

                var updateLineItemsResponse = Deserialize<OperationResponse>(updateLineItemsResult, HttpStatusCode.OK);
                updateLineItemsResponse.Success.Should().BeTrue();

                costLineItems = await GetCostLineItems(cost.Id, latestStage.Id, latestRevision.Id, User);
                var newVal = costLineItems.Single(x => x.Name == "newVal");

                newVal.Name.Should().Be("newVal");
                newVal.ValueInLocalCurrency.Should().Be(12);
                newVal.LocalCurrencyId.Should().Be(currency.Id);
                newVal.ValueInDefaultCurrency.Should().Be(12 * exchangeRate.Rate);
                newVal.TemplateSectionId.Should().Be(templateSection.Id);
            }
        }

        [TestFixture]
        public class CreateExpectedAssetsShould : CostStageRevisionTest
        {
            [Test]
            public async Task CreateExpectedAssets()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);
                var mediaTypeId = await GetMediaTypeId();
                CreateMediaTypeMappingIfNotCreated(mediaTypeId);

                var model = new CreateExpectedAssetModel
                {
                    Title = "Title1",
                    AdId = "DAGWJHOER123",
                    AiringRegions = new[] { "EMEA" },
                    Duration = TimeSpan.FromHours(1).ToString(),
                    Definition = "HD",
                    MediaTypeId =mediaTypeId,
                    OvalTypeId = await GetOvalTypeId(),
                    FirstAirDate = DateTime.UtcNow.AddDays(5),
                    AssetId = "ADGADG",
                    Scrapped = false
                };

                var url = ExpectedAssetsUrl(cost.Id, latestStage.Id, latestRevision.Id);

                var createExpectedAssetResponse = await Browser.Post(url, with =>
                {
                    with.User(User);
                    with.JsonBody(model);
                });

                var expectedAsset = Deserialize<ExpectedAsset>(createExpectedAssetResponse, HttpStatusCode.Created);

                expectedAsset.Title.Should().Be(model.Title);
                expectedAsset.ProjectAdIdId.Should().NotBeEmpty();
                expectedAsset.AiringRegions.Should().BeEquivalentTo(model.AiringRegions);
                expectedAsset.Duration.ToString().Should().Be(model.Duration);
                expectedAsset.Definition.Should().Be(model.Definition);
                expectedAsset.MediaTypeId.Should().Be(model.MediaTypeId);
                expectedAsset.OvalTypeId.Should().Be(model.OvalTypeId);
                expectedAsset.FirstAirDate.Should().BeSameDateAs(model.FirstAirDate);
                expectedAsset.AssetId.Should().Be(model.AssetId);
                expectedAsset.Scrapped.Should().Be(model.Scrapped);
            }
        }

        [TestFixture]
        public class UpdateExpectedAssetShould : CostStageRevisionTest
        {
            [Test]
            public async Task UpdateExpectedAsset()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);
                var mediaTypeId = await GetMediaTypeId();
                CreateMediaTypeMappingIfNotCreated(mediaTypeId);

                var model = new CreateExpectedAssetModel
                {
                    Title = "Title1",
                    AdId = "DAGWJHOERO534",
                    AiringRegions = new[] { "EMEA" },
                    Duration = TimeSpan.FromHours(1).ToString(),
                    Definition = "HD",
                    MediaTypeId = mediaTypeId,
                    OvalTypeId = await GetOvalTypeId(),
                    FirstAirDate = DateTime.UtcNow.AddDays(5),
                    AssetId = "ADGADG",
                    Scrapped = false
                };

                var url = ExpectedAssetsUrl(cost.Id, latestStage.Id, latestRevision.Id);

                var expectedAsset = Deserialize<ExpectedAsset>(await Browser.Post(url, with =>
                {
                    with.User(User);
                    with.JsonBody(model);
                }), HttpStatusCode.Created);

                var updateModel = new UpdateExpectedAssetModel
                {
                    Title = "Title2",
                    AdId = "DAGWJHOERO534",
                    AiringRegions = new[] { "AFRICA" },
                    Duration = TimeSpan.FromHours(2).ToString(),
                    Definition = "SD",
                    MediaTypeId = await GetMediaTypeId(),
                    OvalTypeId = await GetOvalTypeId(),
                    FirstAirDate = DateTime.UtcNow.AddDays(10),
                    AssetId = "blah",
                    Scrapped = true
                };

                url += "/" + expectedAsset.Id;

                var updateResponse = Deserialize<ServiceResult>(await Browser.Put(url, with =>
                {
                    with.User(User);
                    with.JsonBody(updateModel);
                }), HttpStatusCode.OK);

                updateResponse.Success.Should().BeTrue();

                expectedAsset = Deserialize<ExpectedAsset>(await Browser.Get(url, with => with.User(User)), HttpStatusCode.OK);

                expectedAsset.Title.Should().Be(updateModel.Title);
                expectedAsset.ProjectAdIdId.Should().NotBeEmpty();
                expectedAsset.AiringRegions.Should().BeEquivalentTo(updateModel.AiringRegions);
                expectedAsset.Duration.ToString().Should().Be(updateModel.Duration);
                expectedAsset.Definition.Should().Be(updateModel.Definition);
                expectedAsset.MediaTypeId.Should().Be(updateModel.MediaTypeId);
                expectedAsset.OvalTypeId.Should().Be(updateModel.OvalTypeId);
                //expectedAsset.FirstAirDate.Should().Be(updateModel.FirstAirDate);
                expectedAsset.AssetId.Should().Be(updateModel.AssetId);
                expectedAsset.Scrapped.Should().Be(updateModel.Scrapped);
                expectedAsset.Modified.Should().BeAfter(expectedAsset.Created);
            }
        }

        [TestFixture]
        public class ReopenTraffickingFinalActualApprovedShould : CostStageRevisionTest
        {
            [Test]
            public async Task SendCostForApproval()
            {
                const decimal initialAmount = 61100m;
                const decimal amountReduced = 30000m;
                const decimal expectedCreditAmount = amountReduced- 61100m; // For trafficking everthing is payed at FA

                var cost = await CreateDistributionCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);
                var updateCostLineItemsUrl = CostLineItemsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var usdCurrency = EFContext.Currency.First(a => a.Code == "USD");
                var distributionTemplateSection = TrafficCostTemplate.Versions.Last().ProductionDetails.First().Forms.First().CostLineItemSections
                    .Single(x => x.Name == "distributionCosts");
                var otherTemplateSection = TrafficCostTemplate.Versions.Last().ProductionDetails.First().Forms.First().CostLineItemSections.Single(x => x.Name == "OtherCosts");

                var updateCostLineItemModel = GetCostLineModel(initialAmount, usdCurrency, distributionTemplateSection, otherTemplateSection);
                await SendRequest(HttpMethod.PUT, updateCostLineItemsUrl, updateCostLineItemModel, User, HttpStatusCode.OK);

                var saveCustomDataUrl = SaveCustomDataUrl(latestRevision.Id, "PaymentDetails");
                var customData = GetCustomData(latestRevision);
                await SendRequest(HttpMethod.POST, saveCustomDataUrl, customData, User, HttpStatusCode.OK);

                GetUsers(out var clientAdmin, out var brandManager);
                EFContext.Add(brandManager);
                EFContext.Add(clientAdmin);
                EFContext.SaveChanges();

                var approval = EFContext.Approval.First(a => a.CostStageRevisionId == latestRevision.Id && a.CreatedById == User.Id && a.Type == ApprovalType.Brand);

                var approvalUrl = ApprovalsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var updateApprovals = GetApprovalsUpdate(brandManager, approval);

                await SendRequest(HttpMethod.POST, approvalUrl, updateApprovals, User, HttpStatusCode.OK);

                var submitUrl = ActionUrl(cost.Id);
                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.Submit,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, User, HttpStatusCode.OK);

                var message = GetPurchaseOrderResponseMessage(cost);
                await PurchaseOrderResponseConsumer.Consume(message);

                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.NextStage,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, User, HttpStatusCode.OK);

                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.Submit,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, User, HttpStatusCode.OK);
             
                cost = EFContext.Cost.Include(c => c.LatestCostStageRevision).First(a => a.Id == cost.Id);
                EFContext.Entry(cost).Reload();
                cost.Status.Should().Be(CostStageRevisionStatus.Approved);
                cost.LatestCostStageRevision.Status.Should().Be(CostStageRevisionStatus.Approved);
                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.RequestReopen,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, User, HttpStatusCode.OK);

                cost = EFContext.Cost
                    .Include(c => c.LatestCostStageRevision)
                    .First(a => a.Id == cost.Id);
                EFContext.Entry(cost).Reload();
                cost.Status.Should().Be(CostStageRevisionStatus.PendingReopen);
                cost.LatestCostStageRevision.Status.Should().Be(CostStageRevisionStatus.PendingReopen);

                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.ApproveReopen,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, clientAdmin, HttpStatusCode.OK);
               
                cost = EFContext.Cost
                    .Include(c => c.LatestCostStageRevision)
                    .Include(a => a.CostStages)
                    .ThenInclude(cs => cs.CostStageRevisions)
                    .First(a => a.Id == cost.Id);
                EFContext.Entry(cost).Reload();
                cost.Status.Should().Be(CostStageRevisionStatus.Draft);
                EFContext.Entry(cost.LatestCostStageRevision).Reload();
                cost.LatestCostStageRevision.Status.Should().Be(CostStageRevisionStatus.Draft);
                cost.CostStages.Count.Should().Be(2);
                cost.CostStages.First(a => a.Key == CostStages.OriginalEstimate.ToString()).CostStageRevisions.Count.Should().Be(1);
                cost.CostStages.First(a => a.Key == CostStages.FinalActual.ToString()).CostStageRevisions.Count.Should().Be(2);
                cost.CostStages.First(a => a.Key == CostStages.FinalActual.ToString()).CostStageRevisions.Where(a => a.Status == CostStageRevisionStatus.Draft).ToList().Count
                    .Should().Be(1);
                cost.CostStages.First(a => a.Key == CostStages.FinalActual.ToString()).CostStageRevisions.Where(a => a.Status == CostStageRevisionStatus.Approved).ToList().Count
                    .Should().Be(1);

                updateCostLineItemModel.CostLineItemData.First(a => a.Name == "distributionCosts").Value = amountReduced;
                updateCostLineItemsUrl = CostLineItemsUrl(cost.Id, cost.LatestCostStageRevision.CostStageId, (Guid)cost.LatestCostStageRevisionId);

                await SendRequest(HttpMethod.PUT, updateCostLineItemsUrl, updateCostLineItemModel, User, HttpStatusCode.OK);
                await SendRequest(HttpMethod.POST, submitUrl, new ExecuteActionModel
                {
                    Action = CostAction.Submit,
                    Parameters = new Dictionary<string, dynamic> { { "costId", cost.Id.ToString() } }
                }, User, HttpStatusCode.OK);

             var costStageRevisions = EFContext.CostStageRevision.Include(csr => csr.CostStageRevisionPaymentTotals).Where(csr => csr.CostStage.CostId == cost.Id).ToList();
                costStageRevisions.Count.Should().Be(3);
                costStageRevisions.Where(a => a.CostStageRevisionPaymentTotals.Any(b => b.IsProjection)).ToList().Count.Should().Be(1);
                costStageRevisions.Where(a => a.CostStageRevisionPaymentTotals.Any(b => b.StageName == CostStages.FinalActual.ToString())).ToList().Count.Should().Be(3);
                //ADC-2690 verify the LineItemTotalCalculatedValue of FA CostTotal which should be equal Grand total at Final actual -II minus Grand total in Final actual -I
                costStageRevisions
                    .Where(a => a.CostStageRevisionPaymentTotals.Any(b => b.StageName == CostStages.FinalActual.ToString() && b.LineItemTotalCalculatedValue == expectedCreditAmount))
                    .ToList().Count.Should().Be(1);
                costStageRevisions
                    .Where(a => a.CostStageRevisionPaymentTotals.Any(b => b.StageName == CostStages.FinalActual.ToString() && b.LineItemTotalCalculatedValue == initialAmount))
                    .ToList().Count.Should().Be(2);
                costStageRevisions
                    .Where(a => a.CostStageRevisionPaymentTotals.Any(b => b.StageName == CostStages.OriginalEstimate.ToString()))
                    .ToList().Count.Should().Be(1);
            }

            private IEnumerable<ApprovalModel> GetApprovalsUpdate(CostUser brandManager, Approval approval)
            {
                IEnumerable<ApprovalModel> updateApprovals = new List<ApprovalModel>
                {
                    new ApprovalModel
                    {
                        Requisitioners = new List<RequisitionerModel>
                        {
                            new RequisitionerModel
                            {
                                Id = brandManager.Id,
                                FullName = brandManager.FullName,
                                Email = brandManager.Email,
                                BusinessRoles = new[] { brandManager.UserBusinessRoles.First().BusinessRole.Value }
                            }
                        },
                        Status = ApprovalStatus.New,
                        Type = ApprovalType.Brand,
                        CreatedById = User.Id,
                        Created = DateTime.Now,
                        IsExternal = true,
                        ValidBusinessRoles = new[] { brandManager.UserBusinessRoles.First().BusinessRole.Value },
                        ApprovalMembers = new List<ApprovalModel.Member>(),
                        Id = approval.Id
                    }
                };
                return updateApprovals;
            }

            private static PurchaseOrderResponse GetPurchaseOrderResponseMessage(Cost cost)
            {
                var message = new PurchaseOrderResponse
                {
                    ActivityType = "updated",
                    CostNumber = cost.CostNumber,
                    ClientName = "Pg",
                    EventTimeStamp = DateTime.Now,
                    Payload = JObject.Parse($@"{{
    ""requisition"": ""135244"",
    ""poNumber"": ""8000103223"",
    ""approverEmail"": ""hempton.g@pg.com"",
    ""ioNumberOwner"": ""hempton.g@pg.com"",
    ""grNumber"": null,
    ""glAccount"": ""0033500001"",
    ""poDate"": ""2017-10-30T11:11:21+00:00"",
    ""grDate"": null,
    ""accountCode"": ""823-4776-KR-G4P~F1--004001597408-S821018AT-0033500001"",
    ""comments"": ""Management hierarchy approver. The approver is the manager of Esteban Sanchez with an approval limit of 100000.00 USD for requisition"",
    ""type"": null,
    ""itemIdCode"": ""199150"",
    ""approvalStatus"": ""approved"",
    ""totalAmount"": null
  }}")
                };
                return message;
            }

            private async Task SendRequest(HttpMethod method, string url, object data, CostUser user, HttpStatusCode code)
            {
                BrowserResponse response;
                switch (method)
                {
                    case HttpMethod.POST:
                        response = await Browser.Post(url, with =>
                          {
                              with.User(user);
                              with.JsonBody(data);
                          });
                        response.StatusCode.Should().Be(code);
                        break;
                    case HttpMethod.PUT:
                        response = await Browser.Put(url, with =>
                          {
                              with.User(user);
                              with.JsonBody(data);
                          });
                        response.StatusCode.Should().Be(code);
                        break;
                }
            }

            private void GetUsers(out CostUser clientAdmin, out CostUser brandManager)
            {
                brandManager = new CostUser
                {
                    AgencyId = User.AgencyId,
                    AbstractType = User.AbstractType,
                    Email = $"{Guid.NewGuid()}Brand@brand.com",
                    GdamUserId = "12341231234123",
                    FirstName = "Brand",
                    LastName = "manager",
                    FullName = "Brand manager",
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = EFContext.BusinessRole.First(a => a.Key == plugins.Constants.BusinessRole.BrandManager),
                            ObjectType = "Client"
                        }
                    }
                };

                clientAdmin = new CostUser
                {
                    AgencyId = User.AgencyId,
                    AbstractType = User.AbstractType,
                    Email = $"{Guid.NewGuid()}clientAdmin@brand.com",
                    GdamUserId = "1234231234123",
                    FirstName = "Client",
                    LastName = "Admin",
                    FullName = "Client Admin",
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = EFContext.BusinessRole.First(a => a.Key == plugins.Constants.BusinessRole.GovernanceManager),
                            ObjectType = "Client"
                        }
                    },
                    UserUserGroups = new List<UserUserGroup>
                    {
                        new UserUserGroup
                        {
                            UserGroup = new UserGroup
                            {
                                Role = EFContext.Role.First(a => a.Name == "client.admin"),
                                Disabled = false,
                                Name = "client.admin",
                                ObjectType = "abstract_type",
                                ObjectId = EFContext.AbstractType.First(a => a.Module.ClientType == ClientType.Pg).Id
                            }
                        }
                    }
                };
            }

            private static Dictionary<string, dynamic> GetCustomData(CostStageRevision latestRevision)
            {
                var customData = new Dictionary<string, dynamic>
                {
                    { "name", "PaymentDetails" },
                    {
                        "data", new
                        {
                            grNumber = "",
                            ioNumber = "1231231231",
                            poNumber = ""
                        }
                    },
                    { "objectId", latestRevision.Id.ToString() }
                };
                return customData;
            }

            private static UpdateCostLineItemsModel GetCostLineModel(decimal initialAmount, Currency usdCurrency, CostLineItemSectionTemplateModel distributionTemplateSection,
                CostLineItemSectionTemplateModel otherTemplateSection)
            {
                var updateCostLineItemModel = new UpdateCostLineItemsModel
                {
                    CostLineItemData = new List<CostLineItemModel>
                    {
                        new CostLineItemModel
                        {
                            Name = "distributionCosts",
                            Value = initialAmount,
                            LocalCurrencyId = usdCurrency.Id,
                            TemplateSectionId = distributionTemplateSection.Id
                        },
                        new CostLineItemModel
                        {
                            Name = "taxIfApplicable",
                            Value = 0,
                            LocalCurrencyId = usdCurrency.Id,
                            TemplateSectionId = otherTemplateSection.Id
                        },
                        new CostLineItemModel
                        {
                            Name = "technicalFee",
                            Value = 0,
                            LocalCurrencyId = usdCurrency.Id,
                            TemplateSectionId = otherTemplateSection.Id
                        },
                        new CostLineItemModel
                        {
                            Name = "foreignExchange",
                            Value = 0,
                            LocalCurrencyId = usdCurrency.Id,
                            TemplateSectionId = otherTemplateSection.Id
                        }
                    }
                };
                return updateCostLineItemModel;
            }
        }

        [TestFixture]
        public class DeleteExpectedAssetShould : CostStageRevisionTest
        {
            [Test]
            public async Task DeleteExpectedAsset()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);
                var mediaTypeId = await GetMediaTypeId();
                CreateMediaTypeMappingIfNotCreated(mediaTypeId);

                var model = new CreateExpectedAssetModel
                {
                    Title = "Title1",
                    AdId = "DAGWJHOER1O",
                    AiringRegions = new[] { "EMEA" },
                    Duration = TimeSpan.FromHours(1).ToString(),
                    Definition = "HD",
                    MediaTypeId = mediaTypeId,
                    OvalTypeId = await GetOvalTypeId(),
                    FirstAirDate = DateTime.UtcNow.AddDays(5),
                    AssetId = "ADGADG",
                    Scrapped = false
                };

                var url = ExpectedAssetsUrl(cost.Id, latestStage.Id, latestRevision.Id);

                var expectedAsset = Deserialize<ExpectedAsset>(await Browser.Post(url, with =>
                {
                    with.User(User);
                    with.JsonBody(model);
                }), HttpStatusCode.Created);

                url += "/" + expectedAsset.Id;

                var deleteRequest = await Browser.Delete(url, w => w.User(User));
                deleteRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
