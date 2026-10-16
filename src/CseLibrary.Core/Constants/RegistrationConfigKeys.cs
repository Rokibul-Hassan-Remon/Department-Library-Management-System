using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Constants;

/// <summary>D-56 Q-S21-1: GeneralConfig keys naming the role a registered member gets; empty means no role.</summary>
public static class RegistrationConfigKeys
{
    public const string StudentDefaultRole = "Registration.StudentDefaultRole";
    public const string FellowDefaultRole = "Registration.FellowDefaultRole";

    public static readonly string[] All = [StudentDefaultRole, FellowDefaultRole];

    public static string DefaultRoleFor(MemberType type) => type == MemberType.Student ? StudentDefaultRole : FellowDefaultRole;
}
