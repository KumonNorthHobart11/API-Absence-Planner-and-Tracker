using System.Globalization;
using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class AbsenceService : IAbsenceService
{
    private readonly IFirestoreRepository _repo;
    private readonly INotificationService _notif;
    private const string Col = "absences";

    public AbsenceService(IFirestoreRepository repo, INotificationService notif)
    {
   _repo = repo; _notif = notif;
    }

    public async Task<List<Absence>> GetAllAsync(string userId, string role)
    {
        var all = await _repo.GetAllAsync<Absence>(Col);
        return role is "admin" or "superadmin" ? all : all.Where(a => a.UserId == userId).ToList();
    }

    public async Task<Absence> GetByIdAsync(string id) =>
        await _repo.GetAsync<Absence>(Col, id) ?? throw new KeyNotFoundException("Absence not found.");

    public async Task<List<Absence>> GetByStatusAsync(string status, string userId, string role)
    {
 var all = await GetAllAsync(userId, role);
        return all.Where(a => a.Status == status).ToList();
    }

    public async Task<Absence> CreateAsync(CreateAbsenceRequest req, string userId, string userName)
{
        var students = await _repo.GetAllAsync<Student>("students");
        var student = students.FirstOrDefault(s => s.Id == req.StudentId && s.Users.Any(u => u.UserId == userId))
            ?? throw new ArgumentException("Student not linked to current user.");

     var start = DateOnly.ParseExact(req.StartDate, "yyyy-MM-dd");
        var end = DateOnly.ParseExact(req.EndDate, "yyyy-MM-dd");
        if (start > end) throw new ArgumentException("startDate must be <= endDate.");

        if (req.HolidayId != null)
        {
          var holiday = await _repo.GetAsync<Holiday>("holidays", req.HolidayId)
      ?? throw new KeyNotFoundException("Holiday not found.");
   var hStart = DateOnly.ParseExact(holiday.StartDate, "yyyy-MM-dd");
            var hEnd = DateOnly.ParseExact(holiday.EndDate, "yyyy-MM-dd");
          if (start < hStart || end > hEnd)
        throw new ArgumentException("Dates must be within the holiday range.");
        }

        var config = await _repo.GetAsync<CalendarDayConfig>("calendar_day_config", "default");
        if (config != null)
        {
            if (!config.AllowedDays.Contains(start.DayOfWeek.ToString()))
  throw new ArgumentException($"Start date falls on {start.DayOfWeek} which is not an allowed day.");
        if (!config.AllowedDays.Contains(end.DayOfWeek.ToString()))
 throw new ArgumentException($"End date falls on {end.DayOfWeek} which is not an allowed day.");
        }

        var absence = new Absence
   {
            Id = Guid.NewGuid().ToString(), StudentId = req.StudentId, StudentName = student.Name,
         UserId = userId, UserName = userName, HolidayId = req.HolidayId,
   StartDate = req.StartDate, EndDate = req.EndDate, Reason = req.Reason,
            HomeworkLoad = req.HomeworkLoad, DigitalKumon = req.DigitalKumon, Status = "pending"
        };
        await _repo.SetAsync(Col, absence.Id, absence);
        await _notif.CreateForAdminsAsync($"New absence submitted by {userName} for {student.Name}", "absence_submitted");
        return absence;
    }

    public async Task UpdateAsync(string id, UpdateAbsenceRequest req, string userId)
    {
     var absence = await GetByIdAsync(id);
    if (absence.UserId != userId) throw new UnauthorizedAccessException("Not your absence.");
        if (absence.LockedBy != null && absence.LockedBy != userId && absence.LockedAt.HasValue && (DateTime.UtcNow - absence.LockedAt.Value).TotalMinutes < 5)
      throw new InvalidOperationException("Absence is locked by another user.");

        var start = DateOnly.ParseExact(req.StartDate, "yyyy-MM-dd");
    var end = DateOnly.ParseExact(req.EndDate, "yyyy-MM-dd");
        if (start > end) throw new ArgumentException("startDate must be <= endDate.");

        if (absence.HolidayId != null)
     {
        var holiday = await _repo.GetAsync<Holiday>("holidays", absence.HolidayId);
            if (holiday != null)
      {
    var hStart = DateOnly.ParseExact(holiday.StartDate, "yyyy-MM-dd");
     var hEnd = DateOnly.ParseExact(holiday.EndDate, "yyyy-MM-dd");
      if (start < hStart || end > hEnd)
       throw new ArgumentException("Dates must be within the holiday range.");
 }
        }

        var config = await _repo.GetAsync<CalendarDayConfig>("calendar_day_config", "default");
        if (config != null)
        {
     if (!config.AllowedDays.Contains(start.DayOfWeek.ToString()))
            throw new ArgumentException($"Start date falls on disallowed day.");
   if (!config.AllowedDays.Contains(end.DayOfWeek.ToString()))
                throw new ArgumentException($"End date falls on disallowed day.");
   }

 await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
   ["startDate"] = req.StartDate, ["endDate"] = req.EndDate,
            ["reason"] = req.Reason, ["updatedAt"] = DateTime.UtcNow,
       ["lockedBy"] = null, ["lockedAt"] = null
        });
    }

    public async Task LockAsync(string id, string userId)
    {
  var absence = await GetByIdAsync(id);
        if (absence.LockedBy != null && absence.LockedBy != userId && absence.LockedAt.HasValue && (DateTime.UtcNow - absence.LockedAt.Value).TotalMinutes < 5)
  throw new InvalidOperationException("Absence is locked by another user.");

await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
  {
            ["lockedBy"] = userId, ["lockedAt"] = DateTime.UtcNow
        });
    }

    public async Task UnlockAsync(string id)
    {
      await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
   ["lockedBy"] = null, ["lockedAt"] = null
      });
    }

    public async Task ApproveAsync(string id)
    {
        var absence = await GetByIdAsync(id);
     await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
         ["status"] = "approved", ["updatedAt"] = DateTime.UtcNow
     });
        await _notif.CreateAsync(absence.UserId, $"Your absence for {absence.StudentName} has been approved.", "absence_approved");
    }

    public async Task RejectAsync(string id)
    {
    var absence = await GetByIdAsync(id);
        await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
        ["status"] = "rejected", ["updatedAt"] = DateTime.UtcNow
      });
        await _notif.CreateAsync(absence.UserId, $"Your absence for {absence.StudentName} has been rejected.", "absence_rejected");
    }

    public async Task<List<ExpiredSubmissionDto>> GetExpiredAsync(string userId)
    {
        var holidays = await _repo.GetAllAsync<Holiday>("holidays");
     var students = (await _repo.GetAllAsync<Student>("students")).Where(s => s.Users.Any(u => u.UserId == userId)).ToList();
        var absences = await _repo.GetAllAsync<Absence>(Col);
    var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<ExpiredSubmissionDto>();

        foreach (var h in holidays)
        {
            if (!DateOnly.TryParseExact(h.SubmissionDeadline, "yyyy-MM-dd", null, DateTimeStyles.None, out var deadline)) continue;
          if (deadline >= now) continue;
       foreach (var s in students)
   {
        if (!absences.Any(a => a.HolidayId == h.Id && a.StudentId == s.Id))
result.Add(new ExpiredSubmissionDto(h.Id, h.Name, s.Id, s.Name));
            }
        }
        return result;
    }
}
