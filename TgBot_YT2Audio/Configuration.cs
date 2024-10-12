using System.Text.Json;
using System.Text.Json.Serialization;

namespace TgBot_YT2Audio;

public class Configuration
{
    private static readonly Configuration Instance = Load();

    public string? OutputFolder { get; }
    public string? YoutubeDlPath { get; }
    public string? BotApiToken { get; }
    public string? LocalApiServer { get; }
    public int? MaxTaskCount { get; }

    [JsonConstructor]
    private Configuration(string? outputFolder, string? youtubeDlPath, string? botApiToken, string? localApiServer, int? maxTaskCount)
    {
        OutputFolder = outputFolder;
        YoutubeDlPath = youtubeDlPath;
        BotApiToken = botApiToken;
        LocalApiServer = localApiServer;
        MaxTaskCount = maxTaskCount;
    }

    private Configuration()
    {

    }

    private bool IsValid()
    {
        if (string.IsNullOrEmpty(OutputFolder)) return false;
        if (string.IsNullOrEmpty(YoutubeDlPath)) return false;
        return !string.IsNullOrEmpty(BotApiToken);
    }
    public static Configuration GetInstance()
    {
        return Instance;
    }

    private void Save()
    {
        using var fs = new FileStream("config.json", FileMode.Create);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        JsonSerializer.Serialize(fs, this, jsonOptions);
    }

    private static Configuration Load()
    {
        try
        {
            if (!File.Exists("config.json"))
            {
                var newConfig = new Configuration();
                newConfig.Save();
            }
            using var fs = new FileStream("config.json", FileMode.Open);
            var config = JsonSerializer.Deserialize<Configuration>(fs);
            if (config != null && config.IsValid()) return config;
            throw new FileLoadException("config.json is invalid");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
