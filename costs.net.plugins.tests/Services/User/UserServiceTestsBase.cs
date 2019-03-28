namespace costs.net.plugins.tests.Services.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ads.Net.Acl;
    using Ads.Net.Acl.Api;
    using Ads.Net.Acl.Helper;
    using Ads.Net.Acl.Methods;
    using AutoMapper;
    using core.Models.ACL;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.Costs;
    using core.Services.Events;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins.PG.Form;
    using plugins.PG.Services;
    using plugins.PG.Services.Agency;
    using Serilog;

    [TestFixture]
    public class UserServiceTestsBase
    {
        [SetUp]
        public void Setup()
        {
            User = new UserIdentity
            {
                Id = _xUserId
            };
            var clientModule = new Module
            {
                Id = Guid.NewGuid(),
                ClientType = ClientType.Pg,
                Name = "Procter & Gamble",
                ClientTag = "costPG",
                Key = "P&G"
            };
            var agencies = new List<Agency>
            {
                new Agency
                {
                    Id = Guid.NewGuid(),
                    Name = "SmoName1",
                    Labels = new []{ "CM_Prime_P&G" }
                },
                new Agency
                {
                    Id = Guid.NewGuid(),
                    Name = "SmoName2",
                    Labels = new []{ "CM_Prime_P&G" }
                }
            };
            _abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Id = Guid.NewGuid(),
                    ObjectId = clientModule.Id,
                    Module = clientModule,
                    Type = core.Constants.AccessObjectType.Module
                },
                new AbstractType
                {
                    Id = Guid.NewGuid(),
                    ObjectId = agencies.First(a => a.Name == "SmoName1").Id,
                    Agency = agencies.First(a => a.Name == "SmoName1"),
                    Type = core.Constants.AccessObjectType.Agency
                },
                new AbstractType
                {
                    Id = Guid.NewGuid(),
                    ObjectId = agencies.First(a => a.Name == "SmoName2").Id,
                    Agency = agencies.First(a => a.Name == "SmoName2"),
                    Type = core.Constants.AccessObjectType.Agency
                }
            };
            _costStageReviDetailsFormFirstCost = new PgStageDetailsForm
            {
                BudgetRegion = new AbstractTypeValue
                {
                    Id = Guid.NewGuid(),
                    Key = "",
                    Name = "RegionOne"
                },
                SmoName = "SmoName1",
                SmoId = Guid.NewGuid().ToString()
            };
            _costStageReviDetailsFormSecondCost = new PgStageDetailsForm
            {
                BudgetRegion = new AbstractTypeValue
                {
                    Id = Guid.NewGuid(),
                    Key = "",
                    Name = "RegionTwo"
                },
                SmoName = "SmoName2",
                SmoId = Guid.NewGuid().ToString()
            };
            _costStageReviDetailsFormThirdCost = new PgStageDetailsForm
            {
                BudgetRegion = new AbstractTypeValue
                {
                    Id = Guid.NewGuid(),
                    Key = "",
                    Name = "RegionThree"
                },
                SmoName = "SmoName3",
                SmoId = Guid.NewGuid().ToString()
            };
            _costs = new List<Cost>
            {
                new Cost
                {
                    Id = Guid.NewGuid(),
                    CostNumber = "number1",
                    ProjectId = Guid.NewGuid(),
                    LatestCostStageRevision = new CostStageRevision
                    {
                        StageDetails = new CustomFormData
                        {
                            Data = JsonConvert.SerializeObject(_costStageReviDetailsFormFirstCost)
                        }
                    }
                },
                new Cost
                {
                    Id = Guid.NewGuid(),
                    CostNumber = "number2",
                    ProjectId = Guid.NewGuid(),
                    LatestCostStageRevision = new CostStageRevision
                    {
                        StageDetails = new CustomFormData
                        {
                            Data = JsonConvert.SerializeObject(_costStageReviDetailsFormSecondCost)
                        }
                    }
                },
                new Cost
                {
                    Id = Guid.NewGuid(),
                    CostNumber = "number3",
                    ProjectId = Guid.NewGuid(),
                    LatestCostStageRevision = new CostStageRevision
                    {
                        StageDetails = new CustomFormData
                        {
                            Data = JsonConvert.SerializeObject(_costStageReviDetailsFormThirdCost)
                        }
                    }
                }
            };

            var adminRole = new Role
            {
                Name = Roles.ClientAdmin,
                Subtype = "role",
                AbstractTypeId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            var costCreaterRole = new Role
            {
                Name = Roles.CostOwner,
                Subtype = "role",
                AbstractTypeId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            var costOwnerRole = new Role
            {
                Name = Roles.CostOwner,
                Subtype = "role",
                AbstractTypeId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            var costApproverRole = new Role
            {
                Name = Roles.CostApprover,
                Subtype = "role",
                AbstractTypeId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            var agencyAdminRole = new Role
            {
                Name = Roles.AgencyAdmin,
                Subtype = "role",
                AbstractTypeId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };

            var userBeingUpdated = new CostUser
            {
                Id = _userNoRoles,
                Email = "agencyemail@email.com",
                UserUserGroups = new List<UserUserGroup>(),
                UserBusinessRoles = new List<UserBusinessRole>(),
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            _businessRoles = new List<BusinessRole>
            {
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.GovernanceManager,
                    Value = Constants.BusinessRole.GovernanceManager,
                    RoleId = adminRole.Id,
                    Role = adminRole
                },
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.AgencyOwner,
                    Value = Constants.BusinessRole.AgencyOwner,
                    RoleId = costCreaterRole.Id,
                    Role = costCreaterRole
                },
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.AgencyFinanceManager,
                    Value = Constants.BusinessRole.AgencyFinanceManager,
                    RoleId = costCreaterRole.Id,
                    Role = costCreaterRole
                }     ,
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.FinanceManager,
                    Value = Constants.BusinessRole.FinanceManager,
                    RoleId = adminRole.Id,
                    Role = adminRole
                },
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.AdstreamAdmin,
                    Value = Constants.BusinessRole.AdstreamAdmin,
                    RoleId = costCreaterRole.Id,
                    Role = costCreaterRole
                },
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.CostConsultant,
                    Value = Constants.BusinessRole.CostConsultant,
                    RoleId = costApproverRole.Id,
                    Role = costApproverRole
                },
                new BusinessRole
                {
                    Id = Guid.NewGuid(),
                    Key = Constants.BusinessRole.AgencyAdmin,
                    Value = Constants.BusinessRole.AgencyAdmin,
                    RoleId = agencyAdminRole.Id,
                    Role = agencyAdminRole
                }
            };

            var userWithOneClientRole = new CostUser
            {
                Id = _UserWithOneRoleId,
                Email = "agencyemail2@email.com",

                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserId = _UserWithOneRoleId,
                        Id = Guid.NewGuid(),
                        UserGroup = new UserGroup
                        {
                            Id = Guid.NewGuid(),
                            ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id
                        }
                    }
                },

                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner),
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        Id = Guid.NewGuid(),
                        CostUserId = _UserWithOneRoleId,
                        ObjectType = core.Constants.AccessObjectType.Client,
                        ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            var userWithOneRegionRole = new CostUser
            {
                Id = _UserWithOneRegionRoleId,
                Email = "agencyemail23@email.com",

                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserId = _UserWithOneRoleId,
                        Id = Guid.NewGuid(),
                        UserGroup = new UserGroup
                        {
                            Id = Guid.NewGuid(),
                            ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id,
                            Label = "RegionOne"
                        }
                    }
                },

                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.FinanceManager),
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.FinanceManager).Id,
                        Id = Guid.NewGuid(),
                        CostUserId = _UserWithOneRegionRoleId,
                        Labels = new[] { "RegionOne" },
                        ObjectType = core.Constants.AccessObjectType.Region,
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            var userWithTwoRegionRoles = new CostUser
            {
                Id = _UserWithTwoRegionRolesId,
                Email = "agencyemail22@email.com",

                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserId = _UserWithTwoRegionRolesId,
                        Id = Guid.NewGuid(),
                        UserGroup = new UserGroup
                        {
                            Id = Guid.NewGuid(),
                            ObjectId = agencies.First(a => a.Name == "SmoName2").Id
                        }
                    }
                },

                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.FinanceManager),
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.FinanceManager).Id,
                        Id = Guid.NewGuid(),
                        CostUserId = _UserWithTwoRegionRolesId,
                        Labels = new[] { "RegionOne", "RegionTwo" },
                        ObjectType = core.Constants.AccessObjectType.Region,
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            var userWithTwoRoles = new CostUser
            {
                Id = _UserWithTwoRolesId,
                Email = "agencyemail3@email.com",

                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        Id = Guid.NewGuid(),
                        UserId = _UserWithTwoRolesId,
                        UserGroup = new UserGroup
                        {
                            ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id,
                            Name = "Name",
                            Id = Guid.NewGuid()
                        }
                    },
                    new UserUserGroup
                    {
                        Id = Guid.NewGuid(),
                        UserId = _UserWithTwoRolesId,
                        UserGroup = new UserGroup
                        {
                            Name = "Name2",
                            Id = Guid.NewGuid(),
                            Label = "RegionOne",
                            ObjectId = _costs.First(a => a.CostNumber == "number1").Id
                        }
                    }
                },

                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager),
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        Id = Guid.NewGuid(),
                        CostUserId = _UserWithTwoRolesId,
                        Labels = new[] { "RegionOne" },
                        ObjectType = core.Constants.AccessObjectType.Region
                    },
                    new UserBusinessRole
                    {
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner),
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        Id = Guid.NewGuid(),
                        CostUserId = _UserWithTwoRolesId,
                        ObjectType = core.Constants.AccessObjectType.Client,
                        ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            var userWithAgencyAdminRole = new CostUser
            {
                Id = UserWithAgencyAdminRoleId,
                Email = "userWithAgencyAdminRole@email.com",

                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserId = UserWithAgencyAdminRoleId,
                        Id = Guid.NewGuid(),
                        UserGroup = new UserGroup
                        {
                            Id = Guid.NewGuid(),
                            ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id
                        }
                    }
                },

                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        Id = Guid.NewGuid(),
                        BusinessRole = _businessRoles.First(a => a.Key == Constants.BusinessRole.AgencyAdmin),
                        BusinessRoleId = _businessRoles.First(a => a.Key == Constants.BusinessRole.AgencyAdmin).Id,
                        CostUserId = UserWithAgencyAdminRoleId,
                        ObjectType = core.Constants.AccessObjectType.Client,
                        ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG" },
                    Name = "AgencyName_11111"
                },
                ApprovalLimit = 0m,
                UserGroups = new string[0]
            };

            var userPerformingUpdate = new CostUser
            {
                Id = _xUserId,
                Email = "owneremail@email.com",
                UserUserGroups = new List<UserUserGroup>(),
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        Id = Guid.NewGuid(),
                        BusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.GovernanceManager)
                    }
                },
                Agency = new Agency
                {
                    Id = Guid.NewGuid(),
                    Labels = new[] { "costPG", "CM_Prime_P&G" },
                    Name = "P&GBU"
                },
                ApprovalLimit = 1111100m,
                UserGroups = new string[0]
            };
            _roles = new List<Role> { costOwnerRole, adminRole };
            _users = new List<CostUser>
            {
                userBeingUpdated,
                userPerformingUpdate,
                userWithOneClientRole,
                userWithTwoRoles,
                userWithOneRegionRole,
                userWithTwoRegionRoles,
                userWithAgencyAdminRole
            };

            EfContext = EFContextFactory.CreateInMemoryEFContextTest();

            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings { CostsAdminUserId = Guid.NewGuid().ToString() });
            _aclClientMock.Setup(a => a.Access).Returns(new Access(new AccessApi(new RequestHelper("http://localhost:9999/")), new FunctionHelper()));
            _aclClientMock.Setup(a => a.Get).Returns(new Get(new GetApi(new RequestHelper("http://localhost:9999/")), new FunctionHelper()));
            _pgUserService = new PgUserService(
                EfContext,
                _mapperMock.Object,
                _permissionServiceMock.Object,
                _loggerMock.Object,
                _costStageRevisionServiceMock.Object,
                ActivityLogServiceMock.Object,
                _eventServiceMock.Object,
                _pgAgencyServiceMock.Object);
        }

        protected List<BusinessRole> _businessRoles;
        protected List<Role> _roles;
        protected List<CostUser> _users;
        protected List<Cost> _costs;
        protected List<AbstractType> _abstractTypes;
        protected PgStageDetailsForm _costStageReviDetailsFormFirstCost;
        protected PgStageDetailsForm _costStageReviDetailsFormSecondCost;
        protected PgStageDetailsForm _costStageReviDetailsFormThirdCost;
        protected readonly Guid _xUserId = Guid.NewGuid();
        protected readonly Guid _userNoRoles = Guid.NewGuid();
        protected readonly Guid _UserWithOneRoleId = Guid.NewGuid();
        protected readonly Guid _UserWithTwoRolesId = Guid.NewGuid();
        protected readonly Guid _UserWithTwoRegionRolesId = Guid.NewGuid();
        protected readonly Guid _UserWithOneRegionRoleId = Guid.NewGuid();
        protected readonly Guid UserWithAgencyAdminRoleId = Guid.NewGuid();

        protected readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
        protected readonly Mock<IOptions<AppSettings>> _appSettingsMock = new Mock<IOptions<AppSettings>>();

        protected readonly Mock<IAclClient> _aclClientMock = new Mock<IAclClient>();

        protected readonly Mock<IEventService> _eventServiceMock = new Mock<IEventService>();
        protected readonly Mock<IPermissionService> _permissionServiceMock = new Mock<IPermissionService>();
        protected readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        protected readonly Mock<ICostStageRevisionService> _costStageRevisionServiceMock = new Mock<ICostStageRevisionService>();
        protected readonly Mock<IActivityLogService> ActivityLogServiceMock = new Mock<IActivityLogService>();
        protected readonly Mock<IPgAgencyService> _pgAgencyServiceMock = new Mock<IPgAgencyService>();
        protected PgUserService _pgUserService;
        protected UserIdentity User;
        protected EFContextTest EfContext;
    }
}