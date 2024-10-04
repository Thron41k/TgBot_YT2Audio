using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(string url, long id, Message message, TelegramBotClient bot)
{
    private readonly string? _url = url;
    private Message _message = message;
    private TaskStates _taskState = TaskStates.Created;
    private TaskTypes _taskType = TaskTypes.None;
    private readonly YoutubeDL _ytdl = new()
    {
        YoutubeDLPath = "yt-dlp\\yt-dlp.exe"
    };

    public bool Check(int messageId, long userId)
    {
        return _message.MessageId == messageId && id == userId;
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
                case TaskTypes.None:
                    break;
                case TaskTypes.Video:
                    try
                    {
                        var res = await _ytdl.RunVideoDataFetch(_url);
                        var video = res.Data;
                        Console.WriteLine(video.Format);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
                case TaskTypes.Audio:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await bot.EditMessageTextAsync(_message.Chat.Id, _message.MessageId, "Загрузка началась...");
        }
    }
}
