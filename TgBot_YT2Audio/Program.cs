﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio;
using TgBot_YT2Audio.DownloadTask;

using var cts = new CancellationTokenSource();
var tgOptions = new TelegramBotClientOptions(Configuration.GetInstance().BotApiToken!, baseUrl: Configuration.GetInstance().LocalApiServer);
var bot = new TelegramBotClient(cancellationToken: cts.Token, options: tgOptions);
Console.WriteLine($"Local server {bot.LocalBotServer}");
bot.Timeout = new TimeSpan(0, 1, 0, 0);
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
            Message mes;
            var isMusic = Helpers.IsMusic(message.Text);
            if (!isMusic)
                mes = await bot.SendTextMessageAsync(message.Chat, "Что вы хотите скачать?", replyMarkup: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио"));
            else
                mes = await bot.SendTextMessageAsync(message.Chat, "Готовлюсь к скачиванию аудио...");
            taskManager.AddTask(message, mes, bot,true);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
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