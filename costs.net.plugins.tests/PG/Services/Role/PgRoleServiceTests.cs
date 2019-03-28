namespace costs.net.plugins.tests.PG.Services.Role
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core;
    using core.Models.ACL;
    using core.Models.Utils;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Services.Role;

    [TestFixture]
    public class PgRoleServiceTests
    {
        private Mock<EFContext> _efContextMock;
        private AppSettings _appSettings;
        private PgRoleService _sut;

        private readonly string[] _allBusinessRoles =
        {
            plugins.Constants.BusinessRole.Ipm,
            plugins.Constants.BusinessRole.BrandManager,
            plugins.Constants.BusinessRole.FinanceManager,
            plugins.Constants.BusinessRole.RegionSupport,
            plugins.Constants.BusinessRole.PurchasingSupport,
            plugins.Constants.BusinessRole.CostConsultant,
            plugins.Constants.BusinessRole.InsuranceUser,
            plugins.Constants.BusinessRole.RegionalAgencyUser,
            plugins.Constants.BusinessRole.AgencyOwner,
            plugins.Constants.BusinessRole.CentralAdaptationSupplier,
            plugins.Constants.BusinessRole.AgencyFinanceManager,
            plugins.Constants.BusinessRole.GovernanceManager,
            plugins.Constants.BusinessRole.AgencyAdmin,
            plugins.Constants.BusinessRole.AdstreamAdmin

        };
        private readonly string[] _costOwnerBusinessRoles =
        {
            plugins.Constants.BusinessRole.Ipm,
            plugins.Constants.BusinessRole.BrandManager,
            plugins.Constants.BusinessRole.FinanceManager,
            plugins.Constants.BusinessRole.PurchasingSupport,
            plugins.Constants.BusinessRole.InsuranceUser,
            plugins.Constants.BusinessRole.RegionSupport,
            plugins.Constants.BusinessRole.GovernanceManager
        };
        private readonly string[] _agencyUserBusinessRoles =
        {
            plugins.Constants.BusinessRole.RegionalAgencyUser,
            plugins.Constants.BusinessRole.AgencyOwner,
            plugins.Constants.BusinessRole.AgencyFinanceManager,
            plugins.Constants.BusinessRole.AgencyAdmin,
            plugins.Constants.BusinessRole.CentralAdaptationSupplier
        };

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
            _appSettings = new AppSettings();
            var appSettingsMock = new Mock<IOptions<AppSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_appSettings);

            _sut = new PgRoleService(_efContextMock.Object, appSettingsMock.Object);

            _efContextMock.MockAsyncQueryable(
                _allBusinessRoles.Select(r => new BusinessRole { Key = r }).AsQueryable(), 
                d => d.BusinessRole
                );
        }

        [Test]
        public async Task GetBusinessRoles_When_AgencyIsCostModuleOwner_Should_ReturnCorrespondingBusinessRoles()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new CostUser
            {
                Id = userId,
                Agency = new Agency
                {
                    Labels = new[] { Constants.BusinessUnit.CostModulePrimaryLabelPrefix }
                }
            };
            _efContextMock.MockAsyncQueryable(new[] { user }.AsQueryable(), d => d.CostUser);

            // Act
            var businessRoles = await _sut.GetBusinessRoles(currentUserId, userId);

            // Assert
            businessRoles.Should().NotBeNull();
            businessRoles.Should().HaveSameCount(_costOwnerBusinessRoles);
            businessRoles.Select(br => br.Key).ShouldBeEquivalentTo(_costOwnerBusinessRoles);
        }

        [Test]
        public async Task GetBusinessRoles_When_AgencyUser_Should_ReturnCorrespondingBusinessRoles()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new CostUser
            {
                Id = userId,
                Agency = new Agency()
            };
            _efContextMock.MockAsyncQueryable(new[] { user }.AsQueryable(), d => d.CostUser);

            // Act
            var businessRoles = await _sut.GetBusinessRoles(currentUserId, userId);

            // Assert
            businessRoles.Should().NotBeNull();
            businessRoles.Should().HaveSameCount(_agencyUserBusinessRoles);
            businessRoles.Select(br => br.Key).ShouldBeEquivalentTo(_agencyUserBusinessRoles);
        }

        [Test]
        public async Task GetBusinessRoles_When_AgencyIsCostModuleOwner_And_CurrentUserHasAdstreanAdminRole_Should_ReturnCorrespondingRoles()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var rootId = Guid.NewGuid();
            var currentUser = new CostUser
            {
                Id = currentUserId,
                Agency = new Agency(),
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = plugins.Constants.BusinessRole.AdstreamAdmin
                        },
                        ObjectId = rootId,
                        ObjectType = ObjectTypes.AbstractType
                    }
                }
            };
            var user = new CostUser
            {
                Id = userId,
                Agency = new Agency
                {
                    Labels = new[] { Constants.BusinessUnit.CostModulePrimaryLabelPrefix }
                }
            };
            _efContextMock.MockAsyncQueryable(new[] { currentUser, user }.AsQueryable(), d => d.CostUser);
            _efContextMock.MockAsyncQueryable(new List<AbstractType> { new AbstractType { Id = rootId, ParentId = rootId } }.AsQueryable(), d => d.AbstractType);

            var expectedRoles = _costOwnerBusinessRoles.Concat(new [] {plugins.Constants.BusinessRole.AdstreamAdmin});

            // Act
            var businessRoles = await _sut.GetBusinessRoles(currentUserId, userId);

            // Assert
            businessRoles.Should().NotBeNull();
            businessRoles.Should().HaveSameCount(expectedRoles);
            businessRoles.Select(br => br.Key).ShouldBeEquivalentTo(expectedRoles);
        }

        [Test]
        public async Task GetBusinessRoles_WhenPlatformAdmin_Should_ReturnAllBusinessRoles()
        {
            // Arrange
            var currentUserId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new CostUser
            {
                Id = userId,
                Agency = new Agency()
            };
            _efContextMock.MockAsyncQueryable(new[] { user }.AsQueryable(), d => d.CostUser);
            _appSettings.CostsAdminUserId = currentUserId.ToString();

            // Act
            var businessRoles = await _sut.GetBusinessRoles(currentUserId, userId);

            // Assert
            businessRoles.Should().NotBeNull();
            businessRoles.Should().HaveSameCount(_allBusinessRoles);
            businessRoles.Select(br => br.Key).ShouldBeEquivalentTo(_allBusinessRoles);
        }
    }
}