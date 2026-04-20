using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class Holiday
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("startDate")]
    public string StartDate { get; set; } = string.Empty;

    [FirestoreProperty("endDate")]
    public string EndDate { get; set; } = string.Empty;

    [FirestoreProperty("submissionDeadline")]
    public string SubmissionDeadline { get; set; } = string.Empty;

    [FirestoreProperty("description")]
    public string Description { get; set; } = string.Empty;

    [FirestoreProperty("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
