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
    public class YouTubeTaskBase
    {
        private bool _progressQuoted;
        protected readonly TelegramBotClient _bot;
        private readonly UrlTypesEnum _urlType;
        protected readonly Message _initMessage;
        protected Message? _message;
        protected TaskStatesEnum TaskState = TaskStatesEnum.Created;
        protected string _title = "";
        public delegate void CompleteHandler();
        public event CompleteHandler? Complete;
        protected List<FormatData> _formats = [];
        protected string? _quality = "";

        protected readonly YoutubeDL _ytDl = new(30)
        {
            YoutubeDLPath = Configuration.GetInstance().YoutubeDlPath,
            OutputFolder = Configuration.GetInstance().OutputFolder,
        };

        public YouTubeTaskBase(Message initMessage, TelegramBotClient bot)
        {
            _initMessage = initMessage;
            _bot = bot;
            _urlType = urlType;
            switch (_urlType)
            {
                case UrlTypesEnum.YouTubeVideo:
                    _message = bot.SendTextMessageAsync(initMessage.Chat, "Что вы хотите скачать?", replyMarkup: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио")).Result;
                    break;
                case UrlTypesEnum.YouTubeMusic:
                    _ = AudioSelected();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Check(int messageId, long userId)
        {
            return _message != null && _message.MessageId == messageId && _initMessage.From!.Id == userId;
        }

        private async Task AudioSelected(bool clear = false)
        {
            TaskState = TaskStatesEnum.FormatSelected;
            if (!clear)
                _message = await _bot.SendTextMessageAsync(_initMessage.Chat, "Выберите формат",
                    replyMarkup: new InlineKeyboardMarkup().AddButtons("m4a", "mp3"));
            else
                await _bot.EditMessageTextAsync(_initMessage.Chat, _message!.MessageId, "Выберите формат",
                    replyMarkup: new InlineKeyboardMarkup().AddButtons("m4a", "mp3"));
        }

        public async Task Update(object query)
        {
            try
            {
                var queryData = (CallbackQuery)query;
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

        public async Task ErrorNotification()
        {
            Complete?.Invoke();
            await _bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
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
                    await _bot.EditMessageTextAsync(_message!.Chat.Id, _message.MessageId,
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
