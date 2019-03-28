using System.Collections.Generic;
using costs.net.core.Models.Notifications;

namespace costs.net.plugins.PG.Builders.Notifications
{
    class GenericNotificationBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipientGdamUserId">This can either be an A5 user or an email address. If this is an email address, 
        /// ensure the same email address is passed into the <see cref="additionalEmails"/> parameter.
        /// </param>
        /// <param name="subject">The email subject</param>
        /// <param name="header">The header of the email, like a H1 for the email.</param>
        /// <param name="body">The main content of the email.</param>
        /// <param name="additionalEmails">An enumerable collection of email addresses to send this email to.</param>
        /// <returns></returns>
        internal EmailNotificationMessage<GenericNotificationObject> Build(string recipientGdamUserId, string subject, string header, string body, IEnumerable<string> additionalEmails = null)
        {
            var notificationMessage = new EmailNotificationMessage<GenericNotificationObject>(core.Constants.EmailNotificationActionType.GenericAdCost, recipientGdamUserId);
            GenericNotificationObject obj = notificationMessage.Object;

            obj.Body = body;
            obj.Header = header;
            obj.Subject = subject;

            if (additionalEmails != null)
            {
                foreach (var email in additionalEmails)
                {
                    notificationMessage.Parameters.EmailService.AdditionalEmails.Add(email);
                }
            }

            return notificationMessage;
        }
    }
}
