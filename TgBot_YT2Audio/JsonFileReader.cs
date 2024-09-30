using System.Text.Json;

namespace TgBot_YT2Audio;

public static class JsonFileReader
{
    public static async Task<T?> ReadAsync<T>(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }

    public class TokenFile
    {
        public string? Token { get; set; }
    }
}