using Newtonsoft.Json;

namespace WinUI3Demo.Core.Contracts.Services;

public interface ILocalSettingsService
{
    T? ReadSetting<T>(string key);

    T? ReadSetting<T>(string key, T defaultValue);

    Task<T?> ReadSettingAsync<T>(string key);

    Task<T?> ReadSettingAsync<T>(string key, T defaultValue);

    Task SaveSettingAsync<T>(string key, T value);

    T? ReadJsonFile<T>(string fileName, JsonSerializerSettings? jsonSerializerSettings = null);

    Task<T?> ReadJsonFileAsync<T>(string fileName, JsonSerializerSettings? jsonSerializerSettings = null);

    Task SaveJsonFileAsync<T>(string fileName, T value);
}
