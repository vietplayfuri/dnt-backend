
namespace costs.net.plugins.tests.Services.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models.Regions;
    using core.Services.CustomData;
    using core.Services.Regions;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using plugins.PG.Models;
    using plugins.PG.Services.Costs;

    [TestFixture]
    public class CostUserServiceTests
    {
        private readonly Mock<IRegionsService> _regionsService = new Mock<IRegionsService>();
        private readonly Mock<ICustomObjectDataService> _customObjectDataServiceMock = new Mock<ICustomObjectDataService>();
        private CostUserService _costUserService;
        private EFContext _efContext;

        [SetUp]
        public void Setup()
        {
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _costUserService = new CostUserService(_efContext, _regionsService.Object, _customObjectDataServiceMock.Object);
        }

        [Test]
        public async Task GetApproverName_IPM()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId
            };
            var approverUserId = Guid.NewGuid();
            var expectedApproverName = "John Connor";
            var approverUser = new CostUser
            {
                Id = approverUserId,
                FullName = expectedApproverName
            };
            var approvalType = "IPM";
            
            _efContext.Add(approverUser);
            _efContext.Add(cost);
            _efContext.SaveChanges();

            //Act
            var result = await _costUserService.GetApprover(costId, approverUserId, approvalType);

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedApproverName);
        }

        [Test]
        public async Task GetApproverName_Brand()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                IsExternalPurchases = false
            };
            var approverUserId = Guid.NewGuid();
            var expectedApproverName = "Sarah Connor";
            var approverUser = new CostUser
            {
                Id = approverUserId,
                FullName = expectedApproverName
            };
            var approvalType = "Brand";

            _efContext.Add(approverUser);
            _efContext.Add(cost);
            _efContext.SaveChanges();

            //Act
            var result = await _costUserService.GetApprover(costId, approverUserId, approvalType);

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedApproverName);
        }

        [Test]
        public async Task GetApproverName_Coupa()
        {
            //Arrange
            var costId = Guid.NewGuid();
            var costStageRevisionId = Guid.NewGuid();
            var cost = new Cost
            {
                Id = costId,
                IsExternalPurchases = true,
                LatestCostStageRevisionId = costStageRevisionId
            };
            var expectedApproverName = "Sarah Connor";
            var approvalType = "Brand";
            var approverUserId = Guid.NewGuid();

            _efContext.Add(cost);
            _efContext.SaveChanges();
            var pgPaymentDetails = new PgPaymentDetails
            {
                IoNumberOwner = expectedApproverName
            };

            _customObjectDataServiceMock
                .Setup(c => c.GetCustomData<PgPaymentDetails>(costStageRevisionId, CustomObjectDataKeys.PgPaymentDetails))
                .Returns(Task.FromResult(pgPaymentDetails));

            //Act
            var result = await _costUserService.GetApprover(costId, approverUserId, approvalType);

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedApproverName);
        }

        [Test]
        public async Task GetInsuranceUser_NorthAmericanAgency()
        {
            //Arrange
            var agency = new Agency();
            var usa = new Country();
            var northAmericanRegion = new Region();
            var europeanSmo = new Smo();
            var countryId = Guid.NewGuid();
            var northAmericanInsuranceUserId = Guid.NewGuid();
            var europeanInsuranceUserId = Guid.NewGuid();
            var northAmericanRegionId = Guid.NewGuid();
            var europeanSmoId = Guid.NewGuid();
            var northAmericanGdamUserId = "ABC";
            var europeGdamUserId = "DEF";

            var northAmericanInsuranceUser = new CostUser
            {
                Id = northAmericanInsuranceUserId,
                GdamUserId = northAmericanGdamUserId,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        },
                        ObjectType = core.Constants.AccessObjectType.Region
                    }
                }
            };

            var europeanInsuranceUser = new CostUser
            {
                Id = europeanInsuranceUserId,
                GdamUserId = europeGdamUserId,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        },
                        ObjectType = core.Constants.AccessObjectType.Smo,
                    }
                }
            };

            agency.CountryId = countryId;
            agency.Country = usa;
            usa.Id = countryId;
            usa.GeoRegionId = northAmericanRegionId;

            var northAmericanRegionModel = new RegionModel
            {
                Name = Constants.AgencyRegion.NorthAmerica
            };
            _regionsService.Setup(r => r.GetAsync(It.IsAny<Guid>())).ReturnsAsync(northAmericanRegionModel);


            northAmericanRegion.Id = northAmericanRegionId;
            northAmericanRegion.Key = Constants.Region.NorthAmericanArea;
            europeanSmo.Id = europeanSmoId;
            europeanSmo.Key = Constants.Smo.WesternEurope;

            var costUsers = new List<CostUser> { northAmericanInsuranceUser, europeanInsuranceUser };

            //needed otherwise tests fail when run all together!
            var existingUsers = _efContext.CostUser.ToList();
            _efContext.CostUser.RemoveRange(existingUsers);

            _efContext.AddRange(costUsers);
            _efContext.Smo.Add(europeanSmo);
            _efContext.Region.Add(northAmericanRegion);
            _efContext.SaveChanges();
            _regionsService.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.NorthAmerica,
                Name = Constants.AgencyRegion.NorthAmerica
            });

            //Act
            var result = await _costUserService.GetInsuranceUsers(agency);

            //Assert
            result.Should().NotBeNull();
            result.First().Should().Be(northAmericanGdamUserId);
        }

        [Test]
        public async Task GetInsuranceUser_WesternEuropeAgency()
        {
            //Arrange
            var agency = new Agency();
            var england = new Country();
            var northAmericanRegion = new Region();
            var europeanSmo = new Smo();

            var countryId = Guid.NewGuid();
            var northAmericanInsuranceUserId = Guid.NewGuid();
            var europeanInsuranceUserId = Guid.NewGuid();
            var northAmericanRegionId = Guid.NewGuid();
            var europeanSmoId = Guid.NewGuid();
            var northAmericanGdamUserId = "ABC";
            var europeGdamUserId = "DEF";

            var northAmericanInsuranceUser = new CostUser
            {
                GdamUserId = northAmericanGdamUserId,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        },
                        ObjectType = core.Constants.AccessObjectType.Region,
                        ObjectId = northAmericanRegionId
                    }
                }
            };
            var europeanInsuranceUser = new CostUser
            {
                GdamUserId = europeGdamUserId,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.InsuranceUser,
                            Value = Constants.BusinessRole.InsuranceUser
                        },
                        ObjectType = core.Constants.AccessObjectType.Smo,
                        ObjectId = europeanSmoId
                    }
                }
            };

            agency.CountryId = countryId;
            agency.Country = england;
            england.Id = countryId;
            england.GeoRegionId = europeanSmoId;

            var europeanRegion = new RegionModel
            {
                Name = Constants.AgencyRegion.Europe
            };
            _regionsService.Setup(r => r.GetAsync(It.IsAny<Guid>())).Returns(Task.FromResult(europeanRegion));

            northAmericanInsuranceUser.Id = northAmericanInsuranceUserId;
            europeanInsuranceUser.Id = europeanInsuranceUserId;

            northAmericanRegion.Id = northAmericanRegionId;
            northAmericanRegion.Key = Constants.Region.NorthAmericanArea;
            europeanSmo.Id = europeanSmoId;
            europeanSmo.Key = Constants.Smo.WesternEurope;

            var costUsers = new List<CostUser> { northAmericanInsuranceUser, europeanInsuranceUser };
            //needed otherwise tests fail when run all together!
            var existingUsers = _efContext.CostUser.ToList();
            _efContext.CostUser.RemoveRange(existingUsers);

            _efContext.AddRange(costUsers);
            _efContext.Region.Add(northAmericanRegion);
            _efContext.Smo.Add(europeanSmo);
            _efContext.SaveChanges();

            _regionsService.Setup(a => a.GetGeoRegion(It.IsAny<Guid>())).ReturnsAsync(new RegionModel
            {
                Id = Guid.NewGuid(),
                Key = Constants.AgencyRegion.Europe,
                Name = Constants.AgencyRegion.Europe
            });
            //Act
            var result = await _costUserService.GetInsuranceUsers(agency);

            //Assert
            result.Should().NotBeNull();
            result.First().Should().Be(europeGdamUserId);
        }

        [Test]
        public async Task GetFinanceManagementUser()
        {
            //Arrange
            var costStageRevision = new CostStageRevision();
            var northAmericanRegion = new Region();
            var europeanSmo = new Smo();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;
            var role = new Role
            {
                Id = Guid.NewGuid(),
                BusinessRoles = new List<BusinessRole>
                {
                    new BusinessRole
                    {
                        Key = Constants.BusinessRole.FinanceManager,
                        Value = Constants.BusinessRole.FinanceManager,

                    }
                }
            };

            var costStageRevisionId = Guid.NewGuid();
            costStageRevision.Id = costStageRevisionId;

            var northAmericanFinanceUserId = Guid.NewGuid();
            var northAmericanFinanceUserGdamId = "na_finance_gdam_id";
            var europeanFinanceUserId = Guid.NewGuid();
            var northAmericanRegionId = Guid.NewGuid();
            var userGroupId = Guid.NewGuid();
            var europeanSmoId = Guid.NewGuid();
            var northAmericanFinanceUser = new CostUser
            {
                Id = northAmericanFinanceUserId,
                NotificationBudgetRegion = northAmericanRegion,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.FinanceManager,
                            Value = Constants.BusinessRole.FinanceManager,
                            Role = role
                        },
                        ObjectType = core.Constants.AccessObjectType.Client
                    }
                },
                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserGroup = new UserGroup
                        {
                            Id = userGroupId,
                            Role = role
                        },
                        UserGroupId = userGroupId,
                        UserId = northAmericanFinanceUserId
                    }
                },
                GdamUserId = northAmericanFinanceUserGdamId
            };
            var europeanFinanceUser = new CostUser
            {
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.FinanceManager,
                            Value = Constants.BusinessRole.FinanceManager
                        },
                        ObjectId = europeanSmoId,
                        ObjectType = core.Constants.AccessObjectType.Smo
                    }
                },
                Id = europeanFinanceUserId
            };

            northAmericanRegion.Id = northAmericanRegionId;
            northAmericanRegion.Key = Constants.Region.NorthAmericanArea;
            europeanSmo.Id = europeanSmoId;
            europeanSmo.Key = Constants.Smo.WesternEurope;

            var costUsers = new List<CostUser> { northAmericanFinanceUser, europeanFinanceUser };

            _efContext.Region.Add(northAmericanRegion);
            _efContext.Smo.Add(europeanSmo);

            _efContext.AddRange(costUsers);
            _efContext.SaveChanges();

            var costUserGroups = new[] { userGroupId.ToString() };

            //Act
            var result = await _costUserService.GetFinanceManagementUsers(costUserGroups, budgetRegion);

            //Assert
            result.Should().NotBeNull();

            var resultArr = result as string[] ?? result.ToArray();
            resultArr.Should().HaveCount(1);
            resultArr.First().Should().Be(northAmericanFinanceUserGdamId);
        }

        [Test]
        public async Task GetFinanceManagementUser_NA_Budget_Region()
        {
            //Arrange
            var costStageRevision = new CostStageRevision();
            var northAmericanRegion = new Region();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;
            var role = new Role
            {
                Id = Guid.NewGuid(),
                BusinessRoles = new List<BusinessRole>
                {
                    new BusinessRole
                    {
                        Key = Constants.BusinessRole.FinanceManager,
                        Value = Constants.BusinessRole.FinanceManager,

                    }
                }
            };

            var costStageRevisionId = Guid.NewGuid();
            costStageRevision.Id = costStageRevisionId;

            var northAmericanFinanceUserId = Guid.NewGuid();
            var northAmericanFinanceUserGdamId = "na_finance_gdam_id";
            var northAmericanRegionId = Guid.NewGuid();
            var userGroupId = Guid.NewGuid();
            var northAmericanFinanceUser = new CostUser
            {
                Id = northAmericanFinanceUserId,
                NotificationBudgetRegion = northAmericanRegion,
                UserBusinessRoles = new List<UserBusinessRole>
                {
                    new UserBusinessRole
                    {
                        BusinessRole = new BusinessRole
                        {
                            Key = Constants.BusinessRole.FinanceManager,
                            Value = Constants.BusinessRole.FinanceManager,
                            Role = role
                        },
                        ObjectType = core.Constants.AccessObjectType.Client
                    }
                },
                UserUserGroups = new List<UserUserGroup>
                {
                    new UserUserGroup
                    {
                        UserGroup = new UserGroup
                        {
                            Id = userGroupId,
                            Role = role
                        },
                        UserGroupId = userGroupId,
                        UserId = northAmericanFinanceUserId
                    }
                },
                GdamUserId = northAmericanFinanceUserGdamId
            };

            northAmericanRegion.Id = northAmericanRegionId;
            northAmericanRegion.Key = Constants.Region.NorthAmericanArea;

            var costUsers = new List<CostUser> { northAmericanFinanceUser };

            _efContext.Region.Add(northAmericanRegion);

            _efContext.AddRange(costUsers);
            _efContext.SaveChanges();

            var costUserGroups = new[] { userGroupId.ToString() };

            //Act
            var result = await _costUserService.GetFinanceManagementUsers(costUserGroups, budgetRegion);

            //Assert
            result.Should().NotBeNull();

            var resultArr = result as string[] ?? result.ToArray();
            resultArr.Should().HaveCount(1);
            resultArr.First().Should().Be(northAmericanFinanceUserGdamId);
        }

        [Test]
        public async Task Get_Many_FinanceManagementUser_NA_Budget_Region()
        {
            //Arrange
            var expectedCount = 15;
            var costStageRevision = new CostStageRevision();
            var region = new Region();
            var budgetRegion = Constants.BudgetRegion.NorthAmerica;
            var northAmericanRegionId = Guid.NewGuid();
            var role = new Role
            {
                Id = Guid.NewGuid(),
                BusinessRoles = new List<BusinessRole>
                {
                    new BusinessRole
                    {
                        Key = Constants.BusinessRole.FinanceManager,
                        Value = Constants.BusinessRole.FinanceManager,

                    }
                }
            };
            var businessRole = new BusinessRole
            {
                Key = Constants.BusinessRole.FinanceManager,
                Value = Constants.BusinessRole.FinanceManager,
                Role = role
            };

            var costStageRevisionId = Guid.NewGuid();
            costStageRevision.Id = costStageRevisionId;

            var costUsers = new List<CostUser>();
            var userGroupId = Guid.NewGuid();

            for (int i = 0; i < expectedCount; i++)
            {
                var userId = Guid.NewGuid();
                var gdamUserId = "na_finance_gdam_id" + expectedCount;
                
                var financeUser = new CostUser
                {
                    Id = userId,
                    NotificationBudgetRegion = region,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = businessRole,
                            ObjectType = core.Constants.AccessObjectType.Client
                        }
                    },
                    UserUserGroups = new List<UserUserGroup>
                    {
                        new UserUserGroup
                        {
                            UserGroup = new UserGroup
                            {
                                Id = userGroupId,
                                Role = role
                            },
                            UserGroupId = userGroupId,
                            UserId = userId
                        }
                    },
                    GdamUserId = gdamUserId
                };

                costUsers.Add(financeUser);
            }

            region.Id = northAmericanRegionId;
            region.Key = Constants.Region.NorthAmericanArea;
            _efContext.Region.Add(region);

            _efContext.AddRange(costUsers);
            _efContext.SaveChanges();

            var costUserGroups = new[] { userGroupId.ToString() };

            //Act
            var result = await _costUserService.GetFinanceManagementUsers(costUserGroups, budgetRegion);

            //Assert
            result.Should().NotBeNull();

            var resultArr = result as string[] ?? result.ToArray();
            resultArr.Should().HaveCount(expectedCount);
        }
    }
}
