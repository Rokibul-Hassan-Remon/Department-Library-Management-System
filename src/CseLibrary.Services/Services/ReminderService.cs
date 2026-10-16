using CseLibrary.Core.Constants;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CseLibrary.Services.Services;

/// <summary>
/// Reminds members of loans due in <c>Lending.ReminderDaysBefore</c> days and of overdue loans. Each email has an
/// idempotency key in EmailNotificationLogs, so a rerun or restart never sends it twice; a failed one is retried.
/// </summary>
public class ReminderService(IUnitOfWork uow, IEmailSender email, TimeProvider time, ILogger<ReminderService> logger) : IReminderService
{
    public async Task<int> SendDueRemindersAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        var config = await uow.GeneralConfigs.GetByKeyAsync(LendingConfigKeys.ReminderDaysBefore, ct);
        var daysBefore = int.TryParse(config?.Value, out var d) && d >= 0 ? d : LendingConfigKeys.DefaultReminderDaysBefore;

        var targets = (await uow.Loans.GetReminderTargetsAsync(today.AddDays(daysBefore), ct)).Select(t =>
        {
            var type = t.DueDate < today ? EmailType.Overdue : EmailType.DueSoon;
            return (t, type, Key: $"{type}:{t.LoanId}:{t.DueDate:yyyyMMdd}"); // a renewal (new due date) earns a new reminder
        }).ToList();
        var logs = (await uow.EmailNotificationLogs.FindByKeysForUpdateAsync(targets.Select(x => x.Key).ToList(), ct))
            .ToDictionary(l => l.IdempotencyKey); // one query for the whole run, not one per email

        var sent = 0;
        foreach (var (t, type, key) in targets)
        {
            var log = logs.GetValueOrDefault(key);
            if (log?.DeliveryStatus == DeliveryStatus.Sent) continue;
            if (log == null)
            {
                log = new EmailNotificationLog { LendingTransactionId = t.LoanId, EmailType = type, Recipient = t.Email, IdempotencyKey = key };
                await uow.EmailNotificationLogs.AddAsync(log, ct);
            }

            log.Subject = type == EmailType.Overdue ? $"Overdue: {t.BookTitle}" : $"Due on {t.DueDate:yyyy-MM-dd}: {t.BookTitle}";
            log.Attempts++;
            try
            {
                await email.SendAsync(t.Email, log.Subject,
                    $"Dear {t.MemberName},\n\nThe book \"{t.BookTitle}\" (copy {t.AccessionNumber}) " +
                    (type == EmailType.Overdue
                        ? $"was due on {t.DueDate:yyyy-MM-dd}. Please return it to the CSE library as soon as possible."
                        : $"is due on {t.DueDate:yyyy-MM-dd}. Please return or renew it by then.") +
                    "\n\nCSE Department Library", ct);
                log.DeliveryStatus = DeliveryStatus.Sent;
                log.SentAtUtc = time.GetUtcNow().UtcDateTime;
                log.LastError = null;
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.DeliveryStatus = DeliveryStatus.Failed;
                log.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                logger.LogWarning(ex, "Reminder {Key} failed; it is retried on the next run", key);
            }
            await uow.SaveChangesAsync(ct); // one row per email, so a crash mid-run loses at most one
        }
        return sent;
    }
}

/// <summary>No mail server yet: each email is written to the application log (Session 23).</summary>
public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("EMAIL (log only) to {To}: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
