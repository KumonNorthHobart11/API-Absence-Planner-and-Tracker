using System.Security.Cryptography;
using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.Interfaces;
using AbsencePlanner.Core.Models;
using Microsoft.Extensions.Options;

namespace AbsencePlanner.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IFirestoreRepository _repo;
    private readonly IOptionsMonitor<OtpSettings> _settings;
    private const string Col = "otp_records";

    public OtpService(IFirestoreRepository repo, IOptionsMonitor<OtpSettings> settings)
    {
        _repo = repo;
        _settings = settings;
    }

    public async Task<string> GenerateAndStoreAsync(string userId, string purpose)
    {
        // Clean up any previous OTPs for this user+purpose before creating a new one
        await CleanupAllAsync(userId, purpose);

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        var record = new OtpRecord
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Code = code,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_settings.CurrentValue.ExpiryMinutes),
            Used = false
        };

        await _repo.SetAsync(Col, record.Id, record);
        return code;
    }

    public async Task<bool> ValidateAsync(string userId, string code, string purpose)
    {
        var records = await _repo.GetAllAsync<OtpRecord>(Col);
        var userRecords = records.Where(r => r.UserId == userId && r.Purpose == purpose).ToList();

        // Delete all expired records for this user+purpose
        var expiredRecords = userRecords.Where(r => r.ExpiresAt <= DateTime.UtcNow).ToList();
        foreach (var expired in expiredRecords)
            await _repo.DeleteAsync(Col, expired.Id);

        // Find a valid matching record
        var match = userRecords
            .Where(r => r.Code == code && !r.Used && r.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();

        if (match == null) return false;

        // Delete the used OTP immediately from the database
        await _repo.DeleteAsync(Col, match.Id);
        return true;
    }

    public async Task CleanupExpiredAsync(string userId)
    {
        var records = await _repo.GetAllAsync<OtpRecord>(Col);
        var expired = records.Where(r => r.UserId == userId && r.ExpiresAt <= DateTime.UtcNow).ToList();
        foreach (var record in expired)
            await _repo.DeleteAsync(Col, record.Id);
    }

    private async Task CleanupAllAsync(string userId, string purpose)
    {
        var records = await _repo.GetAllAsync<OtpRecord>(Col);
        var old = records.Where(r => r.UserId == userId && r.Purpose == purpose).ToList();
        foreach (var record in old)
            await _repo.DeleteAsync(Col, record.Id);
    }
}
