using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadMusic : YouTubeTaskBase
    {
        public YouTubeTaskDownloadMusic(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskType = TaskTypesEnum.YouTubeTaskDownloadMusic;
        }
        public YouTubeTaskDownloadMusic(Message initMessage, TelegramBotClient bot,string url = "") : base(initMessage, bot, url)
        {
            TaskType = TaskTypesEnum.YouTubeTaskDownloadMusic;
            var messageId = InitMessage.MessageId;
            InitMessage = SendMessageText(InitMessage.Chat.Id, "В очереди...").Result;
            Bot.DeleteMessageAsync(InitMessage.Chat.Id, messageId, cancellationToken: CTokenSource!.Token);
        }
        public override async Task Start()
        {
            await TaskFormatChooseComplete("");
        }

        protected override Task TaskTypeChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override Task TaskQualityChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override async Task TaskFormatChooseComplete(string? mes)
        {
            try
            {

                await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Собираю информацию о аудио...");
                var resultFileInfo = await YtDl.RunVideoDataFetch(InitMessage.Text);
                Title = resultFileInfo.Data.Title;
                Id = resultFileInfo.Data.ID;
                await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Начинаю скачивание...");
                TaskState = TaskStatesEnum.Downloading;
                var opt = new OptionSet
                {
                    Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "audio", $"{Guid}_%(id)s_%(format_note)s.%(ext)s")
                };
                var res = await YtDl.RunAudioDownload(
                    InitMessage.Text, AudioConversionFormat.Mp3, progress: new Progress<DownloadProgress>(ChangeDownloadProgress), overrideOptions: opt, ct: CTokenSource!.Token
                );
                await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Начинаю загрузку...");
                await using Stream stream = File.OpenRead(res.Data);
                await Bot.SendAudioAsync(InitMessage.Chat.Id, stream, caption: Title, title: Title, cancellationToken: CTokenSource!.Token);
                await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId, cancellationToken: CTokenSource!.Token);
                File.Delete(res.Data);
                OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
                return;
            }
            catch (TaskCanceledException)
            {
                Helpers.DeleteUncompletedFile("audio", Id, Guid);
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
