using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Models;

[FirestoreData]
public class Student
{
    [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

 [FirestoreProperty("studentId")]
    public string StudentId { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("grade")]
    public string Grade { get; set; } = string.Empty;

    [FirestoreProperty("section")]
    public string Section { get; set; } = string.Empty;

    [FirestoreProperty("users")]
    public List<StudentUser> Users { get; set; } = new();

    [FirestoreProperty("subjects")]
    public List<Subject> Subjects { get; set; } = new();

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[FirestoreData]
public class StudentUser
{
    [FirestoreProperty("userId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

    [FirestoreProperty("email")]
    public string Email { get; set; } = string.Empty;

    [FirestoreProperty("phone")]
    public string Phone { get; set; } = string.Empty;

    [FirestoreProperty("location")]
    public string Location { get; set; } = string.Empty;

    [FirestoreProperty("relation")]
    public string Relation { get; set; } = string.Empty;
}

[FirestoreData]
public class Subject
{
  [FirestoreProperty("id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;

  [FirestoreProperty("schedules")]
    public List<ClassSchedule> Schedules { get; set; } = new();
}

[FirestoreData]
public class ClassSchedule
{
    [FirestoreProperty("day")]
    public string Day { get; set; } = string.Empty;

    [FirestoreProperty("startTime")]
  public string StartTime { get; set; } = string.Empty;

    [FirestoreProperty("endTime")]
 public string EndTime { get; set; } = string.Empty;
}
