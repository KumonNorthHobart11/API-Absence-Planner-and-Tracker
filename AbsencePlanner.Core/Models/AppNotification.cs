using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class AppNotification
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("message")]
    public string Message { get; set; } = string.Empty;

    [FirestoreProperty("type")]
    public string Type { get; set; } = string.Empty;

    [FirestoreProperty("read")]
    public bool Read { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
