using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CseLibrary.Services.Services;

public class SecurityAuditService(IUnitOfWork uow, ILogger<SecurityAuditService> logger) : ISecurityAuditService
{
    // Best-effort (D-27): the user is the actor of their own sign-in event; a failure goes to the log only.
    public async Task RecordSignInEventAsync(string userId, string action, CancellationToken ct = default)
    {
        try
        {
            var row = SecurityEvent.For(SecurityEvent.User, userId, action);
            row.ActorUserId = userId;
            await uow.AuditLogs.AddAsync(row, ct);
            await uow.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Security event {Action} for user {UserId} could not be written", action, userId);
        }
    }
}
