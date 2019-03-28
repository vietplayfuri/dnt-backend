namespace costs.net.elasticSearch.integration.tests
{
    using System;
    using System.Threading.Tasks;
    using core;
    using core.Builders;
    using core.Builders.Response;
    using core.Builders.Search;
    using core.Models;
    using core.Models.CostUser;
    using core.Models.User;
    using core.Services.Search;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Builders.Search;

    [TestFixture]
    public class UserSearchServiceTests : BaseElasticSearchServiceTests
    {
        private IUserQueryBuilder _userQueryBuilder;
        private UserIdentity _userIdentity;

        private UserSearchService _userSearchService;

        protected override async Task CreateIndexes()
        {
            await ElasticSearchIndexService.CreateIndices(Constants.ElasticSearchIndices.CostUsersIndexName);
        }

        protected override async Task OneTimeSetup()
        {
            _userQueryBuilder = new PgUserQueryBuilder(EFContext);

            var identityUserId = Guid.NewGuid();
            _userIdentity = new UserIdentity
            {
                Id = identityUserId
            };
            EFContext.CostUser.Add(new CostUser
            {
                Id = identityUserId
            });
            await EFContext.SaveChangesAsync();

            _userSearchService = new UserSearchService(
                ElasticClient,
                new[]
                {
                    new Lazy<IUserQueryBuilder, PluginMetadata>(
                        () => _userQueryBuilder,
                        new PluginMetadata { BuType = BuType.Pg }
                    )
                },
                AppSettingsOptionsMock.Object,
                EFContext
            );
        }

        [Test]
        public async Task SearchCostUsers_WhenExactMatchByFullName_ShouldMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fullName = $"Firstname Lastname - {userId}";
            await AddToIndex(new CostUserSearchItem
            {
                Id = userId.ToString(),
                Version = 1,
                FullName = fullName
            }, Constants.ElasticSearchIndices.CostUsersIndexName);

            var query = new CostUserQuery
            {
                SearchText = fullName
            };

            // Act
            var users = await _userSearchService.SearchCostUsers(query, _userIdentity);

            // Assert
            users.Count.Should().Be(1);
        }

        [Test]
        public async Task SearchCostUsers_WhenFullNamePrefixPartiallyMatch_ShouldNotMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fullName = $"Firstname Lastname - {userId}";
            await AddToIndex(new CostUserSearchItem
            {
                Id = userId.ToString(),
                Version = 1,
                FullName = fullName
            }, Constants.ElasticSearchIndices.CostUsersIndexName);

            var query = new CostUserQuery
            {
                SearchText = "Firstname Lastname239487"
            };

            // Act
            var users = await _userSearchService.SearchCostUsers(query, _userIdentity);

            // Assert
            users.Count.Should().Be(0);
        }

        [Test]
        public async Task SearchCostUsers_WhenPrefixMatchByFullName_ShouldMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var fullName = $"Firstname Lastname - 1 - {userId}";
            await AddToIndex(new CostUserSearchItem
            {
                Id = userId.ToString(),
                Version = 1,
                FullName = fullName
            }, Constants.ElasticSearchIndices.CostUsersIndexName);

            var query = new CostUserQuery
            {
                SearchText = "Firstname Lastname - 1"
            };

            // Act
            var users = await _userSearchService.SearchCostUsers(query, _userIdentity);

            // Assert
            users.Count.Should().Be(1);
        }

        [Test]
        public async Task SearchCostUsers_WhenPrefixMatchByLastname_ShouldMatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var firstname = $"Firstname - {userId}";
            var lastname = $"Lastname - {userId}";
            await AddToIndex(new CostUserSearchItem
            {
                Id = userId.ToString(),
                Version = 1,
                FirstName = firstname,
                LastName = lastname,
                FullName = $"{firstname} {lastname}"
            }, Constants.ElasticSearchIndices.CostUsersIndexName);

            var query = new CostUserQuery
            {
                SearchText = "Last"
            };

            // Act
            var users = await _userSearchService.SearchCostUsers(query, _userIdentity);

            // Assert
            users.Count.Should().Be(1);
        }
    }
}
