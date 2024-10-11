using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp.Options;
using YoutubeDLSharp;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeDownloadTaskMusic : YouTubeTaskBase
    {
        public YouTubeDownloadTaskMusic(Message initMessage, TelegramBotClient bot) : base(
            initMessage, bot)
        {

        }
        private async Task TaskFormatChooseCompleteAudio(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    await _bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Собираю информацию о аудио...");
                    var resultFileInfo = await _ytDl.RunVideoDataFetch(_initMessage.Text);
                    _title = resultFileInfo.Data.Title;
                    await _bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var opt = new OptionSet
                    {
                        Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
                    };
                    var res = await _ytDl.RunAudioDownload(
                        _initMessage.Text, Helpers.GetAudioFormat(mes), progress: new Progress<DownloadProgress>(ChangeDownloadProgress), overrideOptions: opt
                    );
                    await _bot.EditMessageTextAsync(_message.Chat.Id, _message.MessageId, "Начинаю загрузку...");
                    await using Stream stream = WebRequestMethods.File.OpenRead(res.Data);
                    await _bot.SendAudioAsync(_message.Chat.Id, stream, caption: _title, title: _title);
                    await _bot.DeleteMessageAsync(_initMessage.Chat.Id, _initMessage.MessageId);
                    await _bot.DeleteMessageAsync(_message.Chat.Id, _message.MessageId);
                    WebRequestMethods.File.Delete(res.Data);
                    Complete?.Invoke();
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ErrorNotification();
        }
    }
}
