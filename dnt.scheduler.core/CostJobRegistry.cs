using costs.net.scheduler.core.Jobs;
using FluentScheduler;

namespace costs.net.scheduler.core
{
    public class CostJobRegistry : Registry
    {
        public CostJobRegistry()
        {
            //Prevent concurrent jobs
            NonReentrantAsDefault();

            // Schedule Email Notification Reminder job to run now and then every ten minutes
            Schedule<EmailNotificationReminderJob>().ToRunNow().AndEvery(1).Minutes();

            // Schedule the Purge job to run at a specific time every day
            Schedule<PurgeJob>().ToRunEvery(1).Days().At(06, 31);

            // Schedule Activity Log Delivery job to every minute
            Schedule<ActivityLogDeliveryJob>().ToRunEvery(1).Minutes();
        }
    }
}
