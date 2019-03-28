namespace costs.net.api.tests.AdvancedSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Services.AdvancedSearch;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;
    using Ploeh.AutoFixture;

    [TestFixture]
    internal class AdvancedSearchControllerTests : BaseTestController
    {
        [Test]
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(7)]
        [TestCase(20)]
        public async Task GetCostOwners(int number)
        {
            var fixture = new Fixture();
            var costOwnerViewModels = fixture.CreateMany<CostOwnerViewModel>(number).ToList();
            var query = new CostQuery();
            var expected = costOwnerViewModels.Count;
            AdvancedSearchServiceMock.Setup(a => a.SearchOwners(query)).ReturnsAsync(costOwnerViewModels);
            var result = await AdvancedSearchController.GetCostOwners(query);
            var objectResult = result.As<OkObjectResult>();
            objectResult.StatusCode.Should().Be((int) HttpStatusCode.OK);
            var costUserSearchItems = objectResult.Value as IEnumerable<CostOwnerViewModel>;
            costUserSearchItems.Should().NotBeNull();
            costUserSearchItems.Count().Should().Be(expected);
            var userList = costUserSearchItems.ToList();
            userList.ForEach(u =>
            {
                u.FullName.Should().NotBeNullOrEmpty();
                var valid = Guid.TryParse(u.Id.ToString(), out var validGuid);
                valid.Should().BeTrue();
                u.Id.Should().Be(validGuid);
            });
        }
    }
}