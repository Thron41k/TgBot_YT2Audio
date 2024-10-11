using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using TgBot_YT2Audio.DownloadTask.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using File = System.IO.File;

namespace TgBot_YT2Audio.DownloadTask;
public class DownloadTask(Message initMessage, Message message, TelegramBotClient bot, UrlTypesEnum urlType)
{
    
    private List<FormatData> _formats = [];
    private string? _quality = "";

    //public async Task Update(CallbackQuery query)
    //{
    //    switch (TaskState)
    //    {
    //        case TaskStatesEnum.Created:
    //            await TaskTypeChooseComplete(query.Data);
    //            break;
    //        case TaskStatesEnum.TypeSelected:
    //            await TaskQualityChooseComplete(query.Data);
    //            break;
    //        case TaskStatesEnum.FormatSelected:
    //            await TaskFormatChooseComplete(query.Data);
    //            break;
    //        case TaskStatesEnum.Downloading:    
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }
    //}

    //private async Task TaskFormatChooseComplete(string? mes)
    //{
    //    try
    //    {
    //        if (mes != null)
    //        {
    //            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю скачивание...");
    //            TaskState = TaskStatesEnum.Downloading;
    //            var progress = new Progress<DownloadProgress>(ChangeDownloadProgress);
    //            switch (TaskType)
    //            {
    //                case TaskTypesEnum.Video:
    //                    {
    //                        var format = _formats.Where(x => x.Extension == mes).MaxBy(x => x.Bitrate);
    //                        if (format != null)
    //                        {
    //                            var opt = new OptionSet
    //                            {
    //                                Format = format.FormatId,
    //                                Output = Path.Combine(_ytDl.OutputFolder, "video", "video_%(id)s_%(format_note)s.%(ext)s")
    //                            };
    //                            var res = await _ytDl.RunVideoDownload(
    //                                initMessage.Text, progress: progress,
    //                                overrideOptions: opt
    //                            );
    //                            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю загрузку...");
    //                            await using Stream stream = File.OpenRead(res.Data);
    //                            await bot.SendVideoAsync(message.Chat.Id, stream, caption: Title,
    //                                supportsStreaming: true);
    //                            await bot.DeleteMessageAsync(initMessage.Chat.Id, initMessage.MessageId);
    //                            await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
    //                            File.Delete(res.Data);
    //                        }

    //                        Fail = true;
    //                        break;
    //                    }

    //                case TaskTypesEnum.Audio:
    //                    {
    //                        var opt = new OptionSet
    //                        {
    //                            Output = Path.Combine(_ytDl.OutputFolder, "audio", "audio_%(id)s_%(format_note)s.%(ext)s")
    //                        };
    //                        var res = await _ytDl.RunAudioDownload(
    //                            initMessage.Text, Helpers.GetAudioFormat(mes), progress: progress, overrideOptions: opt
    //                        );
    //                        await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Начинаю загрузку...");
    //                        await using Stream stream = File.OpenRead(res.Data);
    //                        await bot.SendAudioAsync(message.Chat.Id, stream, caption: Title,title: Title);
    //                        await bot.DeleteMessageAsync(initMessage.Chat.Id, initMessage.MessageId);
    //                        await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
    //                        File.Delete(res.Data);
    //                        Fail = true;
    //                        break;
    //                    }
    //                default:
    //                case TaskTypesEnum.None:
    //                    throw new ArgumentOutOfRangeException();
    //            }

    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //    }
    //    Fail = true;
    //    await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    //}

    //private async void ChangeDownloadProgress(DownloadProgress p)
    //{
    //    try
    //    {
    //        if (string.IsNullOrEmpty(p.TotalDownloadSize)) return;
    //        var pattern = new Regex("[0-9]*[.]?[0-9]+");
    //        var match = pattern.Match(p.TotalDownloadSize);
    //        var tryResult = float.TryParse(match.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,out var result);
    //        if (!tryResult) return;
    //        var totalPercent = (float)Math.Round(100f * p.Progress / 1f, 2);
    //        if ((int)totalPercent % 10 <= 3)
    //        {
    //            if (ProgressQuoted) return;
    //            ProgressQuoted = true;
    //            var unit = p.TotalDownloadSize.Replace(match.Value, "");
    //            await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId,
    //                $"Скачано {Math.Round(p.Progress * result / 1f, 2)}{unit} из {result}{unit}. {totalPercent}%");
    //        }
    //        else
    //        {
    //            ProgressQuoted = false;
    //        }
    //    }
    //    catch
    //    {
    //        // ignored
    //    }
    //}

    //private async Task TaskQualityChooseComplete(string? mes)
    //{
    //    try
    //    {
    //        if (mes != null)
    //        {
    //            TaskState = TaskStatesEnum.FormatSelected;
    //            _quality = mes;
    //            _formats = _formats.Where(x => x.FormatNote == _quality).ToList();
    //            await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите формат",
    //                replyMarkup: Helpers.GetKeyboard(_formats.Select(x => x.Extension).Distinct()));
    //            return;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //    }
    //    Fail = true;
    //    await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    //}

    //private async Task TaskTypeChooseComplete(string? mes)
    //{
    //    try
    //    {
    //        switch (mes)
    //        {
    //            case "Видео":
    //                TaskType = TaskTypesEnum.Video;
    //                TaskState = TaskStatesEnum.TypeSelected;
    //                await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Собираю информацию о видео...");
    //                var reVs = await _ytDl.RunVideoDataFetch(initMessage.Text);
    //                var result = Helpers.GetFormatList(reVs.Data.Formats.ToList());
    //                _formats = result.FormatList;
    //                Title = reVs.Data.Title;
    //                await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите качество",
    //                    replyMarkup: Helpers.GetKeyboard(result.FormatNames));
    //                return;
    //            case "Аудио":
    //                TaskType = TaskTypesEnum.Audio;
    //                TaskState = TaskStatesEnum.FormatSelected;
    //                await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Собираю информацию о аудио...");
    //                var resA = await _ytDl.RunVideoDataFetch(initMessage.Text);
    //                Title = resA.Data.Title;
    //                await bot.EditMessageTextAsync(message.Chat, message.MessageId, "Выберите формат",
    //                    replyMarkup: Helpers.GetKeyboard(Helpers.AudioFormats));
    //                return;
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //    }
    //    Fail = true;
    //    await bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, "Что то пошло не так( попробуйте ещё раз.");
    //}

    //public async Task Force()
    //{
    //    await TaskTypeChooseComplete("Аудио");
    //}
}
