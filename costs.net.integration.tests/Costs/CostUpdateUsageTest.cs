namespace costs.net.integration.tests.Costs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Models.CostTemplate;
    using core.Models.Response;
    using Browser;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using CostType = core.Models.CostTemplate.CostType;

    public class CostUpdateUsageTests
    {
        public abstract class CostUpdateUsageTest : BaseCostIntegrationTest
        { }

        [TestFixture]
        public class UpdateCostSHould : CostUpdateUsageTest
        {
            [Test]
            public async Task NotCreateDuplicateCustomFormDataForCost()
            {
                var cost = await CreateUsageCostEntity(User);
                var jsonString = @"{
                ""rights"": [],
                ""touchpoints"": [
                {
                    ""name"": ""Radio"",
                    ""id"": ""cf18697a-1630-4251-90b1-2f0bca3a3394""
                },
                {
                    ""name"": ""Out of Home"",
                    ""id"": ""22216a42-d530-4fed-9aae-a9ed40645f2b""
                }
                ],
                ""airingCountries"": [],
                ""contract"": {
                    ""exclusivityCategoryValues"": [
                    ""Exclusivity Category 1"",
                    ""Exclusivity Category 2"",
                    ""Exclusivity Category 3""
                        ],
                    ""exclusivity"": ""yes, if yes specify category"",
                    ""startDate"": ""2017-04-30T23:00:00Z"",
                    ""period"": ""1"",
                    ""endDate"": ""2017-05-30T23:00:00Z""
                },
                ""name"": ""Buyout 2"",
                ""nameOfLicensor"": ""Licensor 6""
            }";

                var costTemplateResponse = await Browser.Get($"/v1/costtemplate", w =>
                {
                    w.User(User);
                });
                var costTemplateList = Deserialize<List<CostTemplateDetailsModel>>(costTemplateResponse, HttpStatusCode.OK);
                var usageCostTemplates = costTemplateList.Where(a => a.CostType == CostType.Buyout);
                var usageBuyoutForm = usageCostTemplates.OrderByDescending(a=>a.Created).First().Versions.OrderByDescending(t => t.Created).First().Forms.FirstOrDefault(c => c.Name == "buyoutDetails");
                var updateModel = new UpdateCostFormModel
                {
                    CostFormDetails = new CostFormDetailsModel
                    {
                        Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString)
                    },
                    FormDefinitionId = usageBuyoutForm.Id
                };
                //Update initial values for Usage/Buyout details screen
                var result = await Browser.Patch($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                DoAssertChecksForDuplication(result, cost, usageBuyoutForm);

                updateModel = new UpdateCostFormModel
                {
                    CostFormDetails = new CostFormDetailsModel
                    {
                        Data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString)
                    },
                    FormDefinitionId = usageBuyoutForm.Id
                };
                //Update initial values for Usage/Buyout details screen with the save values
                //We are testing duplication not updated values
                result = await Browser.Patch($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                //We do this again to check for duplication in the customFormData
                DoAssertChecksForDuplication(result, cost, usageBuyoutForm);
            }

            [Test]
            public async Task UpdateCustomFormDataForCost()
            {
                var cost = await CreateUsageCostEntity(User);
                var originalResults = new Dictionary<string, dynamic>
                {
                    { "rights", new dynamic[0] },
                    { "touchpoints", new []
                        {
                            new
                            {
                                name = "Radio",
                                id = "cf18697a-1630-4251-90b1-2f0bca3a3394"
                            },
                            new
                            {
                                name = "Out of Home",
                                id = "22216a42 - d530 - 4fed - 9aae - a9ed40645f2b"
                            }
                        }
                    },
                    { "airingCountries", new dynamic[0] },
                    { "contract", new Dictionary<string, dynamic>
                        {
                            { "exclusivityCategoryValues", new []
                                {
                                    "Exclusivity Category 1",
                                    "Exclusivity Category 2",
                                    "Exclusivity Category 3"
                                }
                            },
                            { "exclusivity", "yes, if yes specify category" },
                            { "startDate", "2017-04-30T23:00:00Z" },
                            { "period", "1" },
                            { "endDate", "2017-05-30T23:00:00Z" }
                        }
                    },
                    { "name", "Buyout 2" },
                    { "nameOfLicensor", "Licensor 6" }
                };

                var costTemplateResponse = await Browser.Get($"/v1/costtemplate", w =>
                {
                    w.User(User);
                });
                var costTemplateList = Deserialize<List<CostTemplateDetailsModel>>(costTemplateResponse, HttpStatusCode.OK);
                var usageCostTemplates = costTemplateList.Where(a => a.CostType == CostType.Buyout);
                var usageBuyoutForm = usageCostTemplates.OrderByDescending(a => a.Created).First().Versions.OrderByDescending(t => t.Created).First().Forms.FirstOrDefault(c => c.Name == "buyoutDetails");

                var initialModel = new UpdateCostFormModel
                {
                    CostFormDetails = new CostFormDetailsModel
                    {
                        Data = originalResults
                    },
                    FormDefinitionId = usageBuyoutForm.Id
                };
                //Update initial values for Usage/Buyout details screen
                var result = await Browser.Patch($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(initialModel);
                });

                DoAssertChecksForDuplication(result, cost, usageBuyoutForm);

               var updateModel = new UpdateCostFormModel
                {
                    CostFormDetails = new CostFormDetailsModel
                    {
                        Data = new Dictionary<string, dynamic>(originalResults)
                    },
                    FormDefinitionId = usageBuyoutForm.Id
                };
                updateModel.CostFormDetails.Data["name"] = "NewName";
                updateModel.CostFormDetails.Data["nameOfLicensor"] = "New Liensor 777";

                //Update initial values for Usage/Buyout details screen with the save values
                result = await Browser.Patch($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                //We do this again to check for duplication in the customFormData
                DoAssertChecksForDuplication(result, cost, usageBuyoutForm);
                //Check for updated values
                DoAssertChecksForUpdatedValues(cost, originalResults);
            }

            private void DoAssertChecksForUpdatedValues( Cost cost, Dictionary<string, dynamic> originalValues)
            {
                var selectJoinResult = EFContext.CostFormDetails.Where(c => c.CostStageRevisionId == cost.LatestCostStageRevision.Id).Join(EFContext.CustomFormData, d => d.FormDataId, cfd => cfd.Id, (details, data) => new { data, details }).First();
                EFContext.ReloadEntity(selectJoinResult.data);

                var updatedValues = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(selectJoinResult.data.Data);

                Assert.AreNotEqual(updatedValues, originalValues);
                Assert.AreNotEqual(updatedValues["name"], originalValues["name"]);
                Assert.AreNotEqual(updatedValues["nameOfLicensor"], originalValues["nameOfLicensor"]);
                Assert.AreEqual(updatedValues["nameOfLicensor"], "New Liensor 777");
                Assert.AreEqual(updatedValues["name"], "NewName");
            }

            private void DoAssertChecksForDuplication(BrowserResponse result, Cost cost, FormDefinitionModel usageBuyoutForm)
            {
                var costDetailsUpdatedResponse = Deserialize<OperationResponse>(result, HttpStatusCode.OK);
                //Check Response Values
                Assert.AreEqual(costDetailsUpdatedResponse.Messages.First(), "Cost form data saved");
                Assert.AreEqual(costDetailsUpdatedResponse.Success, true);

                var selectJoinResult = EFContext.CostFormDetails.Where(c => c.CostStageRevisionId == cost.LatestCostStageRevision.Id).Join(EFContext.CustomFormData, d => d.FormDataId, cfd => cfd.Id, (details, data) => new { data, details });
                //Check there is only one data entry
                Assert.AreEqual(selectJoinResult.Count(), 1);
                var firstResult = selectJoinResult.First();
                //Check if the ID's are correct
                Assert.AreEqual(firstResult.details.FormDefinitionId, usageBuyoutForm.Id);
            }
        }
    }
}