using System.Text;
using Microsoft.Extensions.Localization;
using NCrontab;
using WeddingShare.Constants;
using WeddingShare.Helpers;
using WeddingShare.Helpers.Database;
using WeddingShare.Helpers.Notifications;

namespace WeddingShare.BackgroundWorkers
{
    public sealed class NotificationReport(IServiceScopeFactory scopeFactory, ISettingsHelper settingsHelper, ISmtpClientWrapper smtpHelper, ILoggerFactory loggerFactory, IStringLocalizer<Lang.Translations> localizer) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var enabled = await settingsHelper.GetOrDefault(BackgroundServices.EmailReport.Enabled, true);
            if (enabled)
            {
                var cron = await settingsHelper.GetOrDefault(BackgroundServices.EmailReport.Schedule, "0 0 * * *");
                var nextExecutionTime = DateTime.Now.AddMinutes(1);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var currentCron = await settingsHelper.GetOrDefault(BackgroundServices.EmailReport.Schedule, "0 0 * * *");

                    var now = DateTime.Now;
                    if (now >= nextExecutionTime)
                    {
                        if (await settingsHelper.GetOrDefault(Settings.Basic.EmailReport, true) && await settingsHelper.GetOrDefault(Notifications.Smtp.Enabled, false))
                        {
                            await SendReport();
                        }

                        var schedule = CrontabSchedule.Parse(cron, new CrontabSchedule.ParseOptions() { IncludingSeconds = cron.Split(new[] { ' ' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length == 6 });
                        nextExecutionTime = schedule.GetNextOccurrence(now);
                    }
                    else
                    {
                        if (!currentCron.Equals(cron))
                        {
                            nextExecutionTime = DateTime.Now;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }

                    cron = currentCron;
                }
            }
        }

        private async Task SendReport()
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();

                        var pendingItems = await db.GetGalleryItems();
                        if (pendingItems != null && pendingItems.Any())
                        {
                            var builder = new StringBuilder();
                            builder.AppendLine($"<h1>You have items pending review!</h1>");

                            foreach (var item in pendingItems.GroupBy(x => x.GalleryId).OrderByDescending(x => x.Count()))
                            {
                                var gallery = await db.GetGallery(item.Key);
                                if (gallery != null)
                                {
                                    try
                                    {
                                        builder.AppendLine($"<p style=\"font-size: 16pt;\">{gallery.Name} - Pending Items ({item.Count()})</p>");
                                    }
                                    catch (Exception ex)
                                    {
                                        loggerFactory.CreateLogger<NotificationReport>().LogError(ex, $"Failed to build gallery report for '{gallery.Name}' - {ex?.Message}");
                                    }
                                }
                            }

                            var sent = await new EmailHelper(settingsHelper, smtpHelper, loggerFactory.CreateLogger<EmailHelper>(), localizer).Send("Pending Items Report", builder.ToString());
                            if (!sent)
                            {
                                loggerFactory.CreateLogger<NotificationReport>().LogWarning($"Failed to send notification report");
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger<NotificationReport>().LogError(ex, $"NotificationReport - Failed to send report - {ex?.Message}");
            }
        }
    }
}