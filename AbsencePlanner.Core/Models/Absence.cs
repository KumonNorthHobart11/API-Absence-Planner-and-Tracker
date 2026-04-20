using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class Absence
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("studentId")]
    public string StudentId { get; set; } = string.Empty;

    [FirestoreProperty("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("userName")]
    public string UserName { get; set; } = string.Empty;

    [FirestoreProperty("holidayId")]
    public string? HolidayId { get; set; }

[FirestoreProperty("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [FirestoreProperty("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [FirestoreProperty("reason")]
    public string Reason { get; set; } = string.Empty;

    [FirestoreProperty("homeworkLoad")]
    public string HomeworkLoad { get; set; } = string.Empty;

    [FirestoreProperty("digitalKumon")]
    public bool DigitalKumon { get; set; }

    [FirestoreProperty("status")]
    public string Status { get; set; } = "pending";

    [FirestoreProperty("lockedBy")]
    public string? LockedBy { get; set; }

    [FirestoreProperty("lockedAt")]
    public DateTime? LockedAt { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
