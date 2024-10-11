using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskBase(Message initMessage, TelegramBotClient bot)
    {
        private bool _progressQuoted;
        protected readonly TelegramBotClient Bot;
        private readonly UrlTypesEnum _urlType;
        protected readonly Message _initMessage;
        protected Message? _message;
        protected TaskStatesEnum TaskState = TaskStatesEnum.Created;
        protected string Title = "";
        public delegate void CompleteHandler();
        public event CompleteHandler? Complete;

        #region Delegates
        protected delegate Task TaskFormatChooseCompleteDelegate(string? data);
        protected delegate Task TaskTypeChooseCompleteDelegate(string? data);
        protected delegate Task TaskQualityChooseCompleteDelegate(string? data);
        protected TaskFormatChooseCompleteDelegate? TaskFormatChooseCompleteVar;
        protected TaskTypeChooseCompleteDelegate? TaskTypeChooseCompleteVar;
        protected TaskQualityChooseCompleteDelegate? TaskQualityChooseCompleteVar;
        #endregion

        protected readonly YoutubeDL YtDl = new(30)
        {
            YoutubeDLPath = Configuration.GetInstance().YoutubeDlPath,
            OutputFolder = Configuration.GetInstance().OutputFolder,
        };

        public bool Check(int messageId, long userId)
        {
            return _message != null && _message.MessageId == messageId && _initMessage.From!.Id == userId;
        }

        public async Task Update(object query)
        {
            try
            {
                var queryData = (CallbackQuery)query;
                switch (TaskState)
                {
                    case TaskStatesEnum.FormatSelected:
                        await TaskFormatChooseCompleteVar?.Invoke(queryData.Data)!;
                        break;
                    case TaskStatesEnum.Created:
                        await TaskTypeChooseCompleteVar?.Invoke(queryData.Data)!;
                        break;
                    case TaskStatesEnum.TypeSelected:
                        await TaskQualityChooseCompleteVar?.Invoke(queryData.Data)!;
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

        public async Task ErrorNotification()
        {
            Complete?.Invoke();
            await Bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
        }

        protected async void ChangeDownloadProgress(DownloadProgress p)
        {
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
                    await Bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId,
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
        }
    }
}
