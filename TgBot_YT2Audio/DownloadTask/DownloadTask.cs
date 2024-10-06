using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(string url, long id, Message message, TelegramBotClient bot)
{
    private TaskStates _taskState = TaskStates.Created;
    private TaskTypes _taskType = TaskTypes.None;
    private List<FormatData> _formats = new List<FormatData>();
    private bool _fail = false;
    private readonly YoutubeDL _ytdl = new(10)
    {
        YoutubeDLPath = "yt-dlp\\yt-dlp.exe"
    };
    public bool Fail => _fail;
    public bool Check(int messageId, long userId)
    {
        return message.MessageId == messageId && id == userId;
    }

    public async Task Update(CallbackQuery query)
    {
        switch (_taskState)
        {
            case TaskStates.Created:
                await TaskTypeChooseComplete(query.Data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private InlineKeyboardMarkup GetKeyboard(IEnumerable<string> buttons)
    {
        var keyboard = new InlineKeyboardMarkup();
        var enumerable = buttons as string[] ?? buttons.ToArray();
        for (var i = 0; i < enumerable.Count(); i++)
        {
            if (i % 2 == 0 && i != 0)
            {
                keyboard.AddNewRow();
            }
            keyboard.AddButton(enumerable[i], callbackData: enumerable[i]);
        }
        return keyboard;
    }
    private async Task TaskTypeChooseComplete(string? mes)
    {
        switch (mes)
        {
            case "Видео":
                _taskType= TaskTypes.Video;
                _taskState = TaskStates.TypeSelected;
                break;
            case "Аудио":
                _taskType= TaskTypes.Audio;
                _taskState = TaskStates.TypeSelected;
                break;
        }

        if (_taskState == TaskStates.TypeSelected)
        {
            switch (_taskType)
            {
                case TaskTypes.Video:
                    try
                    {
                        var res = await _ytdl.RunVideoDataFetch(url);
                        var result = Helpers.GetFormatList(res.Data.Formats);
                        _formats = result.FormatList;
                        await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите качество",
                            replyMarkup: GetKeyboard(result.FormatNames));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
                case TaskTypes.Audio:
                    break;
                case TaskTypes.None:
                default:
                    _fail = true;
                    await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
                    break;
            }
            
        }
    }
}
