using AbsencePlanner.Core.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Grpc.Auth;
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
    }

    public FirestoreDb GetDatabase()
    {
        var cfg = _settings.CurrentValue;
        var dbId = string.IsNullOrEmpty(cfg.DatabaseId) || cfg.DatabaseId == "default"
            ? "(default)"
            : cfg.DatabaseId;

        lock (_lock)
        {
            if (_db != null && cfg.ProjectId == _lastProjectId && dbId == _lastDatabaseId)
                return _db;

            _db = BuildFirestoreDb(cfg, dbId);
            _lastProjectId = cfg.ProjectId;
            _lastDatabaseId = dbId;
        }

        return _db;
    }

    private static FirestoreDb BuildFirestoreDb(FirebaseSettings cfg, string dbId)
    {
        var builder = new FirestoreDbBuilder
        {
            ProjectId = cfg.ProjectId,
            DatabaseId = dbId
        };

        // Production: credential JSON is injected via environment variable Firebase__CredentialJson
        if (!string.IsNullOrWhiteSpace(cfg.CredentialJson))
        {
            var credential = GoogleCredential
                .FromJson(cfg.CredentialJson)
                .CreateScoped("https://www.googleapis.com/auth/datastore");

            builder.ChannelCredentials = credential.ToChannelCredentials();
        }
        // Local dev: credential is read from a file path
        else if (!string.IsNullOrWhiteSpace(cfg.CredentialPath))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", cfg.CredentialPath);
        }
        else
        {
            throw new InvalidOperationException(
                "Firebase credentials are not configured. " +
                "Set Firebase__CredentialJson (production) or Firebase:CredentialPath (development).");
        }

        return builder.Build();
    }
}
