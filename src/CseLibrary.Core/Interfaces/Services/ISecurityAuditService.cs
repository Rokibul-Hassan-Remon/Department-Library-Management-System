namespace CseLibrary.Core.Interfaces.Services;

/// <summary>
/// D-24 / D-27: sign-in events (LoginSucceeded, LoginFailed, LockedOut, Logout) are written on their own and
/// best-effort — a failed write is logged and never breaks sign-in. Admin actions write their events in their own
/// transaction instead (UserService, RoleService).
/// </summary>
public interface ISecurityAuditService
{
    Task RecordSignInEventAsync(string userId, string action, CancellationToken ct = default);
}
