using System.Text.Json;
using System.Text.Json.Serialization;

namespace TgBot_YT2Audio;

public class Configuration
{
    [JsonIgnore] public static string ConfigPath { get; set; } = "config1.json";
    public string? OutputFolder { get; }
    public string? YoutubeDlPath { get; }
    public string? FFmpegPath { get; }
    public string? BotApiToken { get; }
    public string? LocalApiServer { get; }
    public int? MaxTaskCount { get; }
    private static Configuration? _instance;
    [JsonConstructor]
    private Configuration(string? outputFolder, string? youtubeDlPath, string? botApiToken, string? localApiServer, int? maxTaskCount, string? fFmpegPath)
    {
        OutputFolder = outputFolder;
        YoutubeDlPath = youtubeDlPath;
        BotApiToken = botApiToken;
        LocalApiServer = localApiServer;
        MaxTaskCount = maxTaskCount;
        FFmpegPath = fFmpegPath;
    }

    private Configuration()
    {
        
    }

    private bool IsValid()
    {
        if (string.IsNullOrEmpty(OutputFolder)) return false;
        if (string.IsNullOrEmpty(YoutubeDlPath)) return false;
        if (string.IsNullOrEmpty(FFmpegPath)) return false;
        return !string.IsNullOrEmpty(BotApiToken);
    }
    public static Configuration? GetInstance()
    {
        return _instance;
    }

    private void Save()
    {
        using var fs = new FileStream(ConfigPath, FileMode.Create);
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        JsonSerializer.Serialize(fs, this, jsonOptions);
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                var newConfig = new Configuration();
                newConfig.Save();
            }
            using var fs = new FileStream(ConfigPath, FileMode.Open);
            var config = JsonSerializer.Deserialize<Configuration>(fs);
            if (config != null && config.IsValid()) _instance =  config;
            throw new FileLoadException("config is invalid");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
