using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class OtpRecord
{
[FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
 public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("code")]
    public string Code { get; set; } = string.Empty;

    [FirestoreProperty("purpose")]
    public string Purpose { get; set; } = string.Empty; // "registration" | "login"

    [FirestoreProperty("expiresAt")]
 public DateTime ExpiresAt { get; set; }

    [FirestoreProperty("used")]
    public bool Used { get; set; }

  [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
