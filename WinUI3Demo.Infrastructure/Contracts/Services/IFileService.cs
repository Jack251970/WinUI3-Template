using Newtonsoft.Json;

namespace WinUI3Demo.Infrastructure.Contracts.Services;

public interface IFileService
{
    T Read<T>(string folderPath, string fileName, JsonSerializerSettings? jsonSerializerSettings = null);

    Task<T> ReadAsync<T>(string folderPath, string fileName, JsonSerializerSettings? jsonSerializerSettings = null);

    Task<string?> SaveAsync<T>(string folderPath, string fileName, T content, bool indent);

    bool Delete(string folderPath, string fileName);
}
