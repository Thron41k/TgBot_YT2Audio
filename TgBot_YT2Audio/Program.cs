using System.Text.RegularExpressions;
using Telegram.Bot;
using TgBot_YT2Audio;
using VideoLibrary;
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

var source = @"/root/";
var youtube = YouTube.Default;
var vid = youtube.GetVideo("https://youtu.be/adRoizGP6vw?si=fawaFoSSjQ05FIKu");
File.WriteAllBytes(source + vid.FullName, vid.GetBytes());
//var inputFile = new MediaFile { Filename = source + vid.FullName };
//var outputFile = new MediaFile { Filename = $"{source + vid.FullName}.mp3" };

//using (var engine = new Engine())
//{
//    engine.GetMetadata(inputFile);

//    engine.Convert(inputFile, outputFile);
//}
using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var user = await bot.GetMeAsync();
bot.OnMessage += async (message, type) =>
{
    try
    {
        if (message.Text is null) return;
        Console.WriteLine($"Received {type} '{message.Text}' in {message.Chat}");
        await bot.SendTextMessageAsync(message.Chat, $"{message.From} said: {message.Text}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
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

