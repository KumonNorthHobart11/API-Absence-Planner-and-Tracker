using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IFirestoreRepository _repo;
    private const string Col = "notifications";

    public NotificationService(IFirestoreRepository repo) => _repo = repo;

    public async Task CreateAsync(string userId, string message, string type)
    {
        var n = new AppNotification { Id = Guid.NewGuid().ToString(), UserId = userId, Message = message, Type = type };
        await _repo.SetAsync(Col, n.Id, n);
    }

    public async Task CreateForAdminsAsync(string message, string type)
    {
        var users = await _repo.GetAllAsync<User>("users");
        foreach (var u in users.Where(u => u.Role is "admin" or "superadmin"))
            await CreateAsync(u.Id, message, type);
    }

    public async Task<List<AppNotification>> GetForUserAsync(string userId)
    {
        var all = await _repo.GetAllAsync<AppNotification>(Col);
        return all.Where(n => n.UserId == userId || n.UserId == "all").OrderByDescending(n => n.CreatedAt).ToList();
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        var list = await GetForUserAsync(userId);
        return list.Count(n => !n.Read);
    }

    public async Task MarkReadAsync(string notificationId)
    {
        await _repo.UpdateFieldsAsync(Col, notificationId, new Dictionary<string, object?> { ["read"] = true });
    }

    public async Task MarkAllReadAsync(string userId)
    {
        var list = await GetForUserAsync(userId);
        foreach (var n in list.Where(n => !n.Read && n.UserId != "all"))
            await MarkReadAsync(n.Id);
    }
}
