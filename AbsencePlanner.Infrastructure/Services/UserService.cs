using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IFirestoreRepository _repo;
    private readonly INotificationService _notif;
    private readonly ITokenRevocationService _revocation;
  private const string Col = "users";

    public UserService(IFirestoreRepository repo, INotificationService notif, ITokenRevocationService revocation)
    {
        _repo = repo; _notif = notif; _revocation = revocation;
    }

    public Task<List<User>> GetAllAsync() => _repo.GetAllAsync<User>(Col);

    public async Task<User> GetByIdAsync(string id) =>
     await _repo.GetAsync<User>(Col, id) ?? throw new KeyNotFoundException("User not found.");

    public async Task<User> CreateAsync(CreateUserRequest req)
    {
        var users = await _repo.GetAllAsync<User>(Col);
  if (users.Any(u => u.Email.Equals(req.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Email already in use.");
        if (users.Any(u => u.Phone == req.Phone))
            throw new InvalidOperationException("Phone already in use.");

        var user = new User
    {
      Id = Guid.NewGuid().ToString(),
            Name = req.Name, Email = req.Email, Phone = req.Phone, Location = req.Location,
     Role = req.Role, PasswordHash = BCrypt.Net.BCrypt.HashPassword("990099"),
     Status = "active", EmailVerified = true
        };
 await _repo.SetAsync(Col, user.Id, user);
        return user;
    }

    public async Task UpdateAsync(string id, UpdateUserRequest req)
    {
      await GetByIdAsync(id);
    await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
  ["name"] = req.Name, ["email"] = req.Email, ["phone"] = req.Phone,
      ["location"] = req.Location, ["role"] = req.Role
        });
    }

    public async Task DeleteAsync(string id)
    {
     var target = await GetByIdAsync(id);
        await _repo.DeleteAsync(Col, id);
        // Revoke any active JWT for this user immediately
        await _revocation.RevokeAsync(id);
    }

    public async Task DeleteAdminAsync(string targetId, string callerRole)
    {
  if (callerRole != "superadmin")
   throw new UnauthorizedAccessException("Only superadmin can delete admin accounts.");

   var target = await GetByIdAsync(targetId);

        if (target.Role != "admin")
     throw new InvalidOperationException($"User '{target.Name}' is not an admin.");

   if (target.Role == "superadmin")
     throw new InvalidOperationException("Superadmin accounts cannot be deleted.");

      await _repo.DeleteAsync(Col, targetId);
  // Immediately invalidate any active JWT for the deleted admin
   await _revocation.RevokeAsync(targetId);
        await _notif.CreateAsync(targetId, "Your admin account has been deleted by the Super Admin.", "account_deleted");
    }

    public async Task<List<User>> GetPendingApprovalAsync() =>
        (await _repo.GetAllAsync<User>(Col)).Where(u => u.Status == "pending_approval").ToList();

    public async Task<List<User>> GetRejectedAsync() =>
        (await _repo.GetAllAsync<User>(Col)).Where(u => u.Status == "rejected").ToList();

    public async Task ApproveAsync(string id)
    {
  var user = await GetByIdAsync(id);
        if (user.Status != "pending_approval") throw new InvalidOperationException("User is not pending approval.");
        await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?> { ["status"] = "active" });
   await _notif.CreateAsync(id, "Your registration has been approved!", "registration_approved");
    }

    public async Task RejectAsync(string id)
    {
     var user = await GetByIdAsync(id);
   await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?> { ["status"] = "rejected" });
        await _notif.CreateAsync(id, "Your registration has been rejected.", "registration_rejected");
    }
}
