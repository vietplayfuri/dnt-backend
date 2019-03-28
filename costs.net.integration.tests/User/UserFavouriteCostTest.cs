namespace costs.net.integration.tests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Browser;
    using core.Models.ACL;
    using core.Models.Costs;
    using core.Models.User;
    using dataAccess.Entity;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;

    [TestFixture]
    public class UserFavouriteCostTest : BaseCostIntegrationTest
    {
        private Cost _cost;
        private CostUser _adminUser;

        [SetUp]
        public async Task SetUp()
        {
            _adminUser = await CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin);

            var templateResult = CreateTemplate(User).Result;

            var createCostResult = await CreateCost(User, new CreateCostModel
            {
                TemplateId = templateResult.Id,
                StageDetails = new StageDetails
                {
                    Data = new Dictionary<string, dynamic>
                    {
                        { "budgetRegion", new AbstractTypeValue { Key = "AAK (Asia)" }},
                        { "contentType", new { id = Guid.NewGuid(), value = "Video" } },
                        { "productionType",  new { id = Guid.NewGuid(), value = "Full Production" } },
                        { "targetBudget", "<10000" },
                        { "projectId", "123456789" },
                        { "approvalStage", "OriginalEstimate" },
                        { "agency", new
                            {
                                id = _adminUser.Agency.Id,
                                abstractTypeId = _adminUser.Agency.AbstractTypes.FirstOrDefault().Id
                            }
                        }
                    }
                }
            });

            _cost = Deserialize<Cost>(createCostResult, HttpStatusCode.Created);
        }

        [Test]
        public async Task UserFavourite_GetUserFavouriteCosts_whenNoFavourite_shouldReturn_NoContent()
        {
            // Arrange
            var url = $"v1/users/{User.Id}/favourite-cost/";

            // Act
            var result = await Browser.Get(url, c => { c.User(User); });
            var favourites = JsonConvert.DeserializeObject<UserFavouriteCostsModel>(result.Body.AsString());

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            favourites.FavouriteCosts.Should().HaveCount(0);
        }

        [Test]
        public async Task UserFavourite_SavesNewFavourite_correctly()
        {
            // Act
            var result = await CreateFavouriteCost();

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task UserFavourite_GetUserFavouriteCosts_correctly()
        {
            // Arrange
            await CreateFavouriteCost();
            var url = $"v1/users/{User.Id}/favourite-cost/";

            // Act
            var result = await Browser.Get(url, c => { c.User(User); });
            var favourites = JsonConvert.DeserializeObject<UserFavouriteCostsModel>(result.Body.AsString());

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            favourites.FavouriteCosts.Should().ContainSingle(c => c.CostId == _cost.Id);
        }

        [Test]
        public async Task UserFavourite_DeletesFavourite_correctly()
        {
            // Arrange
            await CreateFavouriteCost();
            var url = $"v1/users/{User.Id}/favourite-cost/{_cost.Id}";

            // Act
            var result = await Browser.Delete(url, c => { c.User(User); });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<BrowserResponse> CreateFavouriteCost()
        {
            var url = $"v1/users/{User.Id}/favourite-cost/{_cost.Id}";

            var result = await Browser.Put(url, c => { c.User(User); });

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            return result;
        }
    }
}