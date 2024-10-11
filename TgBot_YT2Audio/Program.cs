using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;

using var cts = new CancellationTokenSource();
var tgOptions = new TelegramBotClientOptions(Configuration.GetInstance().BotApiToken!, baseUrl: Configuration.GetInstance().LocalApiServer);
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
            if (!taskManager.UpdateTask(query))
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
Console.WriteLine($"@{user.Username} запущен... Нажмите любую клавишу для выхода.");
Console.ReadLine();
cts.Cancel();

