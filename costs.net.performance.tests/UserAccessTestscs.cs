namespace costs.net.performance.tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.Response;
    using core.Models.User;
    using dataAccess.Entity;
    using FluentAssertions;
    using integration.tests;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using Constants = core.Constants;

    [TestFixture]
    public class UserAccessTestscs : BaseCostIntegrationTest
    {
        private static readonly string PgOwnerLabel = $"{Constants.BusinessUnit.CostModulePrimaryLabelPrefix}P&G";
        private Country _country;
        private Currency _defaultCurrecy;
        private AbstractType _module;
        private AbstractType _root;
        private BusinessRole _agencyAdmin;

        [SetUp]
        public void Setup()
        {
            _country = EFContext.Country.First();
            _defaultCurrecy = EFContext.Currency.First(c => c.DefaultCurrency);
            _module = EFContext.AbstractType
                .Include(a => a.Module)
                .FirstOrDefault(a => 
                    a.Type == AbstractObjectType.Module.ToString() 
                    && a.Module.ClientType == ClientType.Pg);
            _root = EFContext.AbstractType.First(a => a.Id == a.ParentId);

            _agencyAdmin = EFContext.BusinessRole.First(br => br.Key == plugins.Constants.BusinessRole.AgencyAdmin);
        }

        [Test]
        //[TestCase(1, 3, 1, 1)]
        //[TestCase(100, 100, 100, 100)]
        [TestCase(10, 300, 300, 300)]
        public async Task GrantAccessRoles(
            int usersCount, 
            int costsCount,
            int pgBuCount,
            int agencyBuCount)
        {
            // Arrange

            // Create costs
            var costs = new List<Cost>();
            for (var i = 0; i < costsCount; ++i)
            {
                costs.Add(await CreateCostEntity(User));
            }

            // Create P&G business units
            var pgBus = new List<Agency>();
            for (var i = 0; i < pgBuCount; ++i)
            {
                pgBus.Add(CreateAgency($"pg_agency_{i}", true));
            }

            // Create agencies
            var agencies = new List<Agency>();
            for (var i = 0; i < agencyBuCount; ++i)
            {
                agencies.Add(CreateAgency($"v{i}", false));
            }

            // Create users
            var random = new Random();
            var users = new List<CostUser>();
            for (var i = 0; i < usersCount; ++i)
            {
                var name = $"bob_{i}";
                users.Add(new CostUser
                {
                    FirstName = name,
                    LastName = name,
                    FullName = name,
                    AgencyId = agencies[random.Next(0, agencyBuCount)].Id,
                    GdamUserId = name,
                    Email = name,
                    AbstractType = _root
                });
                //users.Add(await CreateUser(, Roles.AgencyAdmin));
            }
            EFContext.CostUser.AddRange(users);
            await EFContext.SaveChangesAsync();

            // Assert
            costs.Should().HaveCount(costsCount);

            // Act
            var model = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        BusinessRoleId = _agencyAdmin.Id,
                        ObjectType = Constants.AccessObjectType.Client
                    }
                }
            };

            var sw = new Stopwatch();
            sw.Start();

            var response = await GrantAccess(users[0].Id, model);

            sw.Stop();

            // Assert
            response.Success.Should().BeTrue();
            sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(4));
        }

        private async Task<OperationResponse> GrantAccess(Guid userId, UpdateUserModel model)
        {
            var response = await Browser.Put($"/v1/users/{userId}", w =>
            {
                w.User(User);
                w.JsonBody(model);
            });
            return Deserialize<OperationResponse>(response, HttpStatusCode.OK);
        }

        private Agency CreateAgency(string name, bool prime)
        {
            var agency = new Agency
            {
                Name = name,
                CountryId = _country.Id,
                GdamAgencyId = name,
                Labels = prime ? new [] { PgOwnerLabel } : new string[0],
                PrimaryCurrency = _defaultCurrecy.Id
            };
            if (prime)
            {
                agency.AbstractTypes = new List<AbstractType>
                {
                    new AbstractType
                    {
                        ObjectId = agency.Id,
                        Parent = _module,
                        Type = AbstractObjectType.Agency.ToString()
                    }
                };
            }
            EFContext.Add(agency);
            return agency;
        }
    }
}