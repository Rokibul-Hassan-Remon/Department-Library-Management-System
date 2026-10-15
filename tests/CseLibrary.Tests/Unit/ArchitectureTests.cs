using System.Reflection;
using System.Xml.Linq;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data;
using CseLibrary.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Tests.Unit;

/// <summary>Layer rules D-10, D-12, D-13 and the project structure D-41 (tests E1, E3, E4).</summary>
public class ArchitectureTests
{
    private static readonly Assembly Web = typeof(Program).Assembly;
    private static readonly Assembly Services = typeof(UserService).Assembly;
    private static readonly Assembly Core = typeof(IUnitOfWork).Assembly;
    private static readonly Assembly Data = typeof(ApplicationDbContext).Assembly;

    [Fact] // E1; RoleManager: role lists come from IRoleService (D-56 item 8)
    public void Controllers_take_no_DbContext_unit_of_work_repository_or_RoleManager()
    {
        var offenders = Web.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => (t, p.ParameterType)))
            .Where(x => typeof(DbContext).IsAssignableFrom(x.ParameterType) || x.ParameterType == typeof(IUnitOfWork)
                        || IsRepository(x.ParameterType)
                        || x.ParameterType.IsGenericType && x.ParameterType.GetGenericTypeDefinition() == typeof(Microsoft.AspNetCore.Identity.RoleManager<>))
            .Select(x => $"{x.t.Name}({x.ParameterType.Name})");
        Assert.Empty(offenders);
    }

    [Fact] // A6 + A10 (FR-M9-05, D-31): class-level [Authorize] everywhere; anonymous only on the allowlist;
           // every other action has [HasPermission] unless it is authenticated-only
    public void Every_controller_requires_login_and_only_the_allowlist_is_anonymous()
    {
        string[] allowlist = ["Account.AccessDenied", "Account.Login", "Home.Error", "Home.HttpStatus", "Home.Index", "Home.Privacy"];
        string[] authenticatedOnly = ["Account.ChangePassword", "Account.Logout"];

        var controllers = Web.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract).ToList();
        Assert.Empty(controllers.Where(c => !c.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)).Select(c => c.Name));

        var actions = controllers.SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && !m.IsDefined(typeof(NonActionAttribute)))
                .Select(m => (Name: $"{c.Name[..^"Controller".Length]}.{m.GetCustomAttribute<ActionNameAttribute>()?.Name ?? m.Name}", Method: m)))
            .ToList();
        var anonymous = actions.Where(a => a.Method.IsDefined(typeof(Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute)));
        Assert.Equal(allowlist, anonymous.Select(a => a.Name).Distinct().Order());

        var unguarded = actions.Where(a => !anonymous.Contains(a) && !authenticatedOnly.Contains(a.Name)
                                           && !a.Method.IsDefined(typeof(CseLibrary.Web.Authorization.HasPermissionAttribute)));
        Assert.Empty(unguarded.Select(a => a.Name));
    }

    [Fact] // E1: assemblies actually referenced, not namespaces (UserManager lives in Microsoft.Extensions.Identity.Core)
    public void Services_and_Core_reference_no_Data_EF_Core_or_ASP_NET_Core()
    {
        Assert.Empty(Forbidden(Services, "CseLibrary.Data", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"));
        Assert.Empty(Forbidden(Core, "CseLibrary.Data", "CseLibrary.Services", "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore"));

        foreach (var project in new[] { "CseLibrary.Services", "CseLibrary.Core" })
            Assert.DoesNotContain(Project(project).Descendants("FrameworkReference"),
                e => (string?)e.Attribute("Include") == "Microsoft.AspNetCore.App");
    }

    [Fact] // E1
    public void Unit_of_work_has_no_generic_or_Type_based_repository_accessor()
    {
        var accessors = typeof(IUnitOfWork).GetMethods()
            .Where(m => m.IsGenericMethod && IsRepository(m.ReturnType) || m.GetParameters().Any(p => p.ParameterType == typeof(Type)));
        Assert.Empty(accessors);
        Assert.False(typeof(IDisposable).IsAssignableFrom(typeof(IUnitOfWork)));
    }

    [Fact] // E1
    public void Repositories_expose_no_IQueryable_or_template_methods()
    {
        string[] banned = ["Query", "GetAllAsync", "FindAsync", "Update"];
        var members = Core.GetTypes().Where(IsRepository).SelectMany(t => t.GetMethods());
        Assert.Empty(members.Where(m => banned.Contains(m.Name) || IsQueryable(m.ReturnType)).Select(m => m.Name));
    }

    [Fact] // E1: the UoW owns SaveChanges
    public void Repository_sources_never_call_SaveChanges()
    {
        var offenders = Directory.GetFiles(Path.Combine(RepoRoot, "src", "CseLibrary.Data", "Repositories"), "*.cs")
            .Where(f => Path.GetFileName(f) != "UnitOfWork.cs" && File.ReadAllText(f).Contains("SaveChanges"));
        Assert.Empty(offenders);
    }

    [Fact] // E3
    public void Services_never_inject_repositories_directly()
    {
        var offenders = Services.GetTypes()
            .SelectMany(t => t.GetConstructors().SelectMany(c => c.GetParameters()).Select(p => (t, p.ParameterType)))
            .Where(x => IsRepository(x.ParameterType))
            .Select(x => $"{x.t.Name}({x.ParameterType.Name})");
        Assert.Empty(offenders);
    }

    [Fact] // E4
    public void Solution_has_the_four_layers_plus_tests_with_allowed_references_only()
    {
        var projects = File.ReadAllLines(Path.Combine(RepoRoot, "CseLibrary.sln"))
            .Where(l => l.StartsWith("Project(") && l.Contains(".csproj"))
            .Select(l => Path.GetFileNameWithoutExtension(l.Split(',')[1].Trim().Trim('"')))
            .Order();
        Assert.Equal(["CseLibrary.Core", "CseLibrary.Data", "CseLibrary.Services", "CseLibrary.Tests", "CseLibrary.Web"], projects);

        Assert.Equal(["CseLibrary.Core", "CseLibrary.Data", "CseLibrary.Services"], References("CseLibrary.Web"));
        Assert.Equal(["CseLibrary.Core"], References("CseLibrary.Services"));
        Assert.Equal(["CseLibrary.Core"], References("CseLibrary.Data"));
        Assert.Empty(References("CseLibrary.Core"));

        var oldNames = new[] { Web, Services, Core, Data }
            .SelectMany(a => a.GetTypes().Select(t => t.Namespace ?? "").Append(a.GetName().Name!))
            .Where(n => n.StartsWith("Default"));
        Assert.Empty(oldNames);
    }

    private static bool IsRepository(Type t) =>
        t.IsInterface && t.Assembly == Core && t.Name.Contains("Repository");

    private static bool IsQueryable(Type t) =>
        t.IsGenericType && typeof(IQueryable).IsAssignableFrom(t)
        || t.IsGenericType && t.GetGenericArguments().Any(IsQueryable);

    private static IEnumerable<string> Forbidden(Assembly assembly, params string[] prefixes) =>
        assembly.GetReferencedAssemblies().Select(a => a.Name!).Where(n => prefixes.Any(n.StartsWith));

    private static IEnumerable<string> References(string project) =>
        Project(project).Descendants("ProjectReference")
            .Select(e => Path.GetFileNameWithoutExtension(((string)e.Attribute("Include")!).Replace('\\', '/')))
            .Order();

    private static XDocument Project(string name) =>
        XDocument.Load(Path.Combine(RepoRoot, "src", name, name + ".csproj"));

    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "CseLibrary.sln"))) return dir.FullName;
        throw new InvalidOperationException("CseLibrary.sln not found above the test output folder.");
    }
}
