using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class StudentRemovalRequest
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("studentId")]
    public string StudentId { get; set; } = string.Empty;

    [FirestoreProperty("studentName")]
    public string StudentName { get; set; } = string.Empty;

    [FirestoreProperty("requestedBy")]
    public string RequestedBy { get; set; } = string.Empty;

    [FirestoreProperty("requestedByName")]
    public string RequestedByName { get; set; } = string.Empty;

    [FirestoreProperty("reason")]
    public string Reason { get; set; } = string.Empty;

    [FirestoreProperty("status")]
    public string Status { get; set; } = "pending";

    [FirestoreProperty("reviewedBy")]
    public string? ReviewedBy { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
