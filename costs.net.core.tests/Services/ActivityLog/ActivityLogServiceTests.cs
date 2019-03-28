
namespace costs.net.core.tests.Services.ActivityLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Builders.ActivityLog;
    using dataAccess;
    using FluentAssertions;
    using core.Models.ActivityLog;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services.ActivityLog;
    using core.Services.Notifications;
    using dataAccess.Entity;
    using Microsoft.Extensions.Options;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ActivityLogServiceTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();
        private readonly Mock<ISupportNotificationService> _supportNotificationServiceMock = new Mock<ISupportNotificationService>();
        private readonly Mock<IOptions<AppSettings>> _appSettingsMock = new Mock<IOptions<AppSettings>>();
        private readonly List<ActivityLogMessageTemplate> _templates = new List<ActivityLogMessageTemplate>();
        private IActivityLogMessageBuilder _builder;
        private ActivityLogService _target;
        private const int MaxRetry = 5;

        private const string IpAddress = "127.0.0.1";

        [SetUp]
        public void Setup()
        {
            _builder = new DotLiquidMessageBuilder(_efContextMock.Object);
            _appSettingsMock.Setup(a => a.Value).Returns(new AppSettings { ActivityLogMaxRetry = MaxRetry, SupportEmailAddress = "adcostssupport@adstream.com" });
            _target = new ActivityLogService(_efContextMock.Object, _builder, _supportNotificationServiceMock.Object, _appSettingsMock.Object);

            var activityLogs = new List<ActivityLog>();
            var deliveries = new List<ActivityLogDelivery>();
            var activityLogDbSet = _efContextMock.MockAsyncQueryable(activityLogs.AsQueryable(), d => d.ActivityLog);
            var deliverDbSet = _efContextMock.MockAsyncQueryable(deliveries.AsQueryable(), d => d.ActivityLogDelivery);

            activityLogDbSet.Setup(d => d.Add(It.IsAny<ActivityLog>())).Callback((ActivityLog a) =>
           {
               activityLogs.Add(a);
           });

            deliverDbSet.Setup(d => d.Add(It.IsAny<ActivityLogDelivery>())).Callback((ActivityLogDelivery d) =>
            {
                deliveries.Add(d);
            });

            _efContextMock.MockAsyncQueryable(_templates.AsQueryable(), d => d.ActivityLogMessageTemplate);
        }

        [Test]
        public async Task Null_CostCreated_Entry_DoesNothing()
        {
            //Arrange
            CostCreated entry = null;
            var expected = 0;

            //Act
            await _target.Log(entry);

            //Assert
            var activityLog = _efContextMock.Object.ActivityLog;
            activityLog.Should().HaveCount(expected);
        }

        [Test]
        public async Task CostCreated_Entry_AddsToDb()
        {
            //Arrange
            var costNumber = "TestCost101";
            var userIdentity = new UserIdentity
            {
                Id = Guid.NewGuid(),
                IpAddress = IpAddress
            };
            var entry = new CostCreated(costNumber, userIdentity);
            var expected = 1;

            //Act
            await _target.Log(entry);

            //Assert
            var activityLog = _efContextMock.Object.ActivityLog;
            activityLog.Should().HaveCount(expected);
        }

        [Test]
        public void Null_Entry_BuildLogMessage_ThrowsError()
        {
            //Arrange
            ActivityLog entry = null;
            try
            {
                //Act
                _target.BuildLogMessage(entry);

            }
            catch (Exception)
            {
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void Valid_Entry_BuildLogMessage_ReturnsJson()
        {
            //Arrange
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            var delivery = new ActivityLogDelivery
            {
                RetryCount = 0,
                Status = ActivityLogDeliveryStatus.New
            };
            var entry = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser,
                ActivityLogDelivery = delivery
            };
            _templates.Add(new ActivityLogMessageTemplate
            {
                ActivityLogType = ActivityLogType.CostCreated,
                Id = 1,
                Template = "{\"timestamp\":\"{{ timestamp | date: \"yyyy-MM-ddTHH:mm:ss.fffZ\" }}\",\"id\":\"{ { messageId } }\",\"type\":\"costCreated\",\"object\":{\"id\":\"{{ objectId }}\",\"type\":\"activity\", \"message\":\"cost ''{{ costId | escape }}'' has been created\", \"costId\":\"{{ costId || escape }}\"},\"subject\":{\"id\":\"{{ subjectId }}\",\"application\":\"adcosts\"}}"
            });

            //Act
            var result = _target.BuildLogMessage(entry);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().NotBeNull();
            var resultJson = JsonConvert.DeserializeObject(result.Message);
            resultJson.Should().NotBeNull(); ;
        }

        [Test]
        public async Task Entry_Successfully_Sent()
        {
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            var delivery = new ActivityLogDelivery
            {
                RetryCount = 0,
                Status = ActivityLogDeliveryStatus.New
            };
            var entry = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser,
                ActivityLogDelivery = delivery
            };
            var expected = ActivityLogDeliveryStatus.Sent;

            await _target.EntryDeliveredSuccessfully(entry);

            entry.ActivityLogDelivery.Status.Should().Be(expected);
        }

        [Test]
        public async Task Entry_FailedToDeliver_MaxRetryNotReached()
        {
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            var delivery = new ActivityLogDelivery
            {
                RetryCount = 2,
                Status = ActivityLogDeliveryStatus.New
            };
            var entry = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser,
                ActivityLogDelivery = delivery
            };
            var message = new ActivityLogMessage();
            var expected = ActivityLogDeliveryStatus.Failed;

            await _target.EntryDeliveryFailed(entry, message);

            entry.ActivityLogDelivery.Status.Should().Be(expected);
        }

        [Test]
        public async Task Entry_FailedToDeliver_MaxRetryReached()
        {
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            var delivery = new ActivityLogDelivery
            {
                RetryCount = MaxRetry,
                Status = ActivityLogDeliveryStatus.New
            };
            var entry = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser,
                ActivityLogDelivery = delivery
            };
            var message = new ActivityLogMessage();
            var expected = ActivityLogDeliveryStatus.MaxRetriesReached;

            await _target.EntryDeliveryFailed(entry, message);

            entry.ActivityLogDelivery.Status.Should().Be(expected);
        }
    }
}
