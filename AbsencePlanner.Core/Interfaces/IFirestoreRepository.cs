using Google.Cloud.Firestore;

namespace AbsencePlanner.Core.Interfaces;

public interface IFirestoreRepository
{
    Task<T?> GetAsync<T>(string collection, string documentId) where T : class;
    Task<List<T>> GetAllAsync<T>(string collection) where T : class;
    Task<List<T>> WhereEqualToAsync<T>(string collection, string field, object value) where T : class;
    Task SetAsync<T>(string collection, string documentId, T data) where T : class;
Task UpdateFieldsAsync(string collection, string documentId, Dictionary<string, object?> updates);
    Task DeleteAsync(string collection, string documentId);
    CollectionReference Collection(string name);
}
