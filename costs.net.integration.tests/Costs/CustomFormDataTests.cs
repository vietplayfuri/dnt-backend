namespace costs.net.integration.tests.Costs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Models.Response;
    using FluentAssertions;
    using NUnit.Framework;

    public class CustomFormDataTests
    {
        public abstract class CustomFormDataTest : BaseCostIntegrationTest
        {
        }

        [TestFixture]
        public class GetCustomFormDataShould : CustomFormDataTest
        {
            [Test]
            public async Task GetCustomFormData()
            {
                var cost = await CreateCostEntity(User);

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

                var updatedResult = await Browser.Put($"/v1/costs/{cost.Id}", w =>
                {
                    w.User(User);
                    w.JsonBody(updateModel);
                });

                var updateResponse = Deserialize<OperationResponse>(updatedResult, HttpStatusCode.OK);

                updateResponse.Success.Should().BeTrue();

                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var productionDetailsForm = Deserialize<CustomFormDataModel>(
                    await Browser.Get($"/v1/costs/{cost.Id}/custom-form-data/{latestRevision.ProductDetailsId}", w => w.User(User)), 
                    HttpStatusCode.OK);

                var type = productionDetailsForm.Data.Value<string>("type");
                var talentCompanies = productionDetailsForm.Data["talentCompanies"].Values<string>().ToArray();

                type.Should().Be(updateModel.ProductionDetails.Data["type"]);
                talentCompanies.Should().BeEquivalentTo(updateModel.ProductionDetails.Data["talentCompanies"]);
            }
        }
    }
}
