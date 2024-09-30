using System.Text.Json;

namespace TgBot_YT2Audio;

public static class TokenFileReader
{
    public static string? Read(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var token = JsonSerializer.Deserialize<TokenFile>(stream);
            return token?.Token;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public class TokenFile
    {
        public string? Token { get; set; }
    }
}