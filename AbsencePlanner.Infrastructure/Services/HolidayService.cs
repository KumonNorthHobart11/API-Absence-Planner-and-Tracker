using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class HolidayService : IHolidayService
{
    private readonly IFirestoreRepository _repo;
    private readonly INotificationService _notif;
    private const string Col = "holidays";

    public HolidayService(IFirestoreRepository repo, INotificationService notif)
    {
        _repo = repo; _notif = notif;
    }

    public Task<List<Holiday>> GetAllAsync() => _repo.GetAllAsync<Holiday>(Col);

    public async Task<Holiday> GetByIdAsync(string id) =>
        await _repo.GetAsync<Holiday>(Col, id) ?? throw new KeyNotFoundException("Holiday not found.");

    public async Task<Holiday> CreateAsync(CreateHolidayRequest req, string userId)
    {
        var h = new Holiday
        {
            Id = Guid.NewGuid().ToString(),
            Name = req.Name,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            SubmissionDeadline = req.SubmissionDeadline,
            Description = req.Description,
            CreatedBy = userId
        };
        await _repo.SetAsync(Col, h.Id, h);
        await _notif.CreateAsync("all", $"New holiday: {h.Name} ({h.StartDate} to {h.EndDate})", "holiday");
        return h;
    }

    public async Task UpdateAsync(string id, UpdateHolidayRequest req)
    {
        await GetByIdAsync(id);
        await _repo.UpdateFieldsAsync(Col, id, new Dictionary<string, object?>
        {
            ["name"] = req.Name,
            ["startDate"] = req.StartDate,
            ["endDate"] = req.EndDate,
            ["submissionDeadline"] = req.SubmissionDeadline,
            ["description"] = req.Description
        });
    }

    public async Task DeleteAsync(string id)
    {
        await GetByIdAsync(id);
        await _repo.DeleteAsync(Col, id);
    }
}
