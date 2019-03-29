namespace costs.net.scheduler.core.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using dataAccess.Entity;
    using FluentScheduler;
    using net.core.ExternalResource.Paperpusher;
    using net.core.Models.ActivityLog;
    using net.core.Services.ActivityLog;
    using Serilog;

    public class ActivityLogDeliveryJob : IJob
    {
        private static readonly ILogger Logger = Log.ForContext<ActivityLogDeliveryJob>();

        private readonly IActivityLogService _service;
        private readonly IPaperpusherClient _paperPusherClient;

        public ActivityLogDeliveryJob(IActivityLogService service, IPaperpusherClient paperPusherClient)
        {
            _service = service;
            _paperPusherClient = paperPusherClient;
        }

        public async void Execute()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StringBuilder logMessage = new StringBuilder();
            try
            {
                logMessage.AppendLine("Starting ActivityLogDeliveryJob!");

                await _service.UpdateEntriesToProcessing();

                var entries = await _service.GetProcessingActivityLogs();
                if (entries == null || !entries.Any())
                {
                    //not entries to send
                    logMessage.AppendLine("No activity log entries ready for delivery.");
                }
                else
                {
                    List<ActivityLog> successLogs = new List<ActivityLog>();
                    Dictionary<ActivityLog, ActivityLogMessage> failedLogs = new Dictionary<ActivityLog, ActivityLogMessage>();
                    foreach (var entry in entries)
                    {
                        ActivityLogMessage message = null;
                        try
                        {
                            entry.ActivityLogDelivery.RetryCount++;

                            logMessage.AppendLine($"Building Activity Log: {entry.Id}");
                            message = _service.BuildLogMessage(entry);
                            logMessage.AppendLine($"Built Activity Log Message: {message.Message}");

                            logMessage.AppendLine($"Sending Activity Log: {entry.Id}");
                            var success = _paperPusherClient.SendMessage(message.Message).Result;
                            if (success)
                            {
                                logMessage.AppendLine($"Successfully sent Activity Log: {entry.Id}");
                                successLogs.Add(entry);
                            }
                            else
                            {
                                logMessage.AppendLine($"[Warning] Failed to send Activity Log: {entry.Id}");
                                failedLogs.Add(entry, message);
                            }
                        }
                        catch (Exception ex)
                        {
                            logMessage.AppendLine($"[Error] Failed to send Activity Log: {entry.Id} - {ex}");
                            failedLogs.Add(entry, message);
                        }
                    }

                    _service.EntriesDeliveredSuccessfully(successLogs);
                    await _service.EntriesDeliveryFailed(failedLogs);
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine($"[Error] An error occurred whilst executing ActivityLogDeliveryJob. - {ex}");
            }
            finally
            {
                sw.Stop();
                logMessage.AppendLine($"Finished ActivityLogDeliveryJob! - Effort: {sw.ElapsedMilliseconds}");
                Logger.Information(logMessage.ToString());
            }
        }
    }
}
