using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class RevokedToken
{
  [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("revokedAt")]
 public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
}
