namespace costs.net.api.tests.Watchers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Builders.Response;
    using core.Models.CostUser;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class WatchersControllerTests : BaseTestController
    {
        [Test]
        public async Task SearchWatchers_return_none()
        {
            const int expected = 0;
            var costQuery = new CostUserQuery();
            var result = await WatchersController.Search(costQuery);
            var objectResult = result.As<OkObjectResult>();
            objectResult.StatusCode.Should().Be((int) HttpStatusCode.OK);
            var costUserSearchItems = objectResult.Value as IEnumerable<CostUserSearchItem>;
            costUserSearchItems.Should().NotBeNull();
            costUserSearchItems.Count().Should().Be(expected);
        }

        [Test]
        public async Task SearchWatchers_return_one()
        {
            const int expected = 1;
            UserSearchServiceMock.Setup(a => a.SearchWatchers(It.IsAny<CostUserQuery>())).ReturnsAsync(new List<CostUserSearchItem> { new CostUserSearchItem() });
            var costQuery = new CostUserQuery();
            var result = await WatchersController.Search(costQuery);
            var objectResult = result.As<OkObjectResult>();
            objectResult.StatusCode.Should().Be((int) HttpStatusCode.OK);
            var costUserSearchItems = objectResult.Value as IEnumerable<CostUserSearchItem>;
            costUserSearchItems.Should().NotBeNull();
            costUserSearchItems.Count().Should().Be(expected);
        }
    }
}