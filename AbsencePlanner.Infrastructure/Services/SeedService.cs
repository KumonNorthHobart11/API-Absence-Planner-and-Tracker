using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;
using Microsoft.Extensions.Options;

namespace AbsencePlanner.Infrastructure.Services;

public class SeedService
{
    private readonly IFirestoreRepository _repo;
    private readonly IOptionsMonitor<AppSettings> _appSettings;

    public SeedService(IFirestoreRepository repo, IOptionsMonitor<AppSettings> appSettings)
    {
        _repo = repo;
        _appSettings = appSettings;
    }

    public async Task SeedAsync()
    {
        var currentVersion = _appSettings.CurrentValue.SeedVersion;

        var configSnap = await _repo.Collection("app_config").Document("default").GetSnapshotAsync();
        var seededVersion = 0;
        if (configSnap.Exists && configSnap.ContainsField("seedVersion"))
            seededVersion = configSnap.GetValue<int>("seedVersion");

        if (seededVersion >= currentVersion)
            return;

        // Clear stale seed collections
        await ClearCollectionAsync("users");
        await ClearCollectionAsync("calendar_day_config");
        await ClearCollectionAsync("menu_permissions");
        await ClearCollectionAsync("feature_permissions");

        var pw = BCrypt.Net.BCrypt.HashPassword("990099");

        // ── SuperAdmin only ───────────────────────────────────────────────────────
        await _repo.SetAsync("users", "u1", new User
        {
            Id = "u1",
            Name = "Super Admin",
            Email = "superadmin@app.com",
            Phone = "1000000001",
            Location = "HQ",
            Role = "superadmin",
            PasswordHash = pw,
            Status = "active",
            EmailVerified = true
        });

        // ── Calendar days ─────────────────────────────────────────────────────────
        await _repo.SetAsync("calendar_day_config", "default", new CalendarDayConfig
        {
            AllowedDays = new() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" }
        });

        // ── Menu permissions ──────────────────────────────────────────────────────
        // MenuKey must match exactly what the frontend sends to /api/permissions/check-menu
        var menus = new List<MenuPermission>
        {
         // Visible to all roles
      new() { MenuKey = "dashboard",        Label = "Dashboard",         Roles = new() { "superadmin", "admin", "user" } },
      new() { MenuKey = "students",    Label = "Student Records",       Roles = new() { "superadmin", "admin", "user" } },
            new() { MenuKey = "absences",          Label = "Absence Approval",      Roles = new() { "superadmin", "admin", "user" } },

          // Admin + superadmin only
            new() { MenuKey = "holidays", Label = "Holiday Periods", Roles = new() { "superadmin", "admin" } },
            new() { MenuKey = "users",    Label = "Parent Approvals",      Roles = new() { "superadmin", "admin" } },
            new() { MenuKey = "student_removal",   Label = "Student Removal",       Roles = new() { "superadmin", "admin" } },
          new() { MenuKey = "packing_report",    Label = "Daily Packing Report",  Roles = new() { "superadmin", "admin" } },

        // SuperAdmin only
            new() { MenuKey = "menu_permission",   Label = "Menu Permission",       Roles = new() { "superadmin" } },
     new() { MenuKey = "feature_permission",Label = "Feature Permission", Roles = new() { "superadmin" } },
          new() { MenuKey = "calendar_control",  Label = "Calendar Day Control",  Roles = new() { "superadmin" } },
    new() { MenuKey = "system_user",       Label = "Add System User",       Roles = new() { "superadmin" } },
        };

        foreach (var m in menus)
            await _repo.SetAsync("menu_permissions", m.MenuKey, m);

        // ── Feature permissions ───────────────────────────────────────────────────
        var features = new List<FeaturePermission>();

        foreach (var menu in menus)
        {
            // superadmin: full access to every menu
            features.Add(new FeaturePermission
            {
                Id = $"{menu.MenuKey}_superadmin",
                MenuKey = menu.MenuKey,
                Role = "superadmin",
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
                CanView = true
            });

            // admin: canAdd, canEdit, canView — no delete
            if (menu.Roles.Contains("admin"))
                features.Add(new FeaturePermission
                {
                    Id = $"{menu.MenuKey}_admin",
                    MenuKey = menu.MenuKey,
                    Role = "admin",
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = false,
                    CanView = true
                });

            // user: canView; canAdd only on absences & students
            if (menu.Roles.Contains("user"))
                features.Add(new FeaturePermission
                {
                    Id = $"{menu.MenuKey}_user",
                    MenuKey = menu.MenuKey,
                    Role = "user",
                    CanAdd = menu.MenuKey is "absences" or "students",
                    CanEdit = false,
                    CanDelete = false,
                    CanView = true
                });
        }

        foreach (var f in features)
            await _repo.SetAsync("feature_permissions", f.Id, f);

        // ── Persist seed version ──────────────────────────────────────────────────
        await _repo.Collection("app_config").Document("default")
               .SetAsync(new Dictionary<string, object>
               {
                   ["seeded"] = true,
                   ["seedVersion"] = currentVersion
               });
    }

    private async Task ClearCollectionAsync(string collection)
    {
        var snap = await _repo.Collection(collection).GetSnapshotAsync();
        foreach (var doc in snap.Documents)
            await _repo.DeleteAsync(collection, doc.Id);
    }
}
