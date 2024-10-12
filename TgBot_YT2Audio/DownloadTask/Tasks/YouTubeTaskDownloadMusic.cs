using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadMusic(Message initMessage, TelegramBotClient bot)
        : YouTubeTaskBase(initMessage, bot)
    {
        public override async Task Start()
        {
            await TaskTypeChooseComplete("");
        }

        protected override async Task TaskTypeChooseComplete(string? mes)
        {
            try
            {
                TaskState = TaskStatesEnum.FormatSelected;
                Message = await SendMessageText(InitMessage.Chat.Id, "Выберите формат", keyboard: Helpers.GetKeyboard(Helpers.AudioFormats));
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

        protected override Task TaskQualityChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override async Task TaskFormatChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    await EditMessageText(Message!.Chat.Id, Message.MessageId, "Собираю информацию о аудио...");
                    var resultFileInfo = await YtDl.RunVideoDataFetch(InitMessage.Text);
                    Title = resultFileInfo.Data.Title;
                    await EditMessageText(Message!.Chat.Id, Message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var opt = new OptionSet
                    {
                        Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
                    };
                    var res = await YtDl.RunAudioDownload(
                        InitMessage.Text, Helpers.GetAudioFormat(mes), progress: new Progress<DownloadProgress>(ChangeDownloadProgress), overrideOptions: opt, ct: CTokenSource!.Token
                    );
                    await EditMessageText(Message.Chat.Id, Message.MessageId, "Начинаю загрузку...");
                    await using Stream stream = File.OpenRead(res.Data);
                    await Bot.SendAudioAsync(Message.Chat.Id, stream, caption: Title, title: Title, cancellationToken: CTokenSource!.Token);
                    await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId, cancellationToken: CTokenSource!.Token);
                    await Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId, cancellationToken: CTokenSource!.Token);
                    File.Delete(res.Data);
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
                    return;
                }
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
