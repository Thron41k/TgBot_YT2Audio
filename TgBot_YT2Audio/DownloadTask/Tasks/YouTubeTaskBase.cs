using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public abstract class YouTubeTaskBase(Message initMessage, TelegramBotClient bot) : IDisposable
    {
        protected readonly CancellationTokenSource? CTokenSource = new();
        private bool _progressQuoted;
        protected TelegramBotClient Bot { get; } = bot;
        protected Message InitMessage { get; } = initMessage;
        protected Message? Message { get; set; }
        protected TaskStatesEnum TaskState = TaskStatesEnum.None;
        protected string Title = "";
        protected string Id = "";
        protected readonly string Guid = Helpers.DownloadFileGuid();

        #region Complete Event
        public class YouTubeTaskBaseEventArgs(Message initMessage, TelegramBotClient bot, TaskResultEnum result) : EventArgs
        {
            public Message InitMessage { get; } = initMessage;
            public TelegramBotClient Bot { get; } = bot;
            public TaskResultEnum Result { get; } = result;
        }

        public event EventHandler<YouTubeTaskBaseEventArgs>? TaskComplete;
        #endregion

        public abstract Task Start();

        public async Task Wait()
        {
            Message = await SendMessageText(InitMessage.Chat.Id, "В очереди...");
        }

        protected abstract Task TaskTypeChooseComplete(string? mes);
        protected abstract Task TaskQualityChooseComplete(string? mes);
        protected abstract Task TaskFormatChooseComplete(string? mes);

        protected readonly YoutubeDL YtDl = new(1)
        {
            YoutubeDLPath = Configuration.GetInstance().YoutubeDlPath,
            OutputFolder = Configuration.GetInstance().OutputFolder,
            FFmpegPath = Configuration.GetInstance().FFmpegPath,
        };
        public bool Check(int messageId, long userId)
        {
            return Message != null && Message.MessageId == messageId && InitMessage.From!.Id == userId;
        }

        public async Task Update(object query)
        {
            try
            {
                var queryData = (CallbackQuery)query;
                if (queryData.Data == "Отмена")
                {
                    await Cancel();
                    return;
                }
                switch (TaskState)
                {
                    case TaskStatesEnum.FormatSelected:
                        await TaskFormatChooseComplete(queryData.Data);
                        break;
                    case TaskStatesEnum.Created:
                        await TaskTypeChooseComplete(queryData.Data);
                        break;
                    case TaskStatesEnum.TypeSelected:
                        await TaskQualityChooseComplete(queryData.Data);
                        break;
                    case TaskStatesEnum.Downloading:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task Cancel()
        {
            await CTokenSource?.CancelAsync()!;
            await Bot.DeleteMessageAsync(InitMessage.Chat.Id, InitMessage.MessageId);
            await Bot.DeleteMessageAsync(Message!.Chat.Id, Message.MessageId);
            OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Cancelled));
        }

        protected async Task EditMessageText(long chatId, int messageId, string text, bool useKeyboard = true, InlineKeyboardMarkup? keyboard = null)
        {
            if (!useKeyboard)
            {
                await Bot.EditMessageTextAsync(chatId, messageId, text, cancellationToken: CTokenSource!.Token);
                return;
            }

            if (keyboard != null)
            {
                await Bot.EditMessageTextAsync(chatId, messageId, text, replyMarkup: keyboard, cancellationToken: CTokenSource!.Token);
                return;
            }

            await Bot.EditMessageTextAsync(chatId, messageId, text, replyMarkup: new InlineKeyboardMarkup().AddButton("Отмена"), cancellationToken: CTokenSource!.Token);
        }

        protected async Task<Message> SendMessageText(long chatId, string text, bool useKeyboard = true, InlineKeyboardMarkup? keyboard = null)
        {
            if (!useKeyboard) return await Bot.SendTextMessageAsync(chatId, text, cancellationToken: CTokenSource!.Token);
            if (keyboard != null) return await Bot.SendTextMessageAsync(chatId, text, replyMarkup: keyboard, cancellationToken: CTokenSource!.Token);
            return await Bot.SendTextMessageAsync(chatId, text, replyMarkup: new InlineKeyboardMarkup().AddButton("Отмена"), cancellationToken: CTokenSource!.Token);
        }

        protected async Task ErrorNotification()
        {
            OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.Failed));
            await EditMessageText(Message!.Chat.Id, Message.MessageId, "Что то пошло не так( попробуйте ещё раз.", false);
        }

        protected async void ChangeDownloadProgress(DownloadProgress p)
        {
            switch (p.State)
            {
                case DownloadState.PostProcessing:
                    await EditMessageText(Message!.Chat.Id, Message.MessageId, "Подготовка к загрузке...");
                    return;
                case DownloadState.Downloading:
                    try
                    {
                        if (string.IsNullOrEmpty(p.TotalDownloadSize)) return;
                        var pattern = new Regex("[0-9]*[.]?[0-9]+");
                        var match = pattern.Match(p.TotalDownloadSize);
                        var tryResult = float.TryParse(match.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result);
                        if (!tryResult) return;
                        var totalPercent = (float)Math.Round(100f * p.Progress / 1f, 2);
                        if ((int)totalPercent % 10 <= 3)
                        {
                            if (_progressQuoted) return;
                            _progressQuoted = true;
                            var unit = p.TotalDownloadSize.Replace(match.Value, "");
                            await EditMessageText(Message!.Chat.Id, Message.MessageId,
                                $"Скачано {Math.Round(p.Progress * result / 1f, 2)}{unit} из {result}{unit}. {totalPercent}%");
                        }
                        else
                        {
                            _progressQuoted = false;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                    break;
            }
        }

        protected void OnTaskComplete(YouTubeTaskBaseEventArgs e)
        {
            TaskComplete?.Invoke(this, e);
        }

        public void Dispose()
        {
            CTokenSource?.Dispose();
        }
    }
}
