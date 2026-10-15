using CseLibrary.Core.Interfaces.Services;

namespace CseLibrary.Web.Infrastructure;

/// <summary>Runs the due/overdue reminders at startup and then once a day.</summary>
public sealed class ReminderWorker(IServiceScopeFactory scopes, ILogger<ReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var sent = await scope.ServiceProvider.GetRequiredService<IReminderService>().SendDueRemindersAsync(stoppingToken);
                logger.LogInformation("Due-date reminders sent: {Count}", sent);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Due-date reminder run failed; retrying at the next run");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
