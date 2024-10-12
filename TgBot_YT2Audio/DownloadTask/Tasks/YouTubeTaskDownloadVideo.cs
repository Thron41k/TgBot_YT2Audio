using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadVideo : YouTubeTaskBase
    {
        private List<FormatData> _formats = [];
        public YouTubeTaskDownloadVideo(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskType = TaskTypesEnum.YouTubeTaskDownloadVideo;
        }

        public override async Task Start()
        {
            await TaskTypeChooseComplete("");
        }

        protected override Task TaskFormatChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override async Task TaskQualityChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    await EditMessageText(Message!.Chat.Id, Message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var format = _formats.Where(x => x.FormatNote == mes && x.Extension == "mp4").MaxBy(x => x.Bitrate);
                    if (format != null)
                    {
                        var opt = new OptionSet
                        {
                            Format = $"{format.FormatId}+bestaudio",
                            Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "video", $"{Guid}_%(id)s_%(format_note)s.%(ext)s")
                        };
                        var res = await YtDl.RunVideoDownload(
                            InitMessage.Text, progress: new Progress<DownloadProgress>(ChangeDownloadProgress),
                            overrideOptions: opt,
                            ct: CTokenSource!.Token
                        );
                        await EditMessageText(Message.Chat.Id, Message.MessageId, "Начинаю загрузку...");
                        await using Stream stream = File.OpenRead(res.Data);
                        await Bot.SendVideoAsync(Message.Chat.Id, stream, caption: Title,
                            supportsStreaming: true, cancellationToken: CTokenSource!.Token);
                        await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId, cancellationToken: CTokenSource!.Token);
                        await Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId, cancellationToken: CTokenSource!.Token);
                        File.Delete(res.Data);
                        OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
                        return;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Helpers.DeleteUncompletedFile("video", Id, Guid);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ErrorNotification();
        }

        protected override async Task TaskTypeChooseComplete(string? mes)
        {
            try
            {
                TaskState = TaskStatesEnum.TypeSelected;
                if (Message == null)
                    Message = await SendMessageText(InitMessage.Chat.Id, "Собираю информацию о видео...");
                else
                    await EditMessageText(Message!.Chat.Id, Message.MessageId, "Собираю информацию о видео...");
                var res = await YtDl.RunVideoDataFetch(InitMessage.Text, ct: CTokenSource!.Token);
                var result = Helpers.GetFormatList(res.Data.Formats.ToList());
                _formats = result.FormatList;
                Id = res.Data.ID;
                Title = res.Data.Title;
                await EditMessageText(Message.Chat.Id, Message.MessageId, "Выберите качество",
                    keyboard: Helpers.GetKeyboard(result.FormatNames));
                return;
            }
            catch (TaskCanceledException)
            {
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
