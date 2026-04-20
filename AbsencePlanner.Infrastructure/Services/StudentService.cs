using AbsencePlanner.Core.DTOs;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;

namespace AbsencePlanner.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly IFirestoreRepository _repo;
    private const string Col = "students";

    public StudentService(IFirestoreRepository repo) => _repo = repo;

    public async Task<List<Student>> GetAllAsync(string userId, string role)
    {
        var all = await _repo.GetAllAsync<Student>(Col);
        if (role is "admin" or "superadmin") return all;
        return all.Where(s => s.Users.Any(u => u.UserId == userId)).ToList();
    }

    public async Task<Student> GetByIdAsync(string id) =>
        await _repo.GetAsync<Student>(Col, id) ?? throw new KeyNotFoundException("Student not found.");

    public async Task<Student?> GetByStudentIdAsync(string studentId)
    {
        var all = await _repo.GetAllAsync<Student>(Col);
        return all.FirstOrDefault(s => s.StudentId == studentId);
    }

    public async Task<Student> CreateAsync(CreateStudentRequest req, string userId, string userName, string userEmail, string userPhone, string userLocation, string role)
    {
        ValidateSchedules(req.Subjects);

        // Build the parent list BEFORE constructing the Student object
        // so it is assigned in the initializer — not mutated after construction
        var parentUsers = new List<StudentUser>();

        if (role is "admin" or "superadmin")
        {
            if (req.Users is { Count: > 0 })
                parentUsers = await ResolveParents(req.Users);
        }
        else
        {
            // Parent (user role) — must supply ParentRelation
            var relation = req.ParentRelation?.Trim();
            if (string.IsNullOrEmpty(relation))
                throw new ArgumentException("ParentRelation is required when a parent creates a student.");

            // Fetch the full user record to guarantee up-to-date info
            var parentUser = await _repo.GetAsync<User>("users", userId)
                ?? throw new KeyNotFoundException("Parent user not found.");

            parentUsers.Add(new StudentUser
            {
                UserId = parentUser.Id,
                Name = parentUser.Name,
                Email = parentUser.Email,
                Phone = parentUser.Phone,
                Location = parentUser.Location,
                Relation = relation
            });
        }

        var student = new Student
        {
            Id = Guid.NewGuid().ToString(),
            StudentId = GenerateStudentId(),
            Name = req.Name,
            Grade = req.Grade,
            Section = req.Section,
            Users = parentUsers,   // assigned in initializer — not mutated afterwards
            Subjects = req.Subjects.Select(s => new Subject
            {
                Id = Guid.NewGuid().ToString(),
                Name = s.Name,
                Schedules = s.Schedules.Select(sc => new ClassSchedule { Day = sc.Day, StartTime = sc.StartTime, EndTime = sc.EndTime }).ToList()
            }).ToList()
        };

        await _repo.SetAsync(Col, student.Id, student);
        return student;
    }

    public async Task UpdateAsync(string id, UpdateStudentRequest req, string role)
    {
        var student = await GetByIdAsync(id);
        ValidateSchedules(req.Subjects);

        student.Name = req.Name; student.Grade = req.Grade; student.Section = req.Section;
        student.Subjects = req.Subjects.Select(s => new Subject
        {
            Id = Guid.NewGuid().ToString(),
            Name = s.Name,
            Schedules = s.Schedules.Select(sc => new ClassSchedule { Day = sc.Day, StartTime = sc.StartTime, EndTime = sc.EndTime }).ToList()
        }).ToList();

        // Admin/superadmin may replace the parent list entirely
        if (role is "admin" or "superadmin" && req.Users is { Count: > 0 })
            student.Users = await ResolveParents(req.Users);

        // User role: parent list is managed via Link endpoint — not touched here

        await _repo.SetAsync(Col, student.Id, student);
    }

    public async Task DeleteAsync(string id)
    {
        await GetByIdAsync(id);
        await _repo.DeleteAsync(Col, id);
    }

    public async Task LinkAsync(string id, string userId, string userName, string userEmail, string userPhone, string userLocation, string relation)
    {
        var student = await GetByIdAsync(id);
        if (student.Users.Any(u => u.UserId == userId)) throw new InvalidOperationException("User already linked.");
        if (student.Users.Any(u => u.Relation == relation)) throw new InvalidOperationException($"Relation '{relation}' already taken.");
        if (student.Users.Count >= 3) throw new InvalidOperationException("Maximum 3 parents allowed.");

        student.Users.Add(new StudentUser { UserId = userId, Name = userName, Email = userEmail, Phone = userPhone, Location = userLocation, Relation = relation });
        await _repo.SetAsync(Col, student.Id, student);
    }

    private static string GenerateStudentId()
    {
        var rng = new Random();
        return string.Concat(Enumerable.Range(0, 13).Select(_ => rng.Next(0, 10)));
    }

    private static void ValidateSchedules(List<SubjectDto> subjects)
    {
        var allSchedules = subjects.SelectMany(s => s.Schedules).ToList();
        for (int i = 0; i < allSchedules.Count; i++)
            for (int j = i + 1; j < allSchedules.Count; j++)
            {
                var a = allSchedules[i]; var b = allSchedules[j];
                if (a.Day == b.Day && TimeSpan.Parse(a.StartTime) < TimeSpan.Parse(b.EndTime) && TimeSpan.Parse(b.StartTime) < TimeSpan.Parse(a.EndTime))
                    throw new ArgumentException($"Schedule overlap detected on {a.Day}.");
            }
    }

    private async Task<List<StudentUser>> ResolveParents(List<ParentEntryDto> parents)
    {
        var result = new List<StudentUser>();
        var allUsers = await _repo.GetAllAsync<User>("users");

        foreach (var p in parents)
        {
            var existing = allUsers.FirstOrDefault(u =>
                   u.Email.Equals(p.Email, StringComparison.OrdinalIgnoreCase) || u.Phone == p.Phone);

            if (existing == null)
            {
                existing = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Location = p.Location,
                    Role = "user",
                    Status = "active",
                    EmailVerified = true,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234")
                };
                await _repo.SetAsync("users", existing.Id, existing);
                allUsers.Add(existing);
            }

            result.Add(new StudentUser { UserId = existing.Id, Name = existing.Name, Email = existing.Email, Phone = existing.Phone, Location = existing.Location, Relation = p.Relation });
        }
        return result;
    }
}
