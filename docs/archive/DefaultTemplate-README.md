# DefaultTemplate — .NET 8 MVC + Onion Architecture

A reusable starter template that wires up the prerequisites every internal project needs, so new apps can be cloned and customised quickly.

## Stack

| Layer              | Project                | Responsibilities                                                 |
| ------------------ | ---------------------- | ---------------------------------------------------------------- |
| Domain (innermost) | `Default.core`         | Entities, DTOs, service & repository interfaces, permissions     |
| Infrastructure     | `Default.repository`   | EF Core 8 + SQL Server, Identity store, generic repo, UoW, seeder |
| Application        | `Default.service`      | Business services, AutoMapper profiles                            |
| Presentation       | `DefaultTemplate`      | MVC controllers, Razor views, permission-based authorization      |

Dependency direction (Onion):
```
DefaultTemplate ──► Default.service ──► Default.repository ──► Default.core
```

## Built-in features

- **ASP.NET Core Identity** with `ApplicationUser` / `ApplicationRole`
- **Permission-based authorization** via `[HasPermission("Module.Action")]` attribute, claims-cached on login
- **Generic Repository + UnitOfWork** (`IRepository<T>`, `IUnitOfWork`)
- **AutoMapper** with auto-registered profiles
- **Serilog** with rolling file + console sinks (config in `appsettings.json`)
- **DB seeder**: creates default permissions, Admin role with all permissions, default admin user, sample general configs
- **CRUD modules**: Users, Roles, Permissions, GeneralConfig (with appropriate permission gating)

## First-run setup

### 1. Configure connection string
Edit `DefaultTemplate/appsettings.json` → `ConnectionStrings:DefaultConnection`. Defaults to LocalDB.

### 2. Create the initial migration
From the solution root:

```powershell
dotnet ef migrations add InitialCreate --project Default.repository --startup-project DefaultTemplate
```

If `dotnet-ef` isn't installed:
```powershell
dotnet tool install --global dotnet-ef --version 8.0.10
```

### 3. Set the seed admin password (required)
The seeder will refuse to start if no admin password is configured. Use **user-secrets** in development so credentials never end up in source control:

```powershell
dotnet user-secrets set "Seed:Admin:Password" "<a-strong-password>" --project DefaultTemplate
```

For production/staging, set `Seed__Admin__Password` as an environment variable or pull it from Azure Key Vault / your secret store of choice.

You can also override the email, role, and full name through the same `Seed:Admin` section in `appsettings.json` if you don't want the defaults (`admin@local`, role `Admin`).

### 4. Run
```powershell
dotnet run --project DefaultTemplate
```

On first run, `DbSeeder` applies pending migrations, seeds permissions/roles, and creates the admin user with the email/password you configured. Sign in, then change the password from `/Account/ChangePassword` (or rotate the secret) before sharing the environment.

## Adding a new module

1. **Entity** in `Default.core/Entities` (extend `BaseEntity`).
2. **DTOs** in `Default.core/DTOs`.
3. **Service interface** in `Default.core/Interfaces/Services` and **service** in `Default.service/Services` (register in `Default.service/DependencyInjection.cs`).
4. **EF configuration** in `Default.repository/Data/Configurations` and a `DbSet<T>` in `ApplicationDbContext`.
5. **Permission constants** in `Default.core/Constants/Permissions.cs` (add to `All()` so seeder picks them up).
6. **Controller + views** in the web project, gated with `[HasPermission(Permissions.YourModule.View)]`.
7. **AutoMapper** mappings in `Default.service/Mapping/MappingProfile.cs`.
8. Add a migration: `dotnet ef migrations add Add<Module> --project Default.repository --startup-project DefaultTemplate`.

## Notes

- `AutoMapper 12.0.1` ships with a known advisory (NU1903). Bump to a newer line when you adopt this template if you need to clear it.
- The bootstrap `LocalDB` connection assumes Windows. Replace for non-Windows hosts.
- Permission claims are stamped onto the user at login via `AccountController.AddPermissionClaimsAsync`; if you change a role's permissions while a user is signed in, they need to re-login (or call `RefreshSignInAsync`) to pick them up.
