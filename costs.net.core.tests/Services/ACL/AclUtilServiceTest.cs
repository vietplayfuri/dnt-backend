namespace costs.net.core.tests.Services.ACL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ads.Net.Acl;
    using Ads.Net.Acl.Api;
    using Ads.Net.Acl.Helper;
    using Ads.Net.Acl.Methods;
    using Ads.Net.Acl.Model;
    using AutoMapper;
    using Builders;
    using Builders.Response;
    using core.Models;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ACL;
    using core.Services.Events;
    using core.Services.User;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using NUnit.Framework;
    using Serilog;
    using UserGroup = dataAccess.Entity.UserGroup;

    [TestFixture]
    public class AclUtilServiceTest
    {
        [SetUp]
        public void Setup()
        {
            _aclClientMock = new Mock<IAclClient>();
            _eventServiceMock = new Mock<IEventService>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _aclSettingsMock = new Mock<IOptions<AclSettings>>();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _loggerMock = new Mock<ILogger>();
            _pluginUserServicesMock = new Mock<IEnumerable<Lazy<IPgUserService, PluginMetadata>>>();

            _aclClientMock.Setup(a => a.Access).Returns(new Access(new AccessApi(new RequestHelper("http://localhost:9999/")), new FunctionHelper()));
            _aclClientMock.Setup(a => a.Get).Returns(new Get(new GetApi(new RequestHelper("http://localhost:9999/")), new FunctionHelper()));
            _aclClientMock.Setup(a => a.Batch).Returns(new Batch(new BatchApi(new RequestHelper("http://localhost:9999/"))));
            _aclSettingsMock.Setup(a => a.Value).Returns(new AclSettings { AclDb = "http://localhost:8529", AclUrl = "http://localhost:7777" });
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings { CostsAdminUserId = "77681eb0-fc0d-44cf-83a0-36d51851e9ae" });
            _aclUtilService = new AclUtilService(
                _aclSettingsMock.Object,
                _aclClientMock.Object,
                _appSettingsMock.Object,
                _loggerMock.Object,
                _efContext,
                _eventServiceMock.Object,
                _mapperMock.Object
                );
            _permissionService = new PermissionService(
                _aclClientMock.Object,
                _appSettingsMock.Object,
                _efContext,
                _aclUtilService,
                _loggerMock.Object,
                _pluginUserServicesMock.Object
            );
        }

        private AclUtilService _aclUtilService;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IAclClient> _aclClientMock;
        private IPermissionService _permissionService;
        private EFContext _efContext;
        private Mock<IOptions<AclSettings>> _aclSettingsMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IEnumerable<Lazy<IPgUserService, PluginMetadata>>> _pluginUserServicesMock;
        private Mock<IMapper> _mapperMock = new Mock<IMapper>();

        [Test]
        public async Task RecalculateFromAgency()
        {
            //Arange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var agencyId = Guid.NewGuid();
            var newUserGroupId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var costUser = new CostUser { Id = userId, ParentId = agencyId };
            var adminUser = new CostUser { Id = Guid.NewGuid(), Email = ApprovalMemberModel.BrandApprovalUserEmail };
            var currentUserGroups = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            var abstractTypes = new List<AbstractType>
            {
                new AbstractType
                {
                    UserGroups = currentUserGroups,
                    Agency = new Agency (),
                    Type = AbstractObjectType.Module.ToString()
                },
                new AbstractType
                {
                    UserGroups = currentUserGroups,
                    Agency = new Agency(),
                    Type = AbstractObjectType.Agency.ToString()
                }
            };
            var project = new List<Project> { new Project { Id = projectId } };
            var cost = new List<Cost> { new Cost { ParentId = projectId, Id = Guid.NewGuid(), UserGroups = currentUserGroups } };

            _efContext.AddRange(new List<UserGroup>());
            _efContext.AddRange(abstractTypes);
            _efContext.Add(costUser);
            _efContext.Add(adminUser);
            _efContext.AddRange(project);
            _efContext.AddRange(cost);
            _efContext.SaveChanges();

            _aclClientMock.Setup(a => a.Get.GetUserGroupsForUser(costUser.Id.ToString()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>
                {
                    ErrorMessage = null,
                    Response =
                        new List<ResponseUserGroup>
                        {
                            new ResponseUserGroup { ExternalId = currentUserGroups.FirstOrDefault() },
                            new ResponseUserGroup { ExternalId = currentUserGroups.LastOrDefault() },
                            new ResponseUserGroup { ExternalId = newUserGroupId.ToString() }
                        },
                    Status = HttpStatusCode.OK
                });
            _aclClientMock.Setup(a => a.Get.GetObjectUserGroups(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseRole>>
                {
                    ErrorMessage = null,
                    Response = new List<ResponseRole>
                    {
                        new ResponseRole { ExternalId = currentUserGroups.FirstOrDefault() },
                        new ResponseRole { ExternalId = currentUserGroups.LastOrDefault() },
                        new ResponseRole { ExternalId = newUserGroupId.ToString() }
                    },
                    Status = HttpStatusCode.OK
                });

            _aclClientMock.Setup(
                    a => a.Access.GrantAccessToObject(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new AclResponseObject<List<ResponseUserGroup>>
                {
                    ErrorMessage = null,
                    Response = new List<ResponseUserGroup> { new ResponseUserGroup { ExternalId = newUserGroupId.ToString() } },
                    Status = HttpStatusCode.Created
                });

            var agencyAbstractType = _efContext.AbstractType.Include(a => a.Agency).FirstOrDefault(a => a.Type == AbstractObjectType.Agency.ToString());
            await _permissionService.GrantUserAccess<AbstractType>(roleId, agencyAbstractType.Id, costUser, BuType.Pg, userId);
        }
    }
}
