using AbsencePlanner.Core.Configuration;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Options;

namespace AbsencePlanner.Infrastructure.Repositories;

public class FirestoreDbFactory
{
    private readonly IOptionsMonitor<FirebaseSettings> _settings;
    private FirestoreDb? _db;
    private string? _lastProjectId;
    private string? _lastDatabaseId;
    private readonly object _lock = new();

    public FirestoreDbFactory(IOptionsMonitor<FirebaseSettings> settings)
    {
        _settings = settings;

        // Set credential path on first load
        var cfg = _settings.CurrentValue;
        if (!string.IsNullOrEmpty(cfg.CredentialPath))
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", cfg.CredentialPath);
    }

    public FirestoreDb GetDatabase()
    {
        var cfg = _settings.CurrentValue;
        var dbId = cfg.DatabaseId;
        // Firestore SDK requires "(default)" with parentheses
        if (string.IsNullOrEmpty(dbId) || dbId == "default")
            dbId = "(default)";

        lock (_lock)
        {
            if (_db == null || cfg.ProjectId != _lastProjectId || dbId != _lastDatabaseId)
            {
                if (!string.IsNullOrEmpty(cfg.CredentialPath))
                    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", cfg.CredentialPath);

                _db = new FirestoreDbBuilder
                {
                    ProjectId = cfg.ProjectId,
                    DatabaseId = dbId
                }.Build();

                _lastProjectId = cfg.ProjectId;
                _lastDatabaseId = dbId;
            }
        }

        return _db;
    }
}
