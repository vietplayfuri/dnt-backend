namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Browser;
    using core.Models.ACL;
    using core.Services;
    using dataAccess.Entity;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins;

    public class CostOwnerTests : BaseCostIntegrationTest
    {
        private Task<BrowserResponse> ChangeCostOwner(Guid costId, Guid ownerId, CostUser identityUser)
        {
            return Browser.Post($"/v1/costs/{costId}/owners/{ownerId}", w => { w.User(identityUser); });
        }

        [Test]
        public async Task ChangeOwner_ShouldChangeOwnerOfTheCost()
        {
            // Arrange
            var cost = await CreateCostEntity(User);
            var identityUser = await CreateUser($"{Guid.NewGuid()}indentity_user", Roles.AgencyAdmin, businessRoleName: Constants.BusinessRole.AgencyAdmin);
            var newOwner = await CreateUser($"{Guid.NewGuid()}new_costOwner", Roles.CostOwner, businessRoleName: Constants.BusinessRole.AgencyOwner);

            // Act
            var response = await ChangeCostOwner(cost.Id, newOwner.Id, identityUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var serviceResult = JsonConvert.DeserializeObject<ServiceResult<Cost>>(response.Body.AsString());
            serviceResult.Result.Owner.Should().NotBeNull();
            serviceResult.Result.Owner.Id.Should().Be(newOwner.Id);
        }

        [Test]
        public async Task ChangeOwner_WhenUserIsNotAgencyAdmin_ShouldReturnError()
        {
            // Arrange
            var cost = await CreateCostEntity(User);
            var identityUser = await CreateUser($"{Guid.NewGuid()}indentity_user", Roles.CostOwner, businessRoleName: Constants.BusinessRole.AgencyOwner);
            var newOwner = await CreateUser($"{Guid.NewGuid()}new_costOwner", Roles.CostOwner, businessRoleName: Constants.BusinessRole.AgencyOwner);

            // Act
            var response = await ChangeCostOwner(cost.Id, newOwner.Id, identityUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
