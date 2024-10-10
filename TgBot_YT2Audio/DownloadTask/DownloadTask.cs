using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(Message initMessage, Message message, TelegramBotClient bot)
{
    private TaskStates _taskState = TaskStates.Created;
    private TaskTypes _taskType = TaskTypes.None;
    private List<FormatData> _formats = [];
    private string? _quality = "";
    private bool _progressQuoted;
    private string _title = "";

    private readonly YoutubeDL _ytDl = new(30)
    {
        YoutubeDLPath = Configuration.GetInstance().YoutubeDlPath,
        OutputFolder = Configuration.GetInstance().OutputFolder,
    };
    public bool Fail { get; private set; }

    public bool Check(int messageId, long userId)
    {
        return message.MessageId == messageId && initMessage.From!.Id == userId;
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
            case TaskStates.Downloading:    
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
                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю скачивание...");
                _taskState = TaskStates.Downloading;
                var progress = new Progress<DownloadProgress>(ChangeDownloadProgress);
                switch (_taskType)
                {
                    case TaskTypes.Video:
                        {
                            var format = _formats.Where(x => x.Extension == mes).MaxBy(x => x.Bitrate);
                            if (format != null)
                            {
                                var optV = new OptionSet
                                {
                                    Format = format.FormatId,
                                    Output = Path.Combine(_ytDl.OutputFolder, "video", "video_%(id)s_%(format_note)s.%(ext)s")
                                };
                                var resV = await _ytDl.RunVideoDownload(
                                    initMessage.Text, progress: progress,
                                    overrideOptions: optV
                                );
                                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю загрузку...");
                                await using Stream streamV = File.OpenRead(resV.Data);
                                await bot.SendVideoAsync(message.Chat.Id, streamV, caption: _title,
                                    supportsStreaming: true);
                                await bot.DeleteMessageAsync(initMessage.Chat.Id, initMessage.MessageId);
                                await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                File.Delete(resV.Data);
                            }

                            Fail = true;
                            break;
                        }

                    case TaskTypes.Audio:
                        {
                            var optA = new OptionSet
                            {
                                Output = Path.Combine(_ytDl.OutputFolder, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
                            };
                            var resA = await _ytDl.RunAudioDownload(
                                initMessage.Text, Helpers.GetAudioFormat(mes), progress: progress, overrideOptions: optA
                            );
                            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю загрузку...");
                            await using Stream streamA = File.OpenRead(resA.Data);
                            await bot.SendAudioAsync(message.Chat.Id, streamA, caption: _title,title: _title);
                            await bot.DeleteMessageAsync(initMessage.Chat.Id, initMessage.MessageId);
                            await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            File.Delete(resA.Data);
                            Fail = true;
                            break;
                        }
                    default:
                    case TaskTypes.None:
                        throw new ArgumentOutOfRangeException();
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
            var tryResult = float.TryParse(match.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,out var result);
            if (!tryResult) return;
            var totalPercent = (float)Math.Round(100f * p.Progress / 1f, 2);
            if ((int)totalPercent % 10 <= 3)
            {
                if (_progressQuoted) return;
                _progressQuoted = true;
                var unit = p.TotalDownloadSize.Replace(match.Value, "");
                await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId,
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
                    var reVs = await _ytDl.RunVideoDataFetch(initMessage.Text);
                    var result = Helpers.GetFormatList(reVs.Data.Formats.ToList());
                    _formats = result.FormatList;
                    _title = reVs.Data.Title;
                    await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите качество",
                        replyMarkup: GetKeyboard(result.FormatNames));
                    return;
                case "Аудио":
                    _taskType = TaskTypes.Audio;
                    _taskState = TaskStates.FormatSelected;
                    await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Собираю информацию о аудио...");
                    var resA = await _ytDl.RunVideoDataFetch(initMessage.Text);
                    _title = resA.Data.Title;
                    await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите формат",
                        replyMarkup: GetKeyboard(Helpers.AudioFormats));
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

    public async Task Force()
    {
        await TaskTypeChooseComplete("Аудио");
    }
}
