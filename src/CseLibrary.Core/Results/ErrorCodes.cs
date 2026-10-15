namespace CseLibrary.Core.Results;

public static class ErrorCodes
{
    public const string NotFound = "COMMON.NOT_FOUND";
    public const string Duplicate = "COMMON.DUPLICATE";
    public const string IdentityFailed = "IDENTITY.FAILED";
    public const string Forbidden = "SECURITY.FORBIDDEN";
    public const string LastAdmin = "SECURITY.LAST_ADMIN";
    public const string InUse = "COMMON.IN_USE";
    public const string InvalidState = "COMMON.INVALID_STATE";
    public const string MemberInactive = "MEMBER.INACTIVE";
    public const string LimitReached = "LENDING.LIMIT_REACHED";
    public const string HasOverdue = "LENDING.HAS_OVERDUE";
    public const string CopyUnavailable = "LENDING.COPY_UNAVAILABLE";
    public const string RenewNotAllowed = "LENDING.RENEW_NOT_ALLOWED";
    public const string AlreadyRegistered = "REGISTRATION.ALREADY_REGISTERED";
}
