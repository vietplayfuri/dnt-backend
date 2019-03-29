using System;
using costs.net.core.Services.Notifications;
using FluentScheduler;
using Serilog;

namespace costs.net.scheduler.core.Jobs
{
    using net.core.Services.ActivityLog;

    public class PurgeJob : IJob
    {
        private static readonly ILogger Logger = Log.ForContext<PurgeJob>();

        private readonly IPurgeReminderService _purgeReminderService;

        public PurgeJob(IPurgeReminderService purgeReminderService)
        {
            _purgeReminderService = purgeReminderService;
        }

        public async void Execute()
        {
            try
            {
                int count;
                Logger.Information("Starting PurgeJob!");
                count = await _purgeReminderService.DeleteSentOrCancelledReminders();
                Logger.Information($"Purged {count} sent or cancelled reminders.");
                count = await _purgeReminderService.DeleteDeliveredLogs();
                Logger.Information($"Purged {count} activity logs that have been delivered to PaperPusher.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred whilst executing PurgeJob.");
            }
            finally
            {
                Logger.Information("Finished PurgeJob!");
            }
        }
    }
}
