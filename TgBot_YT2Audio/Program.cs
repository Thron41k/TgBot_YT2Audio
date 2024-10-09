using System.Net.Mime;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;
using File = System.IO.File;


var rooDir = "D:\\TGBotRoot";
var tokenData = TokenFileLoad();
if (string.IsNullOrEmpty(tokenData?.TgToken))
{
    Console.WriteLine("Не обнаружен токен для авторизации бота.");
    Console.WriteLine("Создайте в корне файл \"token.json\" с токеном или передайте в качестве первого аргумента путь к файлу с токеном.");
    Console.WriteLine("Файл \"token.json\" должен иметь следующую структуру:");
    Console.WriteLine(Environment.NewLine);
    Console.WriteLine("{");
    Console.WriteLine("\t\"TgToken\": \"ТОКЕНА_ВАШЕГО_ТЕЛЕГРАМ_БОТА\"");
    Console.WriteLine("\t\"GoogleApiClientId\": \"Google Api Client ID\"");
    Console.WriteLine("\t\"GoogleApiClientSecret\": \"Google Api Client Secret\"");
    Console.WriteLine("}");
    return;
}

using var cts = new CancellationTokenSource();
var opt = new TelegramBotClientOptions(tokenData.TgToken,baseUrl: "http://localhost:8081");
var bot = new TelegramBotClient(cancellationToken: cts.Token,options: opt);
Console.WriteLine($"local server {bot.LocalBotServer}");
bot.Timeout = new TimeSpan(0,1,0,0);
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
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        await bot.AnswerCallbackQueryAsync(query.Id);
        if (!taskManager.UpdateTask(query))
        {
            await bot.EditMessageTextAsync(query.Message!.Chat, query.Message!.MessageId, "Что то пошло не так( попробуйте ещё раз.");
        }
        //await bot.DeleteMessageAsync(query.Message!.Chat, query.Message.MessageId);
        //await bot.AnswerCallbackQueryAsync(query.Id, $"You picked {query.Data}");
        //await bot.SendTextMessageAsync(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
    }
};
Console.WriteLine($"@{user.Username} запущен... Нажмите любую клавишу для выхода.");
Console.ReadLine();
cts.Cancel();
return;

TokenFileReader.TokenFile? TokenFileLoad()
{
    if (args.Length > 0 && File.Exists(args[0]))
    {
        return TokenFileReader.Read(args[0]);
    }
    return File.Exists("token.json") ? TokenFileReader.Read("token.json") : null;
}

bool YouTubeUrlValidate(string url)
{
    const string pattern = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
    return Regex.IsMatch(url, pattern);
}