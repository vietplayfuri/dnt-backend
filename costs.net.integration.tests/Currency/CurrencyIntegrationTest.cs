namespace costs.net.integration.tests.Currency
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.Currencies;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class CurrencyIntegrationTest : BaseIntegrationTest
    {
        [Test]
        public async Task CreateDefaultCurrencyValidationTest()
        {
            var user = await CreateUser($"{Guid.NewGuid()}Bob", Roles.ClientAdmin);

            var model = new CreateCurrencyModel
            {
                Description = "United States Dollar",
                Symbol = "$"
            };

            var createResult = await Browser.Post("/v1/currency", with =>
            {
                with.JsonBody(model);
                with.User(user);
            });

            createResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task SetDefaultCurrencyNotFoundTest()
        {
            var user = await CreateUser($"{Guid.NewGuid()}Bob", Roles.ClientAdmin);

            var id = Guid.NewGuid();

            var setDefaultResult = await Browser.Put($"/v1/currency/{id}/default", w => w.User(user));

            setDefaultResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task TestCreate()
        {
            var user = await CreateUser($"{Guid.NewGuid()}Bob", Roles.ClientAdmin);
            var createModel = new CreateCurrencyModel
            {
                Code = "CAD",
                Description = "Canadian Dollar",
                Symbol = "$",
            };

            var createResult = await Browser.Post("/v1/currency", with =>
            {
                with.JsonBody(createModel);
                with.User(user);
            });

            Deserialize<Currency>(createResult, HttpStatusCode.Created);
        }

        [Test]
        public async Task UpdateCurrencyWithMissingFieldsInvalidModelTest()
        {
            var user = await CreateUser($"{Guid.NewGuid()}Bob", Roles.ClientAdmin);

            var model = new UpdateCurrencyModel
            {
                Description = "Australian Dollar"
            };

            var createResult = await Browser.Post("/v1/currency", with =>
            {
                with.JsonBody(model);
                with.User(user);
            });

            createResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}