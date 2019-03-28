namespace costs.net.plugins.tests.Services.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Ads.Net.Acl.Model;
    using core.Extensions;
    using core.Models;
    using core.Models.Response;
    using core.Models.User;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Form;
    using UserGroup = dataAccess.Entity.UserGroup;

    [TestFixture]
    public class AddAccessUserServiceTests : UserServiceTestsBase
    {
        [Test]
        public async Task Add_One_UserAccessPermission_ForRegion_ToUserWithBusinessRole_ForSameRegion_SingleCost_Pass()
        {
            var userBeingUpdated = _users.First(user => user.Id == _UserWithOneRegionRoleId);

            //Same Role is used here on purpose!
            //We are checking if the Business Role gains extra labels!
            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionTwo"
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.FinanceManager).Id,
                        LabelName = "RegionOne"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs.Take(1));
            EfContext.SaveChanges();

            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(It.IsAny<CostStageRevision>())).Returns(_costStageReviDetailsFormFirstCost);

            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock.Setup(a => a.RevokeAccessForSubjectWithRole(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null, It.IsAny<bool>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString() });
            var userBusinessRole = userBeingUpdated.UserBusinessRoles.First();
            _mapperMock.Setup(a => a.Map<AccessDetail>(It.IsAny<UserBusinessRole>())).Returns(new AccessDetail
            {
                BusinessRoleId = userBusinessRole.BusinessRoleId,
                ObjectId = userBusinessRole.ObjectId,
                ObjectType = userBusinessRole.ObjectType
            });
            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().Labels.Length.Should().Be(2);
            verifyUser.UserBusinessRoles.First().Labels.Any(a => a.Contains("RegionTwo")).Should().BeTrue();
            verifyUser.UserBusinessRoles.First().Labels.Any(a => a.Contains("RegionOne")).Should().BeTrue();

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(It.IsAny<Guid>(), It.IsAny<Guid>(), userBeingUpdated, BuType.Pg, null, It.IsAny<string>(), It.IsAny<bool>()),
                Times.Never);
        }

        [Test]
        public async Task Add_SingleUserAccessPermission_ToRegion_ForSingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        LabelName = "RegionOne"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var agencyOwnerBusinessRole = _businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner);
            var firstCost = _costs.First(a => a.CostNumber == "number1");

            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(It.IsAny<CostStageRevision>())).Returns(_costStageReviDetailsFormFirstCost);
            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(
                    agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null, _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Id);
            verifyUser.UserBusinessRoles.First().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Value);
            verifyUser.UserBusinessRoles.First().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Key);
            verifyUser.UserBusinessRoles.First().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Role.Id);

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.BudgetRegion.Name
                    , It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task Add_SingleUserAccessPermission_ToSmo_ForSingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var clientModule = new Module
            {
                Id = Guid.NewGuid(),
                ClientType = ClientType.Pg,
                Name = "Procter & Gamble",
                ClientTag = "costPG",
                Key = "P&G"
            };
            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    Id = Guid.NewGuid(),
                    ObjectId = clientModule.Id,
                    Module = clientModule,
                    Type = core.Constants.AccessObjectType.Module
                }
            };

            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Smo,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        LabelName = "SmoName1"
                    }
                }
            };

            EfContext.AbstractType.AddRange(abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var agencyOwnerBusinessRole = _businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner);
            var firstCost = _costs.First(a => a.CostNumber == "number1");
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(It.IsAny<Guid>())).ReturnsAsync(_costStageReviDetailsFormFirstCost);
            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);
            _aclClientMock.Setup(a => a.Access.CheckAccessToDomainNode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(HttpStatusCode.OK);
            _aclClientMock.Setup(a => a.Get.GetObjectUserGroups(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new AclResponseObject<List<ResponseRole>>
            {
                Status = HttpStatusCode.OK,
                Response = new List<ResponseRole>
                {
                    new ResponseRole
                    {
                        ExternalId = Guid.NewGuid().ToString(),
                        Name = Guid.NewGuid().ToString()
                    }
                }
            });

            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(
                    agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null, _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Id);
            verifyUser.UserBusinessRoles.First().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Value);
            verifyUser.UserBusinessRoles.First().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Key);
            verifyUser.UserBusinessRoles.First().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Role.Id);

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(
                    agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null, _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task Add_ThreeUserAccessPermission_TwoRegion_OneSmo_ForSingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        LabelName = "RegionOne"
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionOne"
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Smo,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "SmoName1"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            //_efContextMock.Setup(c => c.GetCostsByStageDetailsFieldValue(It.IsAny<string>(), It.IsAny<string[]>())).Returns(costSetMock.Object);
            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var agencyOwnerBusinessRole = _businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner);
            var firstCost = _costs.First(a => a.CostNumber == "number1");
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(_costs[0].LatestCostStageRevision)).Returns(_costStageReviDetailsFormFirstCost);
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(_costs[1].LatestCostStageRevision)).Returns(_costStageReviDetailsFormSecondCost);
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(_costs[2].LatestCostStageRevision)).Returns(_costStageReviDetailsFormThirdCost);
            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(
                    agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(2);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Id);
            verifyUser.UserBusinessRoles.First().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Value);
            verifyUser.UserBusinessRoles.First().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Key);
            verifyUser.UserBusinessRoles.First().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Role.Id);

            verifyUser.UserBusinessRoles.Last().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Id);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Value);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Key);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Role.Id);

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.BudgetRegion.Name, It.IsAny<bool>()),
                Times.Once);
            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(
                    agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null, _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task Add_Two_UserAccessPermission_TwoDifferentRegions_singleBusinessRoleWithLabels_SingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var firstCost = _costs.First(a => a.CostNumber == "number1");
            var secondCost = _costs.First(a => a.CostNumber == "number2");
            var thirdCost = _costs.First(a => a.CostNumber == "number3");
            //Same Role is used here on purpose!
            //We are checking if the Business Role gains extra labels!
            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionTwo"
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionOne"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(firstCost.LatestCostStageRevision))
                .Returns(_costStageReviDetailsFormFirstCost);
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(secondCost.LatestCostStageRevision))
                .Returns(_costStageReviDetailsFormSecondCost);
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(thirdCost.LatestCostStageRevision))
                .Returns(_costStageReviDetailsFormThirdCost);

            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(It.IsAny<Guid>(), It.IsAny<Guid>(), userBeingUpdated, BuType.Pg, null,
                    It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            _permissionServiceMock.Setup(a => a.RevokeAccessForSubjectWithRole(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), null, It.IsAny<bool>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString() });

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().Labels.Length.Should().Be(2);
            verifyUser.UserBusinessRoles.First().Labels.Any(a => a.Contains("RegionTwo")).Should().BeTrue();
            verifyUser.UserBusinessRoles.First().Labels.Any(a => a.Contains("RegionOne")).Should().BeTrue();

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(It.IsAny<Guid>(), It.IsIn(firstCost.Id, secondCost.Id, thirdCost.Id), userBeingUpdated, BuType.Pg, null,
                    It.IsIn(_costStageReviDetailsFormFirstCost.BudgetRegion.Name, _costStageReviDetailsFormSecondCost.BudgetRegion.Name,
                        _costStageReviDetailsFormThirdCost.BudgetRegion.Name), It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task Add_TwoUserAccessPermission_OneRegion_OneGlobal_ForSingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectId = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Module).Id,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        ObjectType = core.Constants.AccessObjectType.Client
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionOne"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var agencyOwnerBusinessRole = _businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner);
            var firstCost = _costs.First(a => a.CostNumber == "number1");
            var agencyAbstractType = _abstractTypes.First(a => a.Type == core.Constants.AccessObjectType.Agency);
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(It.IsAny<CostStageRevision>())).Returns(_costStageReviDetailsFormFirstCost);
            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            _permissionServiceMock.Setup(a =>
                    a.CreateDomainNode(typeof(AbstractType).Name.ToSnakeCase(), agencyAbstractType.Id, It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new[] { Guid.NewGuid().ToString() });

            _pgAgencyServiceMock.Setup(a => 
                a.GetOrCreatePseudoAgencies(It.IsAny<Agency[]>()))
                .ReturnsAsync(
                    _abstractTypes
                        .Where(at => at.Type == core.Constants.AccessObjectType.Agency)
                        .Select(a => new AbstractType())
                        .ToList()
                    );

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(3);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Id);
            verifyUser.UserBusinessRoles.First().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Value);
            verifyUser.UserBusinessRoles.First().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Key);
            verifyUser.UserBusinessRoles.First().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Role.Id);
            verifyUser.UserBusinessRoles.First().Labels.Length.Should().Be(0);

            verifyUser.UserBusinessRoles.Last().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Id);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Value);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Key);
            verifyUser.UserBusinessRoles.Last().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyFinanceManager).Role.Id);
            verifyUser.UserBusinessRoles.Last().Labels.Length.Should().Be(1);
            verifyUser.UserBusinessRoles.Last().Labels.First().Should().Be("RegionOne");

            //Only one cost exists so only needed to be Add_ed
            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.BudgetRegion.Name, It.IsAny<bool>()),
                Times.Once);
        }

        [Test]
        public async Task Add_TwoUserAccessPermission_ToRegion_ForSingleCost_Pass()
        {
            var grantAccessResponse = new GrantAccessResponse
            {
                UserGroup = new UserGroup(),
                New = true,
                UserGroups = new[] { Guid.NewGuid().ToString() }
            };

            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyOwner).Id,
                        LabelName = "RegionOne"
                    },
                    new AccessDetail
                    {
                        ObjectType = core.Constants.AccessObjectType.Region,
                        BusinessRoleId = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyFinanceManager).Id,
                        LabelName = "RegionOne"
                    }
                }
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var agencyOwnerBusinessRole = _businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner);
            var firstCost = _costs.First(a => a.CostNumber == "number1");
            _costStageRevisionServiceMock.Setup(a => a.GetStageDetails<PgStageDetailsForm>(It.IsAny<Guid>())).ReturnsAsync(_costStageReviDetailsFormFirstCost);
            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);
            _permissionServiceMock
                .Setup(a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.SmoName, It.IsAny<bool>()))
                .ReturnsAsync(grantAccessResponse);

            // Act
            var result = await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            result.Messages.First().Should().Be("User updated.");
            result.Success.Should().Be(true);

            var verifyUser = EfContext.CostUser.First(a => a.Id == userBeingUpdated.Id);

            verifyUser.UserBusinessRoles.Count.Should().Be(1);
            verifyUser.UserBusinessRoles.First().BusinessRole.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Id);
            verifyUser.UserBusinessRoles.First().BusinessRole.Value.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Value);
            verifyUser.UserBusinessRoles.First().BusinessRole.Key.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Key);
            verifyUser.UserBusinessRoles.First().BusinessRole.Role.Id.Should().Be(_businessRoles.First(br => br.Value == Constants.BusinessRole.AgencyOwner).Role.Id);

            _permissionServiceMock.Verify(
                a => a.GrantUserAccess<Cost>(agencyOwnerBusinessRole.RoleId, firstCost.Id, userBeingUpdated, BuType.Pg, null,
                    _costStageReviDetailsFormFirstCost.BudgetRegion.Name, It.IsAny<bool>()),
                Times.Once);

            //            _efContextMock.Verify(a => a.Update(It.IsAny<CostUser>()), Times.Once);
        }

        [Test]
        public async Task Add_When_BusinessRoleIsCostConsultant_Should_OnlyLinkUserToBusinsessRoleAndDoNotGrantPermissions()
        {
            // Arrange
            var selectedBusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.CostConsultant);
            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        BusinessRoleId = selectedBusinessRole.Id
                    }
                }
            };
            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.SaveChanges();

            _permissionServiceMock.Setup(a => a.CheckHasAccess(_xUserId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(true);

            // Act
            await _pgUserService.UpdateUser(User, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            userBeingUpdated.UserBusinessRoles.Count.Should().Be(1);
            userBeingUpdated.UserBusinessRoles.First().BusinessRole.Id.Should().Be(selectedBusinessRole.Id);
            userBeingUpdated.UserBusinessRoles.First().ObjectId.Should().BeNull();
        }

        [Test]
        public async Task Add_When_UserIsAgencyAdmin_Should_HaveAccessToAssignRolesToUserInTheSameAgencyAsTheyAre()
        {
            // Arrange
            var usersBusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyAdmin);
            var selectedBusinessRole = _businessRoles.First(a => a.Value == Constants.BusinessRole.AgencyAdmin);
            var userBeingUpdated = _users.First(user => user.Id == _userNoRoles);
            var userPerformingAction = _users.First(u => u.Id == UserWithAgencyAdminRoleId);
            var updateUserModel = new UpdateUserModel
            {
                AccessDetails = new List<AccessDetail>
                {
                    new AccessDetail
                    {
                        BusinessRoleId = selectedBusinessRole.Id
                    }
                }
            };
            userPerformingAction.Agency.Id = userBeingUpdated.Agency.Id;
            var userBusinessRole = new UserBusinessRole
            {
                CostUserId = userPerformingAction.Id,
                BusinessRoleId = usersBusinessRole.Id,
                CostUser = userPerformingAction
            };

            EfContext.AbstractType.AddRange(_abstractTypes);
            EfContext.Role.AddRange(_roles);
            EfContext.BusinessRole.AddRange(_businessRoles);
            EfContext.CostUser.AddRange(_users);
            EfContext.Cost.AddRange(_costs);
            EfContext.UserBusinessRole.Add(userBusinessRole);
            EfContext.SaveChanges();

            _permissionServiceMock.Setup(a => a.CheckHasAccess(UserWithAgencyAdminRoleId, userBeingUpdated.Id, AclActionType.Edit, "user")).ReturnsAsync(false);
            _pgAgencyServiceMock.Setup(a =>
                    a.GetOrCreatePseudoAgencies(It.IsAny<Agency[]>()))
                .ReturnsAsync(
                    _abstractTypes
                        .Where(at => at.Type == core.Constants.AccessObjectType.Agency)
                        .Select(a => new AbstractType())
                        .ToList()
                );

            // Act
            await _pgUserService.UpdateUser(new UserIdentity { Id = UserWithAgencyAdminRoleId }, userBeingUpdated.Id, updateUserModel, BuType.Pg);

            // Assert
            userBeingUpdated.UserBusinessRoles.Count.Should().BeGreaterThan(0);
            userBeingUpdated.UserBusinessRoles.First().BusinessRole.Id.Should().Be(selectedBusinessRole.Id);
        }
    }
}