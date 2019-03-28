namespace costs.net.core.tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Ads.Net.Acl;
    using Ads.Net.Acl.Model;
    using Builders;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ACL;
    using core.Services.User;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Serilog;

    [TestFixture]
    public class PermissionServiceTests
    {
        private Mock<IAclClient> _aclClientMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private EFContext EFContext;
        private Mock<IAclUtilService> _aclUtilServiceMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IEnumerable<Lazy<IPgUserService, PluginMetadata>>> _pluginUserServicesMock;
        private PermissionService _sut;
        private Guid xUserId = Guid.NewGuid();

        [SetUp]
        public void Init()
        {
            _aclClientMock = new Mock<IAclClient>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            EFContext = EFContextFactory.CreateInMemoryEFContext();
            _aclUtilServiceMock = new Mock<IAclUtilService>();
            _loggerMock = new Mock<ILogger>();
            _pluginUserServicesMock = new Mock<IEnumerable<Lazy<IPgUserService, PluginMetadata>>>();
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings { CostsAdminUserId = xUserId.ToString() });
            _sut = new PermissionService(
                _aclClientMock.Object,
                _appSettingsMock.Object,
                EFContext,
                _aclUtilServiceMock.Object,
                _loggerMock.Object,
                _pluginUserServicesMock.Object
                );
        }

        [Test]
        public async Task GetPermissionByName_whenExists_shouldReturnPermissionModel()
        {
            // Arrange
            var permisisonName = "cost.read";
            var aclAction = "read";
            var addedPermission = new dataAccess.Entity.Permission
            {
                Name = permisisonName,
                Action = aclAction
            };
            await EFContext.Permission.AddAsync(addedPermission);
            await EFContext.SaveChangesAsync();

            // Act
            var permission = await _sut.GetPermissionByName(permisisonName);

            // Assert
            permission.Name.Should().Be(permisisonName);
            permission.Action.Should().Be(aclAction);
        }


        [Test]
        public async Task GrantAccess_Create_UserGroup_Entity_In_Memory_Pass()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUsers = new List<CostUser>() {
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                },
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                }
            };
            var cost = new Cost
            {
                Id = costId,
            };
            EFContext.Cost.Add(cost);
            EFContext.CostUser.Add(new CostUser { Email = "costs.admin@adstream.com" });
            await EFContext.SaveChangesAsync();
            var saveChanges = false;
            _aclClientMock.Setup(acl => acl.Access.GrantAccessToObject(xUserId.ToString(), costId.ToString(), It.IsAny<string>(), roleId.ToString(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>()
                {
                    Status = HttpStatusCode.Created,
                    Response = new List<ResponseUserGroup> { new ResponseUserGroup { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Link.LinkUserToParent(xUserId.ToString(), It.IsAny<string>(), It.IsAny<Ads.Net.Acl.Model.User>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseDomainNode>>
                {
                    Response = new List<ResponseDomainNode> { new ResponseDomainNode { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Get.GetObjectUserGroups(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseRole>>
                {
                    Response = new List<ResponseRole> { new ResponseRole { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Get.GetUserGroupsForUser(It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>
                {
                    Response = new List<ResponseUserGroup> { new ResponseUserGroup { ExternalId = Guid.NewGuid().ToString() } }
                });


            // Act
            foreach (var costUser in costUsers)
            {
                var grantAccessResponse = await _sut.GrantAccess<Cost>(roleId, costId, costUser, null, saveChanges);
            }


            // Assert
            var userGroups = EFContext.UserGroup.Local.Where(ug => ug.ObjectId == costId && ug.RoleId == roleId);

            /* Make sure there is only 1 item, this is root cause of ADC-2691, it added more than 1 item so get duplicate unique key error */
            userGroups.Count().Should().Be(1); 

            foreach (var item in userGroups)
            {
                /* Make sure all declared entities are added state, not modified, if they are modifed, meaning they can't be added to database
                 * If they can't add to database, user_user_group table doesn't have foreign key
                 *
                 * If we do not have saveChange parameter, it could be an issue for this case, with first loop, the state is Added, but in the second loop, because the entity
                 * has already existed, so state of Entity will be change into Modified, then false
                 */
                EFContext.Entry(item).State.Should().Be(Microsoft.EntityFrameworkCore.EntityState.Added); 
            }
        }

        [Test]
        public async Task GrantAccess_Try_To_Create_Duplicated_UserGroup_Entity_Pass()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUsers = new List<CostUser>() {
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                },
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                }
            };
            var cost = new Cost
            {
                Id = costId,
            };
            var userGroup = new dataAccess.Entity.UserGroup() {
                ObjectId = costId,
                RoleId = roleId
            };
            EFContext.UserGroup.Add(userGroup);
            EFContext.Cost.Add(cost);
            EFContext.CostUser.Add(new CostUser { Email = "costs.admin@adstream.com" });
            await EFContext.SaveChangesAsync();
            var saveChanges = false;
            _aclClientMock.Setup(acl => acl.Access.GrantAccessToObject(xUserId.ToString(), costId.ToString(), It.IsAny<string>(), roleId.ToString(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>()
                {
                    Status = HttpStatusCode.Created,
                    Response = new List<ResponseUserGroup> { new ResponseUserGroup { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Link.LinkUserToParent(xUserId.ToString(), It.IsAny<string>(), It.IsAny<Ads.Net.Acl.Model.User>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseDomainNode>>
                {
                    Response = new List<ResponseDomainNode> { new ResponseDomainNode { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Get.GetObjectUserGroups(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseRole>>
                {
                    Response = new List<ResponseRole> { new ResponseRole { ExternalId = Guid.NewGuid().ToString() } }
                });

            _aclClientMock.Setup(acl => acl.Get.GetUserGroupsForUser(It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>
                {
                    Response = new List<ResponseUserGroup> { new ResponseUserGroup { ExternalId = Guid.NewGuid().ToString() } }
                });

            // Act
            foreach (var costUser in costUsers)
            {
                var grantAccessResponse = await _sut.GrantAccess<Cost>(roleId, costId, costUser, null, saveChanges);
            }

            await EFContext.SaveChangesAsync();

            // Assert
            var userGroupsDatabase = EFContext.UserGroup.Where(ug => ug.ObjectId == costId && ug.RoleId == roleId);

            /* Make sure there is only 1 item, this is root cause of ADC-2700, it added more than 1 item so get duplicate unique key error */
            userGroupsDatabase.Count().Should().Be(1);
        }
    }
}
