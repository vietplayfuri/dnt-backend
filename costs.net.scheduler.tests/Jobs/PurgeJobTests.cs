using System.Threading.Tasks;
using costs.net.core.Services.Notifications;
using costs.net.scheduler.core.Jobs;
using Moq;
using NUnit.Framework;

namespace costs.net.scheduler.tests.Jobs
{
    using net.core.Services.ActivityLog;

    [TestFixture]
    public class PurgeJobTests
    {
        private Mock<IPurgeReminderService> _purgeReminderService;

        [SetUp]
        public void Init()
        {
            _purgeReminderService = new Mock<IPurgeReminderService>();

            _purgeReminderService.Setup(e => e.DeleteSentOrCancelledReminders()).Returns(Task.FromResult(10));
            _purgeReminderService.Setup(e => e.DeleteDeliveredLogs()).Returns(Task.FromResult(15));
        }

        [Test]
        public void ExecuteTest()
        {
            //Arrange
            var target = new PurgeJob(_purgeReminderService.Object);

            //Act
            target.Execute();

            //Assert
            _purgeReminderService.Verify(e => e.DeleteSentOrCancelledReminders(), Times.Once);
            _purgeReminderService.Verify(e => e.DeleteDeliveredLogs(), Times.Once);
        }
    }
}

