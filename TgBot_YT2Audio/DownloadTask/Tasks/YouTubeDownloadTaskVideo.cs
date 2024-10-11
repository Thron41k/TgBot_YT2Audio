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
            TaskQualityChooseCompleteVar = TaskQualityChooseComplete;
            TaskFormatChooseCompleteVar = TaskFormatChooseComplete;
            _ = TaskTypeChooseComplete();
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
                    await Bot.EditMessageTextAsync(Message!.Chat, Message.MessageId, "Выберите формат",
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
                    await Bot.EditMessageTextAsync(Message!.Chat.Id, Message.MessageId, "Начинаю скачивание...");
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
                            InitMessage.Text, progress: new Progress<DownloadProgress>(ChangeDownloadProgress),
                            overrideOptions: opt
                        );
                        await Bot.EditMessageTextAsync(Message.Chat.Id, Message.MessageId, "Начинаю загрузку...");
                        await using Stream stream = File.OpenRead(res.Data);
                        await Bot.SendVideoAsync(Message.Chat.Id, stream, caption: Title,
                            supportsStreaming: true);
                        await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId);
                        await Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId);
                        File.Delete(res.Data);
                        OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
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

        private async Task TaskTypeChooseComplete()
        {
            try
            {
                TaskState = TaskStatesEnum.TypeSelected;
                Message = await Bot.SendTextMessageAsync(InitMessage.Chat, "Собираю информацию о видео...");
                var res = await YtDl.RunVideoDataFetch(InitMessage.Text);
                var result = Helpers.GetFormatList(res.Data.Formats.ToList());
                _formats = result.FormatList;
                Title = res.Data.Title;
                await Bot.EditMessageTextAsync(Message.Chat, Message.MessageId, "Выберите качество", replyMarkup: Helpers.GetKeyboard(result.FormatNames));
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ErrorNotification();
        }
    }
}
