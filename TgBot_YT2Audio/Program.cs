using CommandLine;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;



var daemon = false;
Parser.Default.ParseArguments<StartArgsOptions>(args)
    .WithParsed(o =>
    {
        if (!string.IsNullOrEmpty(o.Config))
        {
            Configuration.ConfigPath = o.Config;
            Console.WriteLine($"Loading configuration file {o.Config}");
        }
        daemon = o.Daemon;
    });
Configuration.Load();
using var cts = new CancellationTokenSource();
var tgOptions = new TelegramBotClientOptions(Configuration.GetInstance()?.BotApiToken!, baseUrl: Configuration.GetInstance()?.LocalApiServer);
var bot = new TelegramBotClient(cancellationToken: cts.Token, options: tgOptions);
Console.WriteLine($"Local server {bot.LocalBotServer}");
bot.Timeout = new TimeSpan(0, 1, 0, 0);
var user = await bot.GetMeAsync();
var taskManager = new TaskManager();

bot.OnMessage += (message, type) =>
{
    try
    {
        if (type != UpdateType.Message) return Task.CompletedTask;
        if (message.Text is null) return Task.CompletedTask;
        var urlType = Helpers.YouTubeUrlValidate(message.Text);
        taskManager.AddTask(message, bot, urlType);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    return Task.CompletedTask;
};

bot.OnUpdate += async update =>
{
    try
    {
        if (update is { CallbackQuery: { } query })
        {
            await bot.AnswerCallbackQueryAsync(query.Id);
            if (!await taskManager.UpdateTask(query))
            {
                await bot.EditMessageTextAsync(query.Message!.Chat, query.Message!.MessageId, "Что то пошло не так( попробуйте ещё раз.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
};

if (daemon)
{
    Console.WriteLine($"@{user.Username} runed in daemon mode.");
    await Task.Delay(-1);
}
else
{
    Console.WriteLine($"@{user.Username} runed... Press any key for exit.");
    Console.ReadKey();
    cts.Cancel();
}


