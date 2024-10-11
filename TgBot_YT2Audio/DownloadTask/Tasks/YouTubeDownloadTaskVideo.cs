using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeDownloadTaskVideo : YouTubeTaskBase
    {
        public YouTubeDownloadTaskVideo(Message initMessage, TelegramBotClient bot) : base(
            initMessage, bot)
        {

        }
        private async Task TaskQualityChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    TaskState = TaskStatesEnum.FormatSelected;
                    _quality = mes;
                    _formats = _formats.Where(x => x.FormatNote == _quality).ToList();
                    await _bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Выберите формат",
                        replyMarkup: Helpers.GetKeyboard(_formats.Select(x => x.Extension).Distinct()));
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ErrorNotification();
        }

        private async Task TaskFormatChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    await _bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var format = _formats.Where(x => x.Extension == mes).MaxBy(x => x.Bitrate);
                    if (format != null)
                    {
                        var opt = new OptionSet
                        {
                            Format = format.FormatId,
                            Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "video", "video_%(id)s_%(format_note)s.%(ext)s")
                        };
                        var res = await _ytDl.RunVideoDownload(
                            _initMessage.Text, progress: new Progress<DownloadProgress>(ChangeDownloadProgress),
                            overrideOptions: opt
                        );
                        await _bot.EditMessageTextAsync(_message.Chat.Id, _message.MessageId, "Начинаю загрузку...");
                        await using Stream stream = File.OpenRead(res.Data);
                        await _bot.SendVideoAsync(_message.Chat.Id, stream, caption: _title,
                            supportsStreaming: true);
                        await _bot.DeleteMessageAsync(_initMessage.Chat.Id, _initMessage.MessageId);
                        await _bot.DeleteMessageAsync(_message.Chat.Id, _message.MessageId);
                        File.Delete(res.Data);
                        //Complete?.Invoke();
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ErrorNotification();
        }

        private async Task TaskTypeChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    TaskState = TaskStatesEnum.TypeSelected;
                    await _bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Собираю информацию о видео...");
                    var reVs = await _ytDl.RunVideoDataFetch(_initMessage.Text);
                    var result = Helpers.GetFormatList(reVs.Data.Formats.ToList());
                    _formats = result.FormatList;
                    _title = reVs.Data.Title;
                    await _bot.EditMessageTextAsync(_message.Chat, _message.MessageId, "Выберите качество", replyMarkup: Helpers.GetKeyboard(result.FormatNames));
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
