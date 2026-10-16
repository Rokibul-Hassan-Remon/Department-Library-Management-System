namespace CseLibrary.Core.Constants;

public static class Permissions
{
    public const string ClaimType = "Permission";

    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Edit = "Users.Edit";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Roles.View";
        public const string Create = "Roles.Create";
        public const string Edit = "Roles.Edit";
        public const string Delete = "Roles.Delete";
        public const string AssignPermissions = "Roles.AssignPermissions";
    }

    public static class PermissionsModule
    {
        public const string View = "Permissions.View";
        public const string Create = "Permissions.Create";
        public const string Edit = "Permissions.Edit";
        public const string Delete = "Permissions.Delete";
    }

    public static class GeneralConfig
    {
        public const string View = "GeneralConfig.View";
        public const string Create = "GeneralConfig.Create";
        public const string Edit = "GeneralConfig.Edit";
        public const string Delete = "GeneralConfig.Delete";
    }

    public static class Books
    {
        public const string View = "Books.View";
        public const string Create = "Books.Create";
        public const string Edit = "Books.Edit";
        public const string Delete = "Books.Delete";
    }

    public static class LibraryMembers
    {
        public const string View = "LibraryMembers.View";
        public const string Create = "LibraryMembers.Create";
        public const string Edit = "LibraryMembers.Edit";
        public const string Delete = "LibraryMembers.Delete";
    }

    public static class Lending
    {
        public const string View = "Lending.View";
        public const string Issue = "Lending.Issue";
        public const string Return = "Lending.Return";
    }

    public static class Registrations
    {
        public const string Review = "Registrations.Review";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
    }

    /// <summary>
    /// D-56 Q-S21-5: the permissions a library member role may hold (Session 24). A role holding any other permission
    /// is privileged: it cannot be a registration default, and only an Admin may assign it.
    /// </summary>
    public static readonly string[] MemberAllowlist = [Books.View];

    public static IEnumerable<(string Name, string Module)> All()
    {
        yield return (Users.View, "Users");
        yield return (Users.Create, "Users");
        yield return (Users.Edit, "Users");
        yield return (Users.Delete, "Users");

        yield return (Roles.View, "Roles");
        yield return (Roles.Create, "Roles");
        yield return (Roles.Edit, "Roles");
        yield return (Roles.Delete, "Roles");
        yield return (Roles.AssignPermissions, "Roles");

        yield return (PermissionsModule.View, "Permissions");
        yield return (PermissionsModule.Create, "Permissions");
        yield return (PermissionsModule.Edit, "Permissions");
        yield return (PermissionsModule.Delete, "Permissions");

        yield return (GeneralConfig.View, "GeneralConfig");
        yield return (GeneralConfig.Create, "GeneralConfig");
        yield return (GeneralConfig.Edit, "GeneralConfig");
        yield return (GeneralConfig.Delete, "GeneralConfig");

        yield return (Books.View, "Books");
        yield return (Books.Create, "Books");
        yield return (Books.Edit, "Books");
        yield return (Books.Delete, "Books");

        yield return (LibraryMembers.View, "LibraryMembers");
        yield return (LibraryMembers.Create, "LibraryMembers");
        yield return (LibraryMembers.Edit, "LibraryMembers");
        yield return (LibraryMembers.Delete, "LibraryMembers");

        yield return (Lending.View, "Lending");
        yield return (Lending.Issue, "Lending");
        yield return (Lending.Return, "Lending");

        yield return (Registrations.Review, "Registrations");

        yield return (Reports.View, "Reports");
    }
}
