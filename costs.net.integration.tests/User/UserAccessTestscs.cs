namespace costs.net.integration.tests.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Builders;
    using core.Models.ACL;
    using core.Models.Response;
    using core.Models.User;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework;
    using tests;
    using Constants = core.Constants;

    [TestFixture]
    public class UserAccessTestscs : BaseCostIntegrationTest
    {
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
        public async Task GrantAccessRoles()
        {
            // Arrange
            // Default budget region of the cost is AsiaPacific ("AAK (Asia)")
            await CreateCostEntity(User);
            var budgetRegionName = plugins.Constants.BudgetRegion.AsiaPacific;

            var pgBuName = $"pg_bu_userAccess_{Guid.NewGuid()}";
            var pgBu = new AgencyBuilder()
                .WithName(pgBuName)
                .WithGdamAgencyId()
                .WithPrimaryCurrency(_defaultCurrecy.Id)
                .WithCountry(_country)
                .MakePrime(_module)
                .Build();
            EFContext.Agency.Add(pgBu);

            var agencyName = $"pg_agency_userAccess_{Guid.NewGuid()}";
            var agency = new AgencyBuilder()
                .WithName(agencyName)
                .WithGdamAgencyId()
                .WithPrimaryCurrency(_defaultCurrecy.Id)
                .WithCountry(_country)
                .Build();
            EFContext.Agency.Add(agency);

            // Create users
            var name = $"{Guid.NewGuid()}bob_userAccess_1";
            var user = new CostUser
            {
                FirstName = name,
                LastName = name,
                FullName = name,
                AgencyId = agency.Id,
                GdamUserId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 24),
                Email = name,
                AbstractType = _root
            };
            EFContext.CostUser.Add(user);
            await EFContext.SaveChangesAsync();

            // Act
            var model = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        BusinessRoleId = _agencyAdmin.Id,
                        ObjectType = Constants.AccessObjectType.Region,
                        LabelName = budgetRegionName
                    }
                }
            };

            var response = await GrantAccess(user.Id, model);

            // Assert
            response.Success.Should().BeTrue();

            var userUserGroup = await EFContext.UserUserGroup
                .Include(uug => uug.UserGroup)
                .FirstOrDefaultAsync(uug =>
                    uug.UserId == user.Id
                    && uug.UserGroup.Label == budgetRegionName
                    );

            userUserGroup.Should().Should().NotBeNull();
            userUserGroup.UserGroup.Label.Should().NotBeNull();
            userUserGroup.UserGroup.Label.Should().Be(budgetRegionName);
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
    }
}