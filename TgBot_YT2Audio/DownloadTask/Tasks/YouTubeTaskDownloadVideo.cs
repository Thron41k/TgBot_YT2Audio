using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadVideo : YouTubeTaskBase
    {

        public YouTubeTaskDownloadVideo(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskType = TaskTypesEnum.YouTubeTaskDownloadVideo;
        }

        public override async Task Start()
        {
            await TaskQualityChooseComplete("");
        }

        protected override Task TaskFormatChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override async Task TaskQualityChooseComplete(string? mes)
        {
            try
            {
                if (Format != null)
                {

                    await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Начинаю скачивание...");
                    var opt = new OptionSet
                    {
                        Format = $"{Format.FormatId}+bestaudio",
                        Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "video", $"{Guid}_%(id)s_%(format_note)s.%(ext)s")
                    };
                    var res = await YtDl.RunVideoDownload(
                        InitMessage.Text, progress: new Progress<DownloadProgress>(ChangeDownloadProgress),
                        overrideOptions: opt,
                        ct: CTokenSource!.Token
                    );
                    await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Начинаю загрузку...");
                    await using Stream stream = File.OpenRead(res.Data);
                    await Bot.SendVideoAsync(InitMessage.Chat.Id, stream, caption: Title,
                        supportsStreaming: true, cancellationToken: CTokenSource!.Token);
                    await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId, cancellationToken: CTokenSource!.Token);
                    File.Delete(res.Data);
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
                    return;
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

        protected override Task TaskTypeChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }
    }
}
