namespace costs.net.integration.tests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using core.Builders.Response;
    using core.Models.ACL;
    using core.Models.CostFilter;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using Moq;
    using Nest;

    [TestFixture]
    public class CostFilterTest : BaseIntegrationTest
    {
        private CostUser _user;
        private CostFilterModel _filter;
        private const string FilterName = "filter-test";

        [OneTimeSetUp]
        public void SetUp()
        {
            CreateUser($"{Guid.NewGuid()}bob", Roles.AgencyAdmin)
                .ContinueWith(c =>
                {
                    _user = c.Result;
                })
                .Wait();
            var countResponse = new Mock<ICountResponse>();
            ElasticClient.Setup(a =>
                    a.CountAsync(It.IsAny<Func<CountDescriptor<CostSearchItem>, ICountRequest>>(), default(CancellationToken)))
                .ReturnsAsync(countResponse.Object);
        }

        [Test, Order(1)]
        public async Task UserCostFilter_SavesNewCostFilter_correctly()
        {
            // Arrange
            var url = $"v1/costfilter/{_user.Id}";

            var request = new CostFilterModel
            {
                Name = FilterName,
                SearchQuery = "{\"pageSize\": 10, \"pageNumber\": 1, \"contentType\": [\"Audio\", \"Photography\", \"Digital\", \"Distribution\"]}",
                UserId = _user.Id,
                Id = Guid.Empty              
            };

            // Act
            var result = await Browser.Put(url, c =>
            {
                c.User(_user);
                c.JsonBody(request);
            });

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);

            url = $"v1/costfilter/{_user.Id}";
            result = await Browser.Get(url, c =>
            {
                c.User(_user);
            });
            var userCostFilters = Deserialize<List<CostFilterModel>>(result, HttpStatusCode.OK);
            _filter = userCostFilters.FirstOrDefault(f => f.Name == FilterName);

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            _filter.Should().NotBeNull();
        }

        [Test, Order(2)]
        public async Task UserCostFilter_GetCostFilter_correctly()
        {
            var url = $"v1/costfilter/{_user.Id}";

            var result = await Browser.Get(url, c =>
            {
                c.User(_user);
            });

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            var userCostFilters = Deserialize<List<CostFilterModel>>(result, HttpStatusCode.OK);
            userCostFilters.Should().NotBeNull();
            userCostFilters.Should().HaveCount(2);
        }

        [Test, Order(3)]
        public async Task UserCostFilter_DeletesCostFilter_correctly()
        {
            var url = $"v1/costfilter/{_user.Id}/{_filter.Id}";

            var result = await Browser.Delete(url, c =>
            {
                c.User(_user);
            });

            result.StatusCode.Should().Be(HttpStatusCode.OK);

            url = $"v1/costfilter/{_user.Id}";
            result = await Browser.Get(url, c =>
            {
                c.User(_user);
            });
            var userCostFilters = Deserialize<List<CostFilterModel>>(result, HttpStatusCode.OK);

            Assert.True(userCostFilters == null || userCostFilters.All(f => f.Name != FilterName));
        }
    }
}
