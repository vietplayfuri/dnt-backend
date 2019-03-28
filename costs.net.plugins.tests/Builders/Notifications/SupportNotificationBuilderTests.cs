
namespace costs.net.plugins.tests.Builders.Notifications
{
    using System;
    using core.Models.Utils;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;
    using plugins.PG.Builders.Notifications;

    public class SupportNotificationBuilderTests
    {
        private SupportNotificationBuilder _target;
        private AppSettings _appSettings;
        private const string SupportEmailAddress = "adcosts.support.test@adstream.com";
        private const string CostOwnerGdamUserId = "57e5461ed9563f268ef4f19d";

        [SetUp]
        public void Init()
        {            
            _appSettings = new AppSettings();
            _appSettings.SupportEmailAddress = SupportEmailAddress;
            var appSettingsMock = new Mock<IOptions<AppSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_appSettings);

            _target = new SupportNotificationBuilder(appSettingsMock.Object);
        }

        [Test]
        public void PurchaseError_TechnicalError_Create_Notification_GenericAdcostMail()
        {
            //Arrange
            const string expectedActionType = "genericAdcostMail";
            const string costNumber = "V01";
            const string errorMessage = "Technical error";
            
            var objectHeader = $"Cost {costNumber}";
            var objectSubject = $"Technical issue with Cost {costNumber}";
            
            //Act
            var result = _target.BuildSupportErrorNotification(costNumber, errorMessage, CostOwnerGdamUserId);

            //Assert
            result.Should().NotBeNull();
            result.Action.Should().NotBeNull();
            result.Action.Type.Should().NotBeNull();
            result.Object.Should().NotBeNull();
            result.Object.Subject.Should().NotBeNull();
            result.Object.Header.Should().NotBeNull();
            result.Object.Body.Should().NotBeNull();

            result.Recipients.Should().NotBeNull();
            result.Subject.Should().NotBeNull();
            result.Viewers.Should().NotBeNull();

            result.Action.Type.Should().Be(expectedActionType);
            result.Type.Should().NotBeNull();
            result.Type.Should().Be(expectedActionType); //Paper-Pusher uses .Type, MS uses Action.Type. Both should be same.
            result.Object.Type.Length.Should().BeGreaterThan(0); //Required by Paper-Pusher

            result.Timestamp.Should().BeOnOrBefore(DateTime.UtcNow); //Not a future notification

            //Identifiers are set to something other than Null/Empty
            result.Id.Should().NotBe(Guid.Empty);
            result.Object.Id.Should().NotBe(Guid.Empty.ToString());
            result.Subject.Id.Should().NotBe(Guid.Empty.ToString());

            result.Object.Subject.Should().NotBeNull();
            result.Object.Subject.Should().Be(objectSubject);
            result.Object.Header.Should().NotBeNull();
            result.Object.Header.Should().Be(objectHeader);
            result.Object.Body.Should().NotBeNull();
            result.Object.Body.Should().Contain(errorMessage);

            result.Recipients.ToArray()[0].Should().Be(CostOwnerGdamUserId);

            result.Parameters.EmailService.AdditionalEmails.Should().Contain(SupportEmailAddress);
        }
    }
}
