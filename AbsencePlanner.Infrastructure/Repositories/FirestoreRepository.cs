using AbsencePlanner.Core.Interfaces;
using Google.Cloud.Firestore;

namespace AbsencePlanner.Infrastructure.Repositories;

public class FirestoreRepository : IFirestoreRepository
{
    private readonly FirestoreDbFactory _factory;

    public FirestoreRepository(FirestoreDbFactory factory) => _factory = factory;

    private FirestoreDb Db => _factory.GetDatabase();

    public async Task<T?> GetAsync<T>(string collection, string documentId) where T : class
    {
        var snap = await Db.Collection(collection).Document(documentId).GetSnapshotAsync();
        return snap.Exists ? snap.ConvertTo<T>() : null;
    }

    public async Task<List<T>> GetAllAsync<T>(string collection) where T : class
    {
        var snap = await Db.Collection(collection).GetSnapshotAsync();
        return snap.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }

    public async Task<List<T>> WhereEqualToAsync<T>(string collection, string field, object value) where T : class
    {
        var snap = await Db.Collection(collection).WhereEqualTo(field, value).GetSnapshotAsync();
        return snap.Documents.Select(d => d.ConvertTo<T>()).ToList();
    }

    public async Task SetAsync<T>(string collection, string documentId, T data) where T : class
    {
        await Db.Collection(collection).Document(documentId).SetAsync(data);
    }

    public async Task UpdateFieldsAsync(string collection, string documentId, Dictionary<string, object?> updates)
    {
        var docRef = Db.Collection(collection).Document(documentId);
        await docRef.UpdateAsync(updates.ToDictionary(k => k.Key, k => k.Value ?? (object)FieldValue.Delete));
    }

    public async Task DeleteAsync(string collection, string documentId)
    {
        await Db.Collection(collection).Document(documentId).DeleteAsync();
    }

    public CollectionReference Collection(string name) => Db.Collection(name);
}
