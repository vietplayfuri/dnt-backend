using System.Collections.Generic;
using costs.net.core.Models.Notifications;

namespace costs.net.plugins.PG.Builders.Notifications
{
    internal class SubmissionFailedNotificationBuilder
    {
        /// <summary>
        /// Creates an email, notifying support that there's a technical problem with submitting this cost
        /// </summary>
        /// <param name="recipientGdamUserId">This can either be an A5 user or an email address. If this is an email address, 
        /// ensure the same email address is passed into the <see cref="additionalEmails"/> parameter.
        /// </param>
        /// <param name="costNumber">Unique display-identifier of affected cost</param>
        /// <param name="additionalEmails">An enumerable collection of email addresses to send this email to.</param>
        /// <returns></returns>
        internal EmailNotificationMessage<SubmissionFailedNotificationObject> Build(string recipientGdamUserId, string costNumber, IEnumerable<string> additionalEmails = null)
        {
            var notificationMessage = new EmailNotificationMessage<SubmissionFailedNotificationObject>(core.Constants.EmailNotificationActionType.SubmissionFailed, recipientGdamUserId);
            notificationMessage.Object.CostNumber = costNumber;
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
