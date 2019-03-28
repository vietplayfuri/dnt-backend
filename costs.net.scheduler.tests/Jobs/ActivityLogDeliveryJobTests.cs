namespace costs.net.scheduler.tests.Jobs
{
    using System;
    using System.Collections.Generic;
    using core.Jobs;
    using dataAccess.Entity;
    using Moq;
    using net.core;
    using net.core.ExternalResource.Paperpusher;
    using net.core.Models.ActivityLog;
    using net.core.Services.ActivityLog;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class ActivityLogDeliveryJobTests
    {
        private readonly Mock<IActivityLogService> _serviceMock = new Mock<IActivityLogService>();
        private readonly Mock<IPaperpusherClient> _ppServiceMock = new Mock<IPaperpusherClient>();

        private ActivityLogDeliveryJob _target;

        [SetUp]
        public void Init()
        {
            _target = new ActivityLogDeliveryJob(_serviceMock.Object, _ppServiceMock.Object);
        }

        [Test]
        public void ExecuteTest_NoEntries()
        {
            
            //Act
            _target.Execute();

            //Assert
            _serviceMock.Verify(e => e.UpdateEntriesToProcessing(), Times.Once);
            _serviceMock.Verify(e => e.GetProcessingActivityLogs(), Times.Once);
            _ppServiceMock.Verify(e => e.SendMessage(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void ExecuteTest_OneEntryIsSent()
        {
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var message = "{}";
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            var delivery = new ActivityLogDelivery
            {
                RetryCount = 0,
                Status = ActivityLogDeliveryStatus.Processing
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
            bool first = true;
            _serviceMock.Setup(x => x.GetProcessingActivityLogs()).ReturnsAsync(() =>
            {
                if (first)
                {
                    first = false;
                    return new[]{entry}; //Return entry for the first loop and null for the second loop (in the Job class).
                }
                return null;
            });
            _serviceMock.Setup(x => x.BuildLogMessage(It.IsAny<ActivityLog>())).Returns(new ActivityLogMessage
            {
                Message = message
            });
            _ppServiceMock.Setup(x => x.SendMessage(It.IsAny<object>())).ReturnsAsync(true);

            //Act
            _target.Execute();

            //Assert
            _serviceMock.Verify(e => e.GetProcessingActivityLogs(), Times.AtLeastOnce);
            _ppServiceMock.Verify(e => e.SendMessage(It.IsAny<object>()), Times.Once);
        }
    }
}
