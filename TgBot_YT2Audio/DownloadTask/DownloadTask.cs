using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(string url, Message message, TelegramBotClient bot)
{
    private readonly string? _url = url;
    private Message _message = message;
    private readonly TelegramBotClient _bot = bot;
    private TaskStates _taskState = TaskStates.Created;
    private TaskTypes _taskType = TaskTypes.None;

    public bool Check(int messageId, long userId)
    {
        return _message.From != null && _message.MessageId == messageId && _message.From.Id == userId;
    }

    public void Update(CallbackQuery query)
    {
        switch (_taskState)
        {
            case TaskStates.Created:
                TaskTypeChooseComplete(query.Data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private  void TaskTypeChooseComplete(string? mes)
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
            _bot.Ed
        }
    }
}
