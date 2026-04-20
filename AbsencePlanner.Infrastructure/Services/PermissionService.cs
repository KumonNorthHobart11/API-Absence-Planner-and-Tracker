using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IFirestoreRepository _repo;

    public PermissionService(IFirestoreRepository repo) => _repo = repo;

    public Task<List<MenuPermission>> GetMenuPermissionsAsync() => _repo.GetAllAsync<MenuPermission>("menu_permissions");

    public async Task SaveMenuPermissionsAsync(List<MenuPermissionDto> perms, string callerRole)
    {
        foreach (var p in perms)
        {
            if (callerRole == "admin" && p.Roles.Contains("superadmin"))
                throw new UnauthorizedAccessException("Admin cannot modify superadmin role assignments.");
            var mp = new MenuPermission { MenuKey = p.MenuKey, Label = p.Label, Roles = p.Roles };
            await _repo.SetAsync("menu_permissions", mp.MenuKey, mp);
        }
    }

    public async Task<List<FeaturePermission>> GetFeaturePermissionsAsync(string? role)
    {
        var all = await _repo.GetAllAsync<FeaturePermission>("feature_permissions");
        return role != null ? all.Where(f => f.Role == role).ToList() : all;
    }

    public async Task SaveFeaturePermissionsAsync(List<FeaturePermissionDto> perms, string callerRole)
    {
        foreach (var p in perms)
        {
            if (callerRole == "admin" && p.Role == "superadmin")
                throw new UnauthorizedAccessException("Admin cannot modify superadmin feature permissions.");
            var fp = new FeaturePermission
            {
                Id = $"{p.MenuKey}_{p.Role}",
                MenuKey = p.MenuKey,
                Role = p.Role,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete,
                CanView = p.CanView
            };
            await _repo.SetAsync("feature_permissions", fp.Id, fp);
        }
    }

    public async Task<CalendarDayConfig> GetCalendarDaysAsync() =>
      await _repo.GetAsync<CalendarDayConfig>("calendar_day_config", "default") ?? new CalendarDayConfig();

    public async Task SaveCalendarDaysAsync(CalendarDaysRequest req)
    {
        await _repo.SetAsync("calendar_day_config", "default", new CalendarDayConfig { AllowedDays = req.AllowedDays });
    }

    public async Task<bool> CheckMenuAsync(string menuKey, string role)
    {
        if (role == "superadmin") return true;
        var mp = await _repo.GetAsync<MenuPermission>("menu_permissions", menuKey);
        return mp?.Roles.Contains(role) ?? false;
    }

    public async Task<bool> CheckFeatureAsync(string menuKey, string role, string action)
    {
        if (role == "superadmin") return true;
        var fp = await _repo.GetAsync<FeaturePermission>("feature_permissions", $"{menuKey}_{role}");
        if (fp == null) return false;
        return action switch
        {
            "add" => fp.CanAdd,
            "edit" => fp.CanEdit,
            "delete" => fp.CanDelete,
            "view" => fp.CanView,
            _ => false
        };
    }
}
