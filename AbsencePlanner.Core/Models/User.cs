using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class User
{
    [FirestoreProperty("id")]
public string Id { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("phone")]
public string Phone { get; set; } = string.Empty;

    [FirestoreProperty("location")]
    public string Location { get; set; } = string.Empty;

    [FirestoreProperty("role")]
    public string Role { get; set; } = "user";

    [FirestoreProperty("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [FirestoreProperty("status")]
    public string Status { get; set; } = "pending_verification";

    [FirestoreProperty("emailVerified")]
    public bool EmailVerified { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
