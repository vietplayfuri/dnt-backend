namespace costs.net.messaging.test.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Builders;
    using core.Builders.Response;
    using core.ExternalResource.Gdam;
    using core.Models;
    using core.Models.AMQ;
    using core.Models.Utils;
    using core.Services;
    using core.Services.ActivityLog;
    using core.Services.Agency;
    using core.Services.Events;
    using core.Services.Search;
    using core.Services.User;
    using dataAccess;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using FluentAssertions;
    using messaging.Handlers;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using plugins;
    using Serilog;
    using tests.common.Stubs.EFContext;

    //[TestFixture]
    public class A5UserLoginTests
    {
        [SetUp]
        public void Init()
        {
            _loggerMock = new Mock<ILogger>();
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();
            _efContext = EFContextFactory.CreateInMemoryEFContext();
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _permissionServiceMock = new Mock<IPermissionService>();
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _eventServiceMock = new Mock<IEventService>();
            _pgUserServiceMock = new Mock<IPgUserService>();
            _elasticSearchServiceMock = new Mock<IElasticSearchService>();
            _gdamClientMock = new Mock<IGdamClient>();
            _agencyServiceMock = new Mock<IAgencyService>();
            _userService = new UserService(
                _mapperMock.Object,
                _permissionServiceMock.Object,
                _loggerMock.Object,
                _appSettingsMock.Object,
                _efContext,
                _elasticSearchServiceMock.Object,
                new[]
                {
                    new Lazy<IPgUserService, PluginMetadata>(
                        () => _pgUserServiceMock.Object, new PluginMetadata { BuType = BuType.Pg })
                },
                _activityLogServiceMock.Object,
                _agencyServiceMock.Object,
                _gdamClientMock.Object);
            _handlerMock = new A5UserLoginHandler(_loggerMock.Object, _userServiceMock.Object);
            _handler = new A5UserLoginHandler(_loggerMock.Object, _userService);
        }

        private Mock<ILogger> _loggerMock;
        private EFContext _efContext;
        private Mock<IUserService> _userServiceMock;
        private Mock<IElasticSearchService> _elasticSearchServiceMock;
        private Mock<IPgUserService> _pgUserServiceMock;
        private Mock<IGdamClient> _gdamClientMock;
        private Mock<IAgencyService> _agencyServiceMock;
        private Mock<IEventService> _eventServiceMock;

        private Mock<IMapper> _mapperMock;
        private Mock<IPermissionService> _permissionServiceMock;
        private Mock<IOptions<AppSettings>> _appSettingsMock;
        private Mock<IActivityLogService> _activityLogServiceMock;
        private A5UserLoginHandler _handlerMock;
        private A5UserLoginHandler _handler;
        private UserService _userService;

        private static async Task<string> ReadAllLinesAsync(string filepath)
        {
            using (var reader = File.OpenText(filepath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task<string> GetJsonString(string fileName)
        {
            var basePath = AppContext.BaseDirectory;

            var jsonPath = $"{basePath}{Path.DirectorySeparatorChar}JsonData{Path.DirectorySeparatorChar}{fileName}.json";
            var stringJson = await ReadAllLinesAsync(jsonPath);
            return stringJson;
        }

        private void InitData(string fileName, string approvalBandString, decimal approvalLimit, out List<CostUser> users, out ApprovalBand approvalBand, out Smo smo, out UserLoginEvent userLoginEventObject)
        {
            var stringJson = GetJsonString(fileName).Result;
            userLoginEventObject = JsonConvert.DeserializeObject<UserLoginEvent>(stringJson);
            users = new List<CostUser>
            {
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = Constants.BusinessRole.BrandManager
                            }
                        }
                    },
                    GdamUserId = "58adcba90c885409f1f47c57"
                },
                new CostUser
                {
                    Id = Guid.NewGuid(),
                    Email = ApprovalMemberModel.BrandApprovalUserEmail,
                    UserBusinessRoles = new List<UserBusinessRole>
                    {
                        new UserBusinessRole
                        {
                            BusinessRole = new BusinessRole
                            {
                                Key = Constants.BusinessRole.BrandManager,
                                Value = Constants.BusinessRole.BrandManager
                            }
                        }
                    },
                    GdamUserId = "4ef31ce1766ec96769b399c0"
                }
            };

            approvalBand = new ApprovalBand
            {
                ApprovalLimit = approvalLimit,
                BusinessRole = users.First(a => a.GdamUserId == "58adcba90c885409f1f47c57").UserBusinessRoles.First().BusinessRole,
                Id = Guid.NewGuid(),
                Band = -1,
                StringBand = approvalBandString
            };
            smo = new Smo
            {
                Countries = new List<Country>
                {
                    new Country
                    {
                        Cities = new List<City>(),
                        Iso = userLoginEventObject.@object.sessionData.CountryID,
                        Name = "POLAND"
                    }
                },
                Key = Constants.Smo.WesternEurope,
                Value = Constants.Smo.WesternEurope
            };
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_1()
        {
            var stringJson = await GetJsonString("userLoginEvent_1");
            var userLoginEventObject = JsonConvert.DeserializeObject<UserLoginEvent>(stringJson);

            await _handlerMock.Handle(userLoginEventObject);
            _userServiceMock.Verify(a => a.UpdateSessionData(It.IsAny<Sessiondata>(), It.IsAny<string>()), Times.Once);
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_2()
        {
            var stringJson = await GetJsonString("userLoginEvent_1");
            var userLoginEventObject = JsonConvert.DeserializeObject<UserLoginEvent>(stringJson);

            await _handlerMock.Handle(userLoginEventObject);
            _userServiceMock.Verify(a => a.UpdateSessionData(It.IsAny<Sessiondata>(), It.IsAny<string>()), Times.Once);
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_UpdateForBand04()
        {
            // setup
            const string fileName = "userLoginEvent_2";
            const string approvalBandString = "04";
            const decimal approvalLimit = 50000m;
            InitData(fileName, approvalBandString, approvalLimit, out var users, out var approvalBand, out var smo, out var userLoginEventObject);
            _userServiceMock.Setup(a => a.UpdateSessionData(It.IsAny<Sessiondata>(), It.IsAny<string>()));
            _efContext.CostUser.AddRange(users);
            _efContext.ApprovalBand.Add(approvalBand);
            _efContext.SaveChanges();

            // Act
            await _handler.Handle(userLoginEventObject);

            // Assert
            var assertUser = _efContext.CostUser.FirstOrDefault(a => a.Id == users.First(b => b.GdamUserId == "58adcba90c885409f1f47c57").Id);
            _efContext.ReloadEntity(assertUser);

            assertUser.ApprovalLimit.Should().Be(approvalBand.ApprovalLimit);
            assertUser.Band.Should().Be(approvalBand.StringBand);
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_ThrowErrorWhenCountryCantBeFound()
        {
            // setup
            const string fileName = "userLoginEvent_2";
            const string approvalBandString = "A&T";
            const decimal approvalLimit = 10000m;
            InitData(fileName, approvalBandString, approvalLimit, out var users, out var approvalBand, out var smo, out var userLoginEventObject);
            userLoginEventObject.@object.sessionData.Band = approvalBandString;
            userLoginEventObject.@object.sessionData.OrgType = "MDO";
            _efContext.CostUser.AddRange(users);
            _efContext.ApprovalBand.Add(approvalBand);
            _efContext.SaveChanges();
            try
            {
                // Act
                await _handler.Handle(userLoginEventObject);
            }
            catch (Exception e) when (e is InvalidOperationException)
            {
                // Assert
                var sessionData = userLoginEventObject.@object.sessionData;
                e.Should().BeOfType<InvalidOperationException>();
                e.Message.Should()
                    .StartWith(
                    $"Smo cannot be found for user: {users.First(b => b.GdamUserId == "58adcba90c885409f1f47c57").Id}, country code supplied: {sessionData.CountryID}");
            }
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_CorrectOrgTypeMapping()
        {
            // setup
            const string fileName = "userLoginEvent_2";
            const string approvalBandString = "A&T";
            const decimal approvalLimit = 10000m;
            InitData(fileName, approvalBandString, approvalLimit, out var users, out var approvalBand, out var smo, out var userLoginEventObject);
            userLoginEventObject.@object.sessionData.Band = approvalBandString;
            userLoginEventObject.@object.sessionData.OrgType = "MDO";
            _userServiceMock.Setup(a => a.UpdateSessionData(It.IsAny<Sessiondata>(), It.IsAny<string>()));
            _efContext.Smo.Add(smo);
            _efContext.CostUser.AddRange(users);
            _efContext.ApprovalBand.Add(approvalBand);
            _efContext.SaveChanges();

            // Act
            await _handler.Handle(userLoginEventObject);

            // Assert
            var assertUser = _efContext.CostUser.FirstOrDefault(a => a.Id == users.First(b => b.GdamUserId == "58adcba90c885409f1f47c57").Id);
            _efContext.ReloadEntity(assertUser);
            var sessionData = userLoginEventObject.@object.sessionData;

            assertUser.ApprovalLimit.Should().Be(approvalBand.ApprovalLimit);
            assertUser.Band.Should().Be(approvalBand.StringBand);
            assertUser.OrgType.Should().Be(sessionData.OrgType);
            assertUser.UserBusinessRoles.Count.Should().Be(2);
            _pgUserServiceMock.Verify(a => a.RemoveUserFromExistingCosts(It.IsAny<List<UserBusinessRole>>(), It.IsAny<CostUser>()), Times.Once);
            _pgUserServiceMock.Verify(a => a.AddUserToExistingCosts(It.IsAny<CostUser>(), It.IsAny<UserBusinessRole>()), Times.Once);
        }

        //[Test]
        public async Task HandleUserLoginEventObject_SSO_UPDATE_WrongOrgTypeMapping()
        {
            // setup
            const string fileName = "userLoginEvent_2";
            const string approvalBandString = "A&T";
            const decimal approvalLimit = 10000m;
            InitData(fileName, approvalBandString, approvalLimit, out var users, out var approvalBand, out var smo, out var userLoginEventObject);
            userLoginEventObject.@object.sessionData.Band = approvalBandString;
            userLoginEventObject.@object.sessionData.OrgType = "MSO";
            _userServiceMock.Setup(a => a.UpdateSessionData(It.IsAny<Sessiondata>(), It.IsAny<string>()));
            _efContext.Smo.Add(smo);
            _efContext.CostUser.AddRange(users);
            _efContext.ApprovalBand.Add(approvalBand);
            _efContext.SaveChanges();

            // Act
            await _handler.Handle(userLoginEventObject);

            // Assert
            var assertUser = _efContext.CostUser.FirstOrDefault(a => a.Id == users.First(b => b.GdamUserId == "58adcba90c885409f1f47c57").Id);
            _efContext.ReloadEntity(assertUser);
            var sessionData = userLoginEventObject.@object.sessionData;

            assertUser.ApprovalLimit.Should().Be(approvalBand.ApprovalLimit);
            assertUser.Band.Should().Be(approvalBand.StringBand);
            assertUser.OrgType.Should().Be(sessionData.OrgType);
            assertUser.UserBusinessRoles.Count.Should().Be(1);
            _pgUserServiceMock.Verify(a => a.RemoveUserFromExistingCosts(It.IsAny<List<UserBusinessRole>>(), It.IsAny<CostUser>()), Times.Never);
            _pgUserServiceMock.Verify(a => a.AddUserToExistingCosts(It.IsAny<CostUser>(), It.IsAny<UserBusinessRole>()), Times.Never);
        }
    }
}
