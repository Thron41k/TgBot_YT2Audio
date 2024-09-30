using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio;



using var cts = new CancellationTokenSource();
var token = (await JsonFileReader.ReadAsync<JsonFileReader.TokenFile>("token.json"))?.Token;
if (token != null)
{
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
}
