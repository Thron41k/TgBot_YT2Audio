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
        //private async Task TaskFormatChooseCompleteAudio(string? mes)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(mes))
        //        {
        //            await Bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Собираю информацию о аудио...");
        //            var resultFileInfo = await YtDl.RunVideoDataFetch(_initMessage.Text);
        //            Title = resultFileInfo.Data.Title;
        //            await Bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Начинаю скачивание...");
        //            TaskState = TaskStatesEnum.Downloading;
        //            var opt = new OptionSet
        //            {
        //                Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
        //            };
        //            var res = await YtDl.RunAudioDownload(
        //                _initMessage.Text, Helpers.GetAudioFormat(mes), progress: new Progress<DownloadProgress>(ChangeDownloadProgress), overrideOptions: opt
        //            );
        //            await Bot.EditMessageTextAsync(_message.Chat.Id, _message.MessageId, "Начинаю загрузку...");
        //            await using Stream stream = WebRequestMethods.File.OpenRead(res.Data);
        //            await Bot.SendAudioAsync(_message.Chat.Id, stream, caption: Title, title: Title);
        //            await Bot.DeleteMessageAsync(_initMessage.Chat.Id, _initMessage.MessageId);
        //            await Bot.DeleteMessageAsync(_message.Chat.Id, _message.MessageId);
        //            WebRequestMethods.File.Delete(res.Data);
        //            Complete?.Invoke();
        //            return;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //    await ErrorNotification();
        //}
    }
}
