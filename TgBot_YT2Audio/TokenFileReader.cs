using System.Text.Json;

namespace TgBot_YT2Audio;

public static class TokenFileReader
{
    public static TokenFile? Tokens { get; set; }
    public static TokenFile? Read(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            Tokens = JsonSerializer.Deserialize<TokenFile>(stream);
            return Tokens;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public class TokenFile
    {
        public string? TgToken { get; set; }
    }
}