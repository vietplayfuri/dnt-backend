using System;
using System.Linq;
using System.Threading.Tasks;
using costs.net.core.Services.Notifications;
using costs.net.dataAccess;
using costs.net.dataAccess.Entity;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using costs.net.tests.common.Stubs.EFContext;

namespace costs.net.core.tests.Services.Notifications
{
    [TestFixture]
    public class EmailNotificationReminderServiceTests
    {
        private Mock<EFContext> _efContextMock;

        [SetUp]
        public void Init()
        {
            _efContextMock = new Mock<EFContext>();
        }

        [Test]
        public void ReminderSending_UpdateStatus()
        {
            //Arrange
            var target = new EmailNotificationReminderService(Log.Logger, _efContextMock.Object);
            var reminder = new EmailNotificationReminder
            {
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Remind
            };

            //Act
            target.ReminderSending(reminder);

            //Assert
            reminder.ReminderStatus.Should().Be(EmailReminderStatus.Reminding);
        }

        [Test]
        public void ReminderSent_UpdateStatus()
        {
            //Arrange
            var target = new EmailNotificationReminderService(Log.Logger, _efContextMock.Object);
            var reminder = new EmailNotificationReminder
            {
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Reminding
            };

            //Act
            target.ReminderSent(reminder);

            //Assert
            reminder.ReminderStatus.Should().Be(EmailReminderStatus.Reminded);
        }

        [Test]
        public async Task ReminderCancelled_UpdateStatus_OnePendingReminderForCost()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            var target = new EmailNotificationReminderService(Log.Logger, _efContextMock.Object);
            var reminder = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Remind
            };
            var reminders = new[] { reminder };
            _efContextMock.MockAsyncQueryable(reminders.AsQueryable(), c => c.EmailNotificationReminder);

            //Act
            await target.CancelReminder(costId);

            //Assert
            reminder.ReminderStatus.Should().Be(EmailReminderStatus.Cancelled);
        }

        [Test]
        public async Task ReminderCancelled_UpdateStatus_OnePendingReminderAndOthersDoneForCost()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            var target = new EmailNotificationReminderService(Log.Logger, _efContextMock.Object);
            var reminderOne = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Cancelled
            };
            var reminderTwo = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Remind
            };
            var reminderThree = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Reminded
            };
            var reminders = new[] { reminderOne, reminderTwo, reminderThree };
            _efContextMock.MockAsyncQueryable(reminders.AsQueryable(), c => c.EmailNotificationReminder);

            //Act
            await target.CancelReminder(costId);

            //Assert
            reminderOne.ReminderStatus.Should().Be(EmailReminderStatus.Cancelled);
            reminderTwo.ReminderStatus.Should().Be(EmailReminderStatus.Cancelled);
            reminderThree.ReminderStatus.Should().Be(EmailReminderStatus.Reminded);
        }

        [Test]
        public async Task ReminderCancelled_UpdateStatus_NoPendingRemindersForCost()
        {
            //Arrange
            Guid costId = Guid.NewGuid();
            var target = new EmailNotificationReminderService(Log.Logger, _efContextMock.Object);
            var reminderOne = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Cancelled
            };
            var reminderTwo = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Cancelled
            };
            var reminderThree = new EmailNotificationReminder
            {
                CostId = costId,
                Id = Guid.NewGuid(),
                ReminderStatus = EmailReminderStatus.Reminded
            };
            var reminders = new[] { reminderOne, reminderTwo, reminderThree };
            _efContextMock.MockAsyncQueryable(reminders.AsQueryable(), c => c.EmailNotificationReminder);

            //Act
            await target.CancelReminder(costId);

            //Assert
            reminderOne.ReminderStatus.Should().Be(EmailReminderStatus.Cancelled);
            reminderTwo.ReminderStatus.Should().Be(EmailReminderStatus.Cancelled);
            reminderThree.ReminderStatus.Should().Be(EmailReminderStatus.Reminded);
        }
    }
}
