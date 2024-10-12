using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp.Metadata;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadStart : YouTubeTaskBase
    {
        public YouTubeTaskDownloadStart(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskType = TaskTypesEnum.YouTubeTaskDownloadStart;
            Url = InitMessage.Text!;
        }

        private List<FormatData> _formats = [];

        public override async Task Start()
        {
            TaskState = TaskStatesEnum.Created;
            var messageId = InitMessage.MessageId;
            InitMessage = await SendMessageText(InitMessage.Chat.Id, "Что вы хотите скачать?", keyboard: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио").AddNewRow().AddButton("Отмена"));
            await Bot.DeleteMessageAsync(InitMessage.Chat.Id, messageId, cancellationToken: CTokenSource!.Token);
        }
        protected override async Task TaskTypeChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    switch (mes)
                    {
                        case "Видео":
                            await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId,
                                "Собираю информацию о видео...");
                            var res = await YtDl.RunVideoDataFetch(Url, ct: CTokenSource!.Token);
                            var result = Helpers.GetFormatList(res.Data.Formats.ToList());
                            _formats = result.FormatList;
                            Id = res.Data.ID;
                            Title = res.Data.Title;
                            await EditMessageText(InitMessage.Chat.Id, InitMessage.MessageId, "Выберите качество",
                                keyboard: Helpers.GetKeyboard(result.FormatNames));
                            TaskState = TaskStatesEnum.TypeSelected;
                            return;
                        case "Аудио":
                            OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.YouTubeMusic));
                            return;
                        default:
                            await ErrorNotification();
                            return;
                    }
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

        protected override async Task TaskQualityChooseComplete(string? mes)
        {
            try
            {
                if (!string.IsNullOrEmpty(mes))
                {
                    Format = _formats.Where(x => x.FormatNote == mes && x.Extension == "mp4").MaxBy(x => x.Bitrate); ;
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.YouTubeVideo));
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

        protected override Task TaskFormatChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }
    }
}
