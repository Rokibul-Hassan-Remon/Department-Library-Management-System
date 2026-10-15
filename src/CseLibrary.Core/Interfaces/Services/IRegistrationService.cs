using CseLibrary.Core.DTOs;
using CseLibrary.Core.Results;

namespace CseLibrary.Core.Interfaces.Services;

/// <summary>Public self-registration (D-54): Student → Active at once; Dept. Fellow → Pending until reviewed.</summary>
public interface IRegistrationService
{
    Task<Result> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PendingRegistrationDto>> GetPendingAsync(CancellationToken ct = default);
    Task<Result> ApproveAsync(int memberId, CancellationToken ct = default);
    Task<Result> RejectAsync(int memberId, CancellationToken ct = default);
}

/// <summary>Sends due-soon and overdue reminders; returns how many were sent.</summary>
public interface IReminderService
{
    Task<int> SendDueRemindersAsync(CancellationToken ct = default);
}

/// <summary>Outgoing mail. Today it only writes to the log; an SMTP sender replaces it when a mail server is chosen.</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
