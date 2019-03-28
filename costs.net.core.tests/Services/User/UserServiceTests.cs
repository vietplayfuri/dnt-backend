namespace costs.net.core.tests.Services.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Builders;
    using Builders.Response;
    using core.Models;
    using core.Models.Gdam;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.Agency;
    using core.Services.Costs;
    using core.Services.Events;
    using core.Services.Module;
    using core.Services.Search;
    using core.Services.User;
    using dataAccess;
    using dataAccess.Entity;
    using ExternalResource.Gdam;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Services;
    using plugins.PG.Services.Agency;
    using Serilog;
    using Agency = dataAccess.Entity.Agency;
    using Module = core.Models.AbstractTypes.Module;

    [TestFixture]
    public class UserServiceTests
    {
        [SetUp]
        public void Init()
        {
            _mapper = new Mock<IMapper>();
            _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
            _moduleServiceMock = new Mock<IModuleService>();
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();

            _appsettings = new Mock<IOptions<AppSettings>>();
            _appsettings.Setup(a => a.Value).Returns(new AppSettings { AdminUser = "4ef31ce1766ec96769b399c0", CostsAdminUserId = "dcc8c610-5eb5-473f-a5f7-b7d5d3ee9b55" });

            _logger = new Mock<ILogger>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _eventServiceMock = new Mock<IEventService>();
            _agencyServiceMock = new Mock<IAgencyService>();
            _pgAgencyServiceMock = new Mock<IPgAgencyService>();
            _gdamClientMock = new Mock<IGdamClient>();

            _pgUserService = new PgUserService(
                _efContext,
                _mapper.Object,
                _permissionServiceMock.Object,
                _logger.Object,
                _costStageRevisionServiceMock.Object,
                _activityLogServiceMock.Object,
                _eventServiceMock.Object,
                _pgAgencyServiceMock.Object
            );
            _userService = new UserService(
                _mapper.Object,
                _permissionServiceMock.Object,
                _logger.Object,
                _appsettings.Object,
                _efContext,
                _elasticSearchServiceMock.Object,
                new[]
                {
                    new Lazy<IPgUserService, PluginMetadata>(
                        () => _pgUserService, new PluginMetadata { BuType = BuType.Pg })
                },
                _activityLogServiceMock.Object,
                _agencyServiceMock.Object,
                _gdamClientMock.Object
            );
        }

        private IUserService _userService;
        private IPgUserService _pgUserService;
        private Mock<IModuleService> _moduleServiceMock;
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private Mock<ICostStageRevisionService> _costStageRevisionServiceMock;
        private Mock<IOptions<AppSettings>> _appsettings;
        private EFContext _efContext;
        private Mock<IMapper> _mapper;
        private Mock<ILogger> _logger;
        private Mock<IPermissionService> _permissionServiceMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IActivityLogService> _activityLogServiceMock;
        private Mock<IPgAgencyService> _pgAgencyServiceMock;
        private Mock<IAgencyService> _agencyServiceMock;
        private Mock<IGdamClient> _gdamClientMock;


        private UpdateUserModel Setup(Guid businessRoleId, Guid userId, Guid newRoleId, Agency agency, List<AbstractType> abstractTypes, Guid accessTypeId, out CostUser dbUser)
        {
            dbUser = new CostUser
            {
                Id = userId,
                Agency = agency
            };

            var model = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        BusinessRoleId = businessRoleId,
                        ObjectType = Constants.AccessObjectType.Client
                    }
                },
                ApprovalLimit = 99999
            };

            var abstractTypez = abstractTypes.AsQueryable();
            var costUsers = new List<CostUser> { dbUser }.AsQueryable();
            var agencies = new List<Agency> { agency }.AsQueryable();
            var businessRoles = new List<BusinessRole>
            {
                new BusinessRole { Id = businessRoleId, Value = "Cost Viewer", Key = "Cost Viewer", RoleId = newRoleId },
                new BusinessRole { Key = plugins.Constants.BusinessRole.AdstreamAdmin }
            }.AsQueryable();
            var userUserGroups = new List<UserUserGroup>
            {
                new UserUserGroup
                {
                    UserId = Guid.NewGuid()
                }
            }.AsQueryable();
            _efContext.AbstractType.AddRange(abstractTypez);
            _efContext.CostUser.AddRange(costUsers);
            _efContext.Agency.AddRange(agencies);
            _efContext.BusinessRole.AddRange(businessRoles);
            _efContext.UserUserGroup.AddRange(userUserGroups);
            _efContext.SaveChanges();

            _permissionServiceMock.Setup(
                p =>
                    p.GrantUserAccess<AbstractType>(It.IsAny<Guid>(), It.IsAny<Guid>(),
                        new CostUser { Id = userId },
                        It.IsAny<BuType>(), It.IsAny<Guid?>(), It.IsAny<string>(),
                        It.IsAny<bool>()));

            _permissionServiceMock.Setup(p => p.RevokeAccessForSubjectWithRole(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString() });

            var abstractType = abstractTypes.First();
            _moduleServiceMock.Setup(a => a.GetClientModulePerUserAsync(It.IsAny<CostUser>())).ReturnsAsync(new Module
            {
                Id = abstractType.Id,
                BuType = (BuType)abstractType.Module.ClientType,
                Key = abstractType.Module.Key,
                Name = abstractType.Module.Name
            });

            _mapper.Setup(m => m.Map<CostUserSearchItem>(It.IsAny<CostUser>())).Returns(new CostUserSearchItem());
            _mapper.Setup(m => m.Map<AgencySearchItem>(It.IsAny<AbstractType>())).Returns(new AgencySearchItem());
            _elasticSearchServiceMock.Setup(a => a.UpdateSearchItem(It.IsAny<AgencySearchItem>(), Constants.ElasticSearchIndices.AgencyIndexName)).Returns(Task.CompletedTask);
            _elasticSearchServiceMock.Setup(a => a.UpdateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName)).Returns(Task.CompletedTask);
            return model;
        }

        [Test]
        public async Task AddUserToDb_WhenUserHasBeenDisabled_ShouldMarkUserAsDisableInDb_AndIncrementVersion_AndReplicateToElastic()
        {
            //Setup
            var gdamUserId = Guid.NewGuid().ToString();
            var a5User = new GdamUser
            {
                _id = gdamUserId,
                _cm = new Cm
                {
                    View = new View
                    {
                        Access = new Access
                        {
                            adcost = false,
                            folders = true
                        }
                    },
                    Common = new User { Disabled = false }
                }
            };

            var costUser = new CostUser
            {
                GdamUserId = gdamUserId,
                Disabled = false,
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        Id = Guid.NewGuid(),
                        Created = DateTime.Now,
                        UserGroup = new UserGroup
                        {
                            Name = "name",
                            Created = DateTime.Now,
                            Disabled = false,
                            Role = new Role()
                        }
                    }
                },
                Version = 0,
                Agency = new Agency()
            };
            _efContext.Add(costUser);
            _efContext.SaveChanges();

            //Act
            await _userService.AddUserToDb(a5User);

            //Assert
            costUser.Disabled.Should().BeTrue();
            costUser.Version.Should().Be(1);
            _elasticSearchServiceMock.Verify(es =>
                es.UpdateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName),
                Times.Once
            );
        }

        [Test]
        public async Task Get_always_shouldQueryUserByEmailFromDB()
        {
            // Arrange
            const string email = "test.test@test.com";
            var dbUser = new CostUser
            {
                Email = email,
                Id = Guid.NewGuid()
            };
            _efContext.CostUser.Add(dbUser);
            _efContext.SaveChanges();

            _mapper.Setup(m => m.Map<CostUserModel>(It.Is<CostUser>(u => u.Email == dbUser.Email))).Returns(new CostUserModel { Email = dbUser.Email });

            // Act
            var user = await _userService.Get(email, BuType.Pg);

            // Assert
            user.Email.Should().Be(email);
        }

        [Test]
        public async Task Update_Non_PG_User_HasModified_ShouldPersistUpdatedUser()
        {
            //Setup
            var loggedInUserId = Guid.NewGuid();
            var businessRoleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var newRoleId = Guid.NewGuid();
            var accessDetailTypeId = Guid.NewGuid();
            CostUser dbUser;
            var loggedInUser = new UserIdentity
            {
                Id = loggedInUserId
            };
            var actioningUser = new CostUser
            {
                Id = loggedInUserId
            };
            var agency = new Agency
            {
                Labels = new[] { "costPG" }
            };
            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Id = accessDetailTypeId,
                    Type = Constants.AccessObjectType.Module,
                    Module = new dataAccess.Entity.Module
                    {
                        ClientType = ClientType.Pg,
                        Name = "Procter & Gamble",
                        ClientTag = "costPG",
                        Key = "P&G"
                    }
                },
                new AbstractType
                {
                    Type = Constants.AccessObjectType.Module,
                    Module = new dataAccess.Entity.Module
                    {
                        ClientType = ClientType.Root,
                        Name = "Adstream",
                        ClientTag = "costPG",
                        Key = "Adstream"
                    }
                },
                new AbstractType
                {
                    Type = Constants.AccessObjectType.Agency,
                    Agency = new Agency
                    {
                        Name = "PG Parent Agency",
                        Labels = new[] { "CM_Prime_P&G" }
                    }
                }
            };
            var model = Setup(businessRoleId, userId, newRoleId, agency, abstractTypes, accessDetailTypeId, out dbUser);

            _permissionServiceMock.Setup(a => a.CreateDomainNode(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { $"{Guid.NewGuid()}" });

            _efContext.CostUser.AddRange(actioningUser);
            _efContext.SaveChanges();
            _permissionServiceMock.Setup(a => a.CheckHasAccess(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            _pgAgencyServiceMock.Setup(a => a.GetOrCreatePseudoAgencies(It.IsAny<Agency[]>()))
                .ReturnsAsync(abstractTypes
                    .Where(at => at.Type == Constants.AccessObjectType.Agency)
                    .Select(at => new AbstractType())
                    .ToList
                );

            //Act
            var result = await _userService.Update(loggedInUser, userId, model, BuType.Pg);

            //Assert

            result.Success.Equals(true);
            result.Messages.First().Should().Be("User updated.");

            var verifyUser = _efContext.CostUser.FirstOrDefault(a => a.Id == dbUser.Id);
            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().BusinessRole.RoleId.Should().Be(newRoleId);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(businessRoleId);

            verifyUser.UserBusinessRoles.First().ObjectId.Should().NotBe(accessDetailTypeId);
            verifyUser.ApprovalLimit.Should().Be(99999);
        }

        [Test]
        public async Task Update_PG_User_Has_Modified_ShouldPersistUpdatedUser()
        {
            //Setup
            var loggedInUserId = Guid.NewGuid();
            var businessRoleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var newRoleId = Guid.NewGuid();
            var accessDetailTypeId = Guid.NewGuid();
            var loggedInUser = new UserIdentity
            {
                Id = loggedInUserId
            };
            var actioningUser = new CostUser
            {
                Id = loggedInUserId
            };
            CostUser dbUser;
            var agency = new Agency
            {
                Id = Guid.NewGuid(),
                Name = "Test Agency",
                Labels = new[] { "CM_Prime_P&G", "costPG" }
            };
            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Id = accessDetailTypeId,
                    Type = Constants.AccessObjectType.Module,
                    Module = new dataAccess.Entity.Module
                    {
                        Id = Guid.NewGuid(),
                        ClientType = ClientType.Pg,
                        Name = "Procter & Gamble",
                        ClientTag = "costPG",
                        Key = "P&G"
                    }
                }
            };
            var model = Setup(businessRoleId, userId, newRoleId, agency, abstractTypes, accessDetailTypeId, out dbUser);
            _efContext.CostUser.Add(actioningUser);
            _efContext.SaveChanges();
            _permissionServiceMock.Setup(a => a.CheckHasAccess(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var result = await _userService.Update(loggedInUser, userId, model, BuType.Pg);

            //Assert
            result.Success.Equals(true);
            result.Messages.First().Should().Be("User updated.");

            var verifyUser = _efContext.CostUser.FirstOrDefault(a => a.Id == dbUser.Id);
            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().BusinessRole.RoleId.Should().Be(newRoleId);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(businessRoleId);

            verifyUser.UserBusinessRoles.First().ObjectId.Should().Be(accessDetailTypeId);

            verifyUser.ApprovalLimit.Should().Be(99999);
        }

        [Test]
        public async Task Update_User_Doesnt_Exist()
        {
            //Setup

            var a5User = new GdamUser
            {
                _id = Guid.NewGuid().ToString(),
                _cm = new Cm
                {
                    View = new View
                    {
                        Access = new Access
                        {
                            adcost = false,
                            folders = true
                        }
                    },
                    Common = new User { Disabled = false }
                }
            };

            //Act
            await _userService.AddUserToDb(a5User);

            //Assert
            _mapper.Verify(a => a.Map<CostUserSearchItem>(It.IsAny<CostUser>()), Times.Never);
            _elasticSearchServiceMock.Verify(a => a.CreateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName), Times.Never);
            _elasticSearchServiceMock.Verify(a => a.UpdateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName), Times.Never);
            _logger.Verify(a => a.Information($"Disabling User with gdam Id {a5User._id} since access was revoked!"), Times.Never);
        }

        [Test]
        public async Task Update_User_RevokeCosts_Does_Exist_Elastic_Updated()
        {
            //Setup
            var gdamUserId = Guid.NewGuid().ToString();
            var a5User = new GdamUser
            {
                _id = gdamUserId,
                _cm = new Cm
                {
                    View = new View
                    {
                        Access = new Access
                        {
                            adcost = false,
                            folders = true
                        }
                    },
                    Common = new User { Disabled = false }
                }
            };

            var costUser = new CostUser
            {
                GdamUserId = gdamUserId,
                Disabled = false,
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        Id = Guid.NewGuid(),
                        Created = DateTime.Now,
                        UserGroup = new UserGroup
                        {
                            Name = "name",
                            Created = DateTime.Now,
                            Disabled = false,
                            Role = new Role()
                        }
                    }
                },
                Agency = new Agency()
            };
            _efContext.Add(costUser);
            _efContext.SaveChanges();

            //Act
            await _userService.AddUserToDb(a5User);

            //Assert
            _mapper.Verify(a => a.Map<CostUserSearchItem>(It.IsAny<CostUser>()), Times.Once);
            _elasticSearchServiceMock.Verify(a => a.CreateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName), Times.Never);
            _elasticSearchServiceMock.Verify(a => a.UpdateSearchItem(It.IsAny<CostUserSearchItem>(), Constants.ElasticSearchIndices.CostUsersIndexName), Times.Once);
            _logger.Verify(a => a.Information($"Disabling User with gdam Id {a5User._id} since access was revoked!"), Times.Once);
            _logger.Verify(a => a.Information($"Updated user with Id {costUser.Id}"), Times.Once);
        }
    }
}
