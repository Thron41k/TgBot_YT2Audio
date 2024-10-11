using System.Net;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeDownloadTaskVideo : YouTubeTaskBase
    {
        private List<FormatData> _formats = [];
        private string? _quality = "";
        public YouTubeDownloadTaskVideo(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
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
                    await Bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Выберите формат",
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
                    await Bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var format = _formats.Where(x => x.Extension == mes).MaxBy(x => x.Bitrate);
                    if (format != null)
                    {
                        var opt = new OptionSet
                        {
                            Format = format.FormatId,
                            Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "video", "video_%(id)s_%(format_note)s.%(ext)s")
                        };
                        var res = await YtDl.RunVideoDownload(
                            _initMessage.Text, progress: new Progress<DownloadProgress>(ChangeDownloadProgress),
                            overrideOptions: opt
                        );
                        await Bot.EditMessageTextAsync(_message.Chat.Id, _message.MessageId, "Начинаю загрузку...");
                        await using Stream stream = File.OpenRead(res.Data);
                        await Bot.SendVideoAsync(_message.Chat.Id, stream, caption: Title,
                            supportsStreaming: true);
                        await Bot.DeleteMessageAsync(_initMessage.Chat.Id, _initMessage.MessageId);
                        await Bot.DeleteMessageAsync(_message.Chat.Id, _message.MessageId);
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
                    await Bot.EditMessageTextAsync(_message!.Chat, _message.MessageId, "Собираю информацию о видео...");
                    var reVs = await YtDl.RunVideoDataFetch(_initMessage.Text);
                    var result = Helpers.GetFormatList(reVs.Data.Formats.ToList());
                    _formats = result.FormatList;
                    Title = reVs.Data.Title;
                    await Bot.EditMessageTextAsync(_message.Chat, _message.MessageId, "Выберите качество", replyMarkup: Helpers.GetKeyboard(result.FormatNames));
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
