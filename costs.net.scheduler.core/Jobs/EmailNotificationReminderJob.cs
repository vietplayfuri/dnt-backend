using System;
using System.Collections.Generic;
using System.Linq;
using costs.net.core.Builders.Notifications;
using costs.net.core.ExternalResource.Paperpusher;
using costs.net.core.Models.Notifications;
using costs.net.core.Services.Notifications;
using costs.net.dataAccess;
using costs.net.dataAccess.Entity;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace costs.net.scheduler.core.Jobs
{
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    public class EmailNotificationReminderJob : IJob
    {
        private static readonly ILogger Logger = Log.ForContext<EmailNotificationReminderJob>();

        private readonly EFContext _efContext;
        private readonly IEmailNotificationReminderService _emailNotificationReminderService;
        private readonly IEmailNotificationBuilder _emailNotificationBuilder;
        private readonly IPaperpusherClient _paperpusherClient;

        public EmailNotificationReminderJob(EFContext efContext,
            IEmailNotificationReminderService emailNotificationReminderService,
            IEmailNotificationBuilder emailNotificationBuilder,
            IPaperpusherClient paperpusherClient)
        {
            _efContext = efContext;
            _emailNotificationReminderService = emailNotificationReminderService;
            _emailNotificationBuilder = emailNotificationBuilder;
            _paperpusherClient = paperpusherClient;
        }

        public async void Execute()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StringBuilder logMessages = new StringBuilder();
            try
            {
                logMessages.AppendLine("Starting EmailNotificationJob!");

                await ProcessReminders(logMessages);
            }
            catch (Exception ex)
            {
                logMessages.AppendLine($"[Error] An error occurred whilst executing EmailNotificationReminderJob: {ex.ToString()}");
            }
            finally
            {
                sw.Stop();
                logMessages.AppendLine($"Finished EmailNotificationJob! - Effort: {sw.ElapsedMilliseconds}");
                Logger.Information(logMessages.ToString());
            }
        }

        private async Task ProcessReminders(StringBuilder logMessages)
        {
            try
            {
                _emailNotificationReminderService.ReminderUpdateToSending();

                List<EmailNotificationReminder> sentReminders = new List<EmailNotificationReminder>();
                Dictionary<EmailNotificationReminder, EmailReminderStatus> updateReminders = new Dictionary<EmailNotificationReminder, EmailReminderStatus>();
                var remindingEmails = await _emailNotificationReminderService.GetEmailNotificationReminderByStatus(EmailReminderStatus.Reminding, 10);

                List<CostStageRevisionStatus> invalidStatuses = new List<CostStageRevisionStatus>();
                invalidStatuses.Add(CostStageRevisionStatus.PendingBrandApproval);
                invalidStatuses.Add(CostStageRevisionStatus.PendingTechnicalApproval);

                List<CostStageRevisionStatus> approvedStatuses = new List<CostStageRevisionStatus>();
                approvedStatuses.Add(CostStageRevisionStatus.Approved);

                foreach (var reminder in remindingEmails)
                {
                    logMessages.AppendLine($"START: sending reminder for cost: {reminder.CostId}");

                    //Get the cost
                    var cost = await _efContext.Cost
                        .AsNoTracking()
                        .Include(c => c.LatestCostStageRevision)
                        .ThenInclude(csr => csr.CostStage)
                        .Include(c => c.Project)
                        .ThenInclude(p => p.Brand)
                        .Include(c => c.Owner)
                            .ThenInclude(o => o.Agency)
                                .ThenInclude(a => a.Country)
                        .FirstAsync(c => c.Id == reminder.CostId);

                    var users = new CostNotificationUsers
                    {
                        CostOwner = cost.Owner
                    };

                    if (approvedStatuses.Contains(cost.LatestCostStageRevision.Status))
                    {
                        updateReminders.Add(reminder, EmailReminderStatus.Reminded);
                    }
                    else if (invalidStatuses.Contains(cost.LatestCostStageRevision.Status))
                    {
                        //Build
                        var notificationMessages = await _emailNotificationBuilder.BuildCostReminderNotification(users, cost, cost.LatestCostStageRevision, DateTime.UtcNow);

                        //Send
                        bool sent = false;
                        foreach (var notification in notificationMessages)
                        {
                            sent = await _paperpusherClient.SendMessage(notification);

                            if (!sent)
                            {
                                logMessages.AppendLine($"[Error] Failed to send notification for cost: {reminder.CostId}");
                            }
                        }

                        //Update
                        if (sent)
                        {
                            sentReminders.Add(reminder);
                            logMessages.AppendLine($"FINISHED: Sent reminder for cost: {reminder.CostId}");
                        }
                    }
                    else
                    {
                        updateReminders.Add(reminder, EmailReminderStatus.Cancelled);
                    }
                }
                _emailNotificationReminderService.ReminderUpdateToSent(sentReminders);
                _emailNotificationReminderService.ReminderUpdateToSent(updateReminders);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
