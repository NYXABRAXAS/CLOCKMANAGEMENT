using Microsoft.EntityFrameworkCore;
using STLMS.Domain.Entities;

namespace STLMS.Infrastructure.Persistence.Seed;

/// <summary>Modules referenced by permission seeding. Expanded as later milestones introduce
/// new modules (Alarms, Habits, Religion, etc.) - each just adds another entry here.</summary>
public static class PermissionModules
{
    public static readonly string[] All =
    [
        "DASHBOARD", "USERS", "ROLES", "RELIGIONS", "SETTINGS", "AUDIT_LOGS", "PROFILE",
    ];
}

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(context);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context);
        await SeedReligionsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        context.Roles.AddRange(
            new Role { Code = RoleCodes.SuperAdmin, Name = "Super Admin", Description = "Full unrestricted access.", IsSystem = true, SortOrder = 1 },
            new Role { Code = RoleCodes.Admin, Name = "Admin", Description = "Manages users, religions, and application settings.", IsSystem = true, SortOrder = 2 },
            new Role { Code = RoleCodes.PremiumUser, Name = "Premium User", Description = "Full feature access on a paid subscription.", IsSystem = true, SortOrder = 3 },
            new Role { Code = RoleCodes.StandardUser, Name = "Standard User", Description = "Free-tier feature access.", IsSystem = true, SortOrder = 4 },
            new Role { Code = RoleCodes.Guest, Name = "Guest", Description = "Read-only trial access.", IsSystem = true, SortOrder = 5 }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAsync(AppDbContext context)
    {
        var existing = await context.Permissions.Select(p => p.Module + ":" + p.Action).ToListAsync();
        var toAdd = new List<Permission>();
        foreach (var module in PermissionModules.All)
        {
            foreach (var action in new[] { "view", "create", "edit", "delete" })
            {
                var key = $"{module}:{action}";
                if (!existing.Contains(key)) toAdd.Add(new Permission { Module = module, Action = action });
            }
        }
        if (toAdd.Count != 0)
        {
            context.Permissions.AddRange(toAdd);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolePermissionsAsync(AppDbContext context)
    {
        var superAdmin = await context.Roles.SingleAsync(r => r.Code == RoleCodes.SuperAdmin);
        var admin = await context.Roles.SingleAsync(r => r.Code == RoleCodes.Admin);
        var premium = await context.Roles.SingleAsync(r => r.Code == RoleCodes.PremiumUser);
        var standard = await context.Roles.SingleAsync(r => r.Code == RoleCodes.StandardUser);
        var guest = await context.Roles.SingleAsync(r => r.Code == RoleCodes.Guest);

        var allPermissions = await context.Permissions.ToListAsync();
        var existingPairs = (await context.RolePermissions.Select(rp => new { rp.RoleId, rp.PermissionId }).ToListAsync())
            .Select(x => (x.RoleId, x.PermissionId)).ToHashSet();

        void Grant(Role role, IEnumerable<Permission> permissions)
        {
            foreach (var permission in permissions)
            {
                if (existingPairs.Contains((role.Id, permission.Id))) continue;
                context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }

        // Super Admin and Admin: everything.
        Grant(superAdmin, allPermissions);
        Grant(admin, allPermissions.Where(p => p.Module != "ROLES" || p.Action == "view"));

        // Premium/Standard: full self-service access to their own data, no admin modules.
        var selfServiceModules = new[] { "DASHBOARD", "SETTINGS", "PROFILE" };
        Grant(premium, allPermissions.Where(p => selfServiceModules.Contains(p.Module)));
        Grant(standard, allPermissions.Where(p => selfServiceModules.Contains(p.Module)));

        // Everyone can read the religions list (it's reference data used by the Settings picker) -
        // only Admin/SuperAdmin can manage the list itself (grant above already covers that).
        Grant(premium, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));
        Grant(standard, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));
        Grant(guest, allPermissions.Where(p => p.Module == "RELIGIONS" && p.Action == "view"));

        // Guest: view-only.
        Grant(guest, allPermissions.Where(p => selfServiceModules.Contains(p.Module) && p.Action == "view"));

        await context.SaveChangesAsync();
    }

    private static async Task SeedReligionsAsync(AppDbContext context)
    {
        if (await context.Religions.AnyAsync()) return;

        context.Religions.AddRange(
            new Religion { Code = ReligionCodes.Islam, Name = "Islam", IsSystem = true, SortOrder = 1 },
            new Religion { Code = ReligionCodes.Hinduism, Name = "Hinduism", IsSystem = true, SortOrder = 2 },
            new Religion { Code = ReligionCodes.Christianity, Name = "Christianity", IsSystem = true, SortOrder = 3 },
            new Religion { Code = ReligionCodes.Sikhism, Name = "Sikhism", IsSystem = true, SortOrder = 4 },
            new Religion { Code = ReligionCodes.Buddhism, Name = "Buddhism", IsSystem = true, SortOrder = 5 },
            new Religion { Code = ReligionCodes.Jainism, Name = "Jainism", IsSystem = true, SortOrder = 6 },
            new Religion { Code = ReligionCodes.Judaism, Name = "Judaism", IsSystem = true, SortOrder = 7 },
            new Religion { Code = ReligionCodes.Other, Name = "Other", IsSystem = true, SortOrder = 8 }
        );
        await context.SaveChangesAsync();
    }
}
