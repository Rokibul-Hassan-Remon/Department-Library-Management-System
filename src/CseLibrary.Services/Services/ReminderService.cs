using CseLibrary.Core.Constants;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CseLibrary.Services.Services;

/// <summary>Reminds members of loans due in <c>Lending.ReminderDaysBefore</c> days and of overdue loans. No fines in v1.</summary>
public class ReminderService(IUnitOfWork uow, IEmailSender email, TimeProvider time) : IReminderService
{
    // ponytail: no "already sent" record; an overdue loan is reminded on every run (daily) and a restart re-sends today's batch.
    // Add a LastRemindedOn column on Loan when real email is switched on and duplicates matter.
    public async Task<int> SendDueRemindersAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(time.GetLocalNow().DateTime);
        var config = await uow.GeneralConfigs.GetByKeyAsync(LendingConfigKeys.ReminderDaysBefore, ct);
        var daysBefore = int.TryParse(config?.Value, out var d) && d >= 0 ? d : LendingConfigKeys.DefaultFor(LendingConfigKeys.ReminderDaysBefore);

        var targets = await uow.Loans.GetReminderTargetsAsync(today.AddDays(daysBefore), today, ct);
        foreach (var t in targets)
        {
            var overdue = t.DueDate < today;
            await email.SendAsync(t.Email,
                overdue ? $"Overdue: {t.BookTitle}" : $"Due on {t.DueDate:yyyy-MM-dd}: {t.BookTitle}",
                $"Dear {t.MemberName},\n\nThe book \"{t.BookTitle}\" (copy {t.AccessionNumber}) " +
                (overdue ? $"was due on {t.DueDate:yyyy-MM-dd}. Please return it to the CSE library as soon as possible."
                         : $"is due on {t.DueDate:yyyy-MM-dd}. Please return or renew it by then.") +
                "\n\nCSE Department Library", ct);
        }
        return targets.Count;
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
