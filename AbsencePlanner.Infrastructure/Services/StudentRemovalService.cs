using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class StudentRemovalService : IStudentRemovalService
{
    private readonly IFirestoreRepository _repo;
    private readonly INotificationService _notif;
    private const string Col = "student_removal_requests";

    public StudentRemovalService(IFirestoreRepository repo, INotificationService notif)
    {
     _repo = repo; _notif = notif;
  }

    public async Task<List<StudentRemovalRequest>> GetAllAsync(string userId, string role)
    {
    var all = await _repo.GetAllAsync<StudentRemovalRequest>(Col);
    return role is "admin" or "superadmin" ? all : all.Where(r => r.RequestedBy == userId).ToList();
    }

    public async Task<List<StudentRemovalRequest>> GetByStatusAsync(string status, string userId, string role)
{
        var all = await GetAllAsync(userId, role);
        return all.Where(r => r.Status == status).ToList();
    }

public async Task<StudentRemovalRequest> CreateAsync(CreateRemovalRequest req, string userId, string userName)
    {
        if (string.IsNullOrWhiteSpace(req.Reason)) throw new ArgumentException("Reason is required.");

        var existing = await _repo.GetAllAsync<StudentRemovalRequest>(Col);
        if (existing.Any(r => r.StudentId == req.StudentId && r.RequestedBy == userId && r.Status == "pending"))
            throw new InvalidOperationException("A pending removal request already exists.");

        var student = await _repo.GetAsync<Student>("students", req.StudentId) ?? throw new KeyNotFoundException("Student not found.");

     var request = new StudentRemovalRequest
        {
   Id = Guid.NewGuid().ToString(), StudentId = req.StudentId, StudentName = student.Name,
      RequestedBy = userId, RequestedByName = userName, Reason = req.Reason, Status = "pending"
   };
   await _repo.SetAsync(Col, request.Id, request);
      await _notif.CreateForAdminsAsync($"Student removal requested for {student.Name} by {userName}", "student_removal_pending");
 return request;
    }

    public async Task ApproveAsync(string id, string adminUserId)
    {
 var req = await _repo.GetAsync<StudentRemovalRequest>(Col, id) ?? throw new KeyNotFoundException("Request not found.");
   await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
     ["status"] = "approved", ["reviewedBy"] = adminUserId, ["updatedAt"] = DateTime.UtcNow
        });
      await _repo.DeleteAsync("students", req.StudentId);
        await _notif.CreateAsync(req.RequestedBy, $"Your removal request for {req.StudentName} has been approved.", "student_removal_approved");
    }

    public async Task RejectAsync(string id, string adminUserId)
    {
        var req = await _repo.GetAsync<StudentRemovalRequest>(Col, id) ?? throw new KeyNotFoundException("Request not found.");
        await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
   {
            ["status"] = "rejected", ["reviewedBy"] = adminUserId, ["updatedAt"] = DateTime.UtcNow
  });
      await _notif.CreateAsync(req.RequestedBy, $"Your removal request for {req.StudentName} has been rejected.", "student_removal_rejected");
    }
}
