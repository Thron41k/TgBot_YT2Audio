using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(string url, long id, Message message, TelegramBotClient bot)
{
    private TaskStates _taskState = TaskStates.Created;
    private TaskTypes _taskType = TaskTypes.None;
    private List<FormatData> _formats = new();
    private string? _quality = "";
    private bool _progressQuoted;

    private readonly YoutubeDL _ytdl = new(10)
    {
        YoutubeDLPath = "yt-dlp\\yt-dlp.exe",
        OutputFolder = "output_tmp",
    };
    public bool Fail { get; private set; } = false;

    public bool Check(int messageId, long userId)
    {
        return message.MessageId == messageId && id == userId;
    }

    public async Task Update(CallbackQuery query)
    {
        switch (_taskState)
        {
            case TaskStates.Created:
                await TaskTypeChooseComplete(query.Data);
                break;
            case TaskStates.TypeSelected:
                await TaskQualityChooseComplete(query.Data);
                break;
            case TaskStates.FormatSelected:
                await TaskFormatChooseComplete(query.Data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static InlineKeyboardMarkup GetKeyboard(IEnumerable<string> buttons)
    {
        var keyboard = new InlineKeyboardMarkup();
        var enumerable = buttons as string[] ?? buttons.ToArray();
        for (var i = 0; i < enumerable.Length; i++)
        {
            if (i % 3 == 0 && i != 0)
            {
                keyboard.AddNewRow();
            }
            keyboard.AddButton(enumerable[i], callbackData: enumerable[i]);
        }
        return keyboard;
    }

    private async Task TaskFormatChooseComplete(string? mes)
    {
        try
        {
            if (mes != null)
            {
                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю загрузку...");
                _taskState = TaskStates.Downloading;
                var format = _formats.Where(x => x.Extension == mes).MaxBy(x => x.Bitrate);
                if (format != null)
                {
                    var progress = new Progress<DownloadProgress>(ChangeDownloadProgress);
                    var res = await _ytdl.RunVideoDownload(
                        url, progress: progress,
                        overrideOptions: new OptionSet()
                        {
                            Format = format.FormatId,
                        }
                    );
                    await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    await using Stream stream = System.IO.File.OpenRead(res.Data);
                    await bot.SendVideoAsync(message.Chat.Id, stream, supportsStreaming: true);
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Fail = true;
        await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    }

    private async void ChangeDownloadProgress(DownloadProgress p)
    {
        try
        {
            if (string.IsNullOrEmpty(p.TotalDownloadSize)) return;
            var pattern = new Regex("[0-9]*[.]?[0-9]+");
            var match = pattern.Match(p.TotalDownloadSize);
            var tryResult = float.TryParse(match.Value.Replace('.', ','), out var result);
            if (!tryResult) return;
            var totalPercent = (float)Math.Round(100f * p.Progress / 1f, 2);
            if ((int)totalPercent % 10 <= 3)
            {
                if (_progressQuoted) return;
                _progressQuoted = true;
                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId,
                    $"Загружено {Math.Round(p.Progress * result / 1f, 2)}МБ из {result}МБ. {totalPercent}%");
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

    private async Task TaskQualityChooseComplete(string? mes)
    {
        try
        {
            if (mes != null)
            {
                _taskState = TaskStates.FormatSelected;
                _quality = mes;
                _formats = _formats.Where(x => x.FormatNote == _quality).ToList();
                await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите формат",
                    replyMarkup: GetKeyboard(_formats.Select(x => x.Extension).Distinct()));
                return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Fail = true;
        await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    }

    private async Task TaskTypeChooseComplete(string? mes)
    {
        try
        {
            switch (mes)
            {
                case "Видео":
                    _taskType = TaskTypes.Video;
                    _taskState = TaskStates.TypeSelected;
                    await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Собираю информацию о видео...");
                    var res = await _ytdl.RunVideoDataFetch(url);
                    var result = Helpers.GetFormatList(res.Data.Formats);
                    _formats = result.FormatList;
                    await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите качество",
                        replyMarkup: GetKeyboard(result.FormatNames));
                    return;
                case "Аудио":
                    _taskType = TaskTypes.Audio;
                    _taskState = TaskStates.TypeSelected;
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Fail = true;
        await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    }
}
