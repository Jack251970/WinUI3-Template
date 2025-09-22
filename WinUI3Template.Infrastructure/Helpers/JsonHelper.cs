using System.Text.Json;

namespace WinUI3Template.Infrastructure.Helpers;

public static class JsonHelper
{
    public static string ConvertToString(object value)
    {
        if (value is not JsonElement jsonElement)
        {
            return value.ToString() ?? string.Empty;
        }

        if (jsonElement.ValueKind == JsonValueKind.String)
        {
            return jsonElement.GetString() ?? string.Empty;
        }
        ;

        return string.Empty;
    }

    public static T? ToObject<T>(string value)
    {
        return JsonSerializer.Deserialize<T>(value);
    }

    public static string Stringify(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        return await Task.Run(() =>
        {
            return JsonSerializer.Deserialize<T>(value);
        });
    }

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run(() =>
        {
            return JsonSerializer.Serialize(value);
        });
    }
}
