namespace TgBot_YT2Audio;

public class FileCleaner
{
    private static readonly FileCleaner Instance = new();

    public static FileCleaner GetInstance()
    {
        return Instance;
    }

    public void Start()
    {
        var timer = new System.Timers.Timer(3600000);
        timer.Elapsed += (_, _) =>
        {
            Console.WriteLine("Clened files");
            try
            {
                Clean();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        };
        timer.Start();
    }

    private static void Clean()
    {
        foreach (var type in new[] { "audio", "video" })
        {
            var path = Path.Combine(Configuration.GetInstance().OutputFolder!, type);
            if (!Directory.Exists(path)) continue;
            foreach (var file in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < DateTime.Now.AddDays(-1))
                {
                    fileInfo.Delete();
                }
            }
        }
    }
}
