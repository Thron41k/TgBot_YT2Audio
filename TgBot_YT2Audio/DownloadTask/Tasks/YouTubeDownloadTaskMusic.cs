using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeDownloadTaskMusic : YouTubeTaskBase
    {
        public YouTubeDownloadTaskMusic(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskFormatChooseCompleteVar = TaskFormatChooseComplete;
            _ = TaskTypeChooseComplete();
        }

        private async Task TaskTypeChooseComplete()
        {
            try
            {
                TaskState = TaskStatesEnum.FormatSelected;
                Message = await Bot.SendTextMessageAsync(InitMessage.Chat, "Выберите формат", replyMarkup: Helpers.GetKeyboard(Helpers.AudioFormats));
                return;
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
                    await Bot.EditMessageTextAsync(Message!.Chat, Message.MessageId, "Собираю информацию о аудио...");
                    var resultFileInfo = await YtDl.RunVideoDataFetch(InitMessage.Text);
                    Title = resultFileInfo.Data.Title;
                    await Bot.EditMessageTextAsync(Message!.Chat.Id, Message.MessageId, "Начинаю скачивание...");
                    TaskState = TaskStatesEnum.Downloading;
                    var opt = new OptionSet
                    {
                        Output = Path.Combine(Configuration.GetInstance().OutputFolder!, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
                    };
                    var res = await YtDl.RunAudioDownload(
                        InitMessage.Text, Helpers.GetAudioFormat(mes), progress: new Progress<DownloadProgress>(ChangeDownloadProgress), overrideOptions: opt
                    );
                    await Bot.EditMessageTextAsync(Message.Chat.Id, Message.MessageId, "Начинаю загрузку...");
                    await using Stream stream = File.OpenRead(res.Data);
                    await Bot.SendAudioAsync(Message.Chat.Id, stream, caption: Title, title: Title);
                    await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId);
                    await Bot.DeleteMessageAsync(Message.Chat.Id, Message.MessageId);
                    File.Delete(res.Data);
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Success));
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
