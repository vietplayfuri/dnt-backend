
namespace costs.net.plugins.tests.PG.Services.PostProcessing
{
    using System;
    using System.Threading.Tasks;
    using core.Models;
    using core.Models.ActivityLog;
    using core.Models.User;
    using core.Services.ActivityLog;
    using core.Services.PostProcessing;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Services.PostProcessing;

    [TestFixture]
    public class PgCustomDataObjectIoNumberAddedTests
    {
        private PgCustomDataObjectIoNumberAdded _target;
        private UserIdentity _user;
        private Mock<IActivityLogService> _activityLogServiceMock;

        [SetUp]
        public void Init()
        {
            _user = new UserIdentity
            {
                Email = "UserName",
                AgencyId = Guid.NewGuid(),
                BuType = BuType.Pg,
                Id = Guid.NewGuid()
            };

            _activityLogServiceMock = new Mock<IActivityLogService>();
            _target = new PgCustomDataObjectIoNumberAdded(_activityLogServiceMock.Object);
        }

        [Test]
        public void CanProcess_None_ReturnsFalse()
        {
            var action = PostProcessingAction.None;

            var result = _target.CanProcess(action);

            result.Should().BeFalse();
        }

        [Test]
        public void CanProcess_CustomDataObjectSaved_ReturnsTrue()
        {
            var action = PostProcessingAction.CustomObjectDataSaved;

            var result = _target.CanProcess(action);

            result.Should().BeTrue();
        }

        [Test]
        public async Task Process_Null_DoesNothing()
        {
            CustomObjectData data = null;

            await _target.Process(_user, data);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_NonCost_DoesNothing()
        {
            string cost = "Not a cost object";

            await _target.Process(_user, cost);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_NullData_DoesNothing()
        {
            string data = null;
            var customObjectData = new CustomObjectData
            {
                Data = data
            };

            await _target.Process(_user, customObjectData);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_InvalidData_DoesNothing()
        {
            var data = "{\"grNumber\":\"\",\"ioNber\":\"1234567892\",\"po\":\"\"}";
            var customObjectData = new CustomObjectData
            {
                Data = data
            };

            await _target.Process(_user, customObjectData);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Never);
        }

        [Test]
        public async Task Process_IoNumber_SendsActivity()
        {
            var data = "{\"grNumber\":\"\",\"ioNumber\":\"1234567892\",\"poNumber\":\"\"}";
            var customObjectData = new CustomObjectData
            {
                Data = data
            };            

            await _target.Process(_user, customObjectData);

            _activityLogServiceMock.Verify(s => s.Log(It.IsAny<IActivityLogEntry>()), Times.Once);
        }
    }
}
