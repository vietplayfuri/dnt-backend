
namespace costs.net.plugins.PG.Builders.Notifications
{
    using core.Builders.Notifications;
    using core.Models.Notifications;
    using core.Models.Utils;
    using Microsoft.Extensions.Options;

    public class SupportNotificationBuilder : ISupportNotificationBuilder
    {
        private readonly AppSettings _appSettings;

        public SupportNotificationBuilder(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public EmailNotificationMessage<GenericNotificationObject> BuildGenericErrorNotification(string costNumber, string errorMessage, string gdamUserId, string[] additionalEmails = null)
        {
            var body = errorMessage;
            var header = $"Cost {costNumber}";
            var subject = $"Technical issue with Cost {costNumber}";

            var notificationBuilder = new GenericNotificationBuilder();

            return notificationBuilder.Build(gdamUserId, subject, header, body, additionalEmails);
        }

        public EmailNotificationMessage<GenericNotificationObject> BuildSupportErrorNotification(string costNumber, string errorMessage, string gdamUserId = null)
        {
            var supportEmail = _appSettings.SupportEmailAddress;

            return BuildGenericErrorNotification(costNumber, errorMessage, gdamUserId ?? supportEmail, new[] { supportEmail });
        }

        public EmailNotificationMessage<GenericNotificationObject> BuildGenericNotification(string costNumber, string body, string subject, string gdamUserId, string[] additionalEmails = null)
        {
            var header = $"Cost {costNumber}";

            var notificationBuilder = new GenericNotificationBuilder();

            return notificationBuilder.Build(gdamUserId, subject, header, body, additionalEmails);
        }

        public EmailNotificationMessage<SubmissionFailedNotificationObject> BuildSubmissionFailedNotification(string costNumber, string gdamUserId = null)
        {
            var supportEmail = _appSettings.SupportEmailAddress;

            var notificationBuilder = new SubmissionFailedNotificationBuilder();

            return notificationBuilder.Build(gdamUserId ?? supportEmail, costNumber, new[] { supportEmail });
        }
    }
}
