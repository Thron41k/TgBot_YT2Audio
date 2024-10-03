using System.Net.Mime;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;
using File = System.IO.File;


var token = TokenFileLoad();
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine("Не обнаружен токен для авторизации бота.");
    Console.WriteLine("Создайте в корне файл \"token.json\" с токеном или передайте в качестве первого аргумента путь к файлу с токеном.");
    Console.WriteLine("Файл \"token.json\" должен иметь следующую структуру:");
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("{");
    Console.WriteLine("\t\"Token\": \"ВАШ_ТОКЕН\"");
    Console.WriteLine("}");
    return;
}

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var user = await bot.GetMeAsync();
var taskManager = new TaskManager();
bot.OnMessage += async (message, type) =>
{
    try
    {
        if (type != UpdateType.Message) return;
        if (message.Text is null) return;
        if (YouTubeUrlValidate(message.Text))
        {
            var mes = await bot.SendTextMessageAsync(message.Chat, "Что вы хотите скачать?",
                replyMarkup: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио"));
            taskManager.AddTask(message.Text, mes, bot);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
};
bot.OnUpdate += async update =>
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        await bot.DeleteMessageAsync(query.Message!.Chat, query.Message.MessageId);
        await bot.AnswerCallbackQueryAsync(query.Id, $"You picked {query.Data}");
        await bot.SendTextMessageAsync(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
    }
};
Console.WriteLine($"@{user.Username} запущен... Нажмите любую клавишу для выхода.");
Console.ReadLine();
cts.Cancel();
return;

string? TokenFileLoad()
{
    if (args.Length > 0 && File.Exists(args[0]))
    {
        return TokenFileReader.Read(args[0]);
    }
    return File.Exists("token.json") ? TokenFileReader.Read("token.json") : null;
}

bool YouTubeUrlValidate(string url)
{
    var pattern = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
    return Regex.IsMatch(url, pattern);
}

