using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class MenuPermission
{
    [FirestoreProperty("menuKey")]
    public string MenuKey { get; set; } = string.Empty;

    [FirestoreProperty("label")]
    public string Label { get; set; } = string.Empty;

    [FirestoreProperty("roles")]
    public List<string> Roles { get; set; } = new();
}

[FirestoreData]
public class FeaturePermission
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("menuKey")]
  public string MenuKey { get; set; } = string.Empty;

    [FirestoreProperty("role")]
    public string Role { get; set; } = string.Empty;

    [FirestoreProperty("canAdd")]
    public bool CanAdd { get; set; }

    [FirestoreProperty("canEdit")]
    public bool CanEdit { get; set; }

    [FirestoreProperty("canDelete")]
    public bool CanDelete { get; set; }

    [FirestoreProperty("canView")]
    public bool CanView { get; set; }
}

[FirestoreData]
public class CalendarDayConfig
{
    [FirestoreProperty("allowedDays")]
    public List<string> AllowedDays { get; set; } = new();
}
