using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;


using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(cancellationToken: cts.Token,options: new TelegramBotClientOptions(Configuration.GetInstance().BotApiToken!, baseUrl: Configuration.GetInstance().LocalApiServer));
Console.WriteLine($"Local server {bot.LocalBotServer}");
bot.Timeout = new TimeSpan(0,1,0,0);
var user = await bot.GetMeAsync();
var taskManager = new TaskManager();
bot.OnMessage += async (message, type) =>
{
    try
    {
        if (type != UpdateType.Message) return;
        if (message.Text is null) return;
        if (Helpers.YouTubeUrlValidate(message.Text))
        {
            var mes = await bot.SendTextMessageAsync(message.Chat, "Что вы хотите скачать?",
                replyMarkup: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио"));
            taskManager.AddTask(message.Text, message.From!.Id, mes, bot);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
};
bot.OnUpdate += async update =>
{
    if (update is { CallbackQuery: { } query })
    {
        await bot.AnswerCallbackQueryAsync(query.Id);
        if (!taskManager.UpdateTask(query))
        {
            await bot.EditMessageTextAsync(query.Message!.Chat, query.Message!.MessageId, "Что то пошло не так( попробуйте ещё раз.");
        }
    }
};
Console.WriteLine($"@{user.Username} запущен... Нажмите любую клавишу для выхода.");
Console.ReadLine();
cts.Cancel();