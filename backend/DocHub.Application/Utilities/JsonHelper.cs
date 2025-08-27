using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocHub.Application.Utilities
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static T? DeserializeJson<T>(string? json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<T>(json, Options);
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeJson<T>(T value) where T : class
        {
            if (value == null)
                return "{}";

            try
            {
                return JsonSerializer.Serialize(value, Options);
            }
            catch
            {
                return "{}";
            }
        }
    }
}
