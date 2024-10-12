using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeTaskDownloadStart(Message initMessage, TelegramBotClient bot)
        : YouTubeTaskBase(initMessage, bot)
    {
        public override async Task Start()
        {
            TaskState = TaskStatesEnum.Created;
            if (Message == null)
                Message = await SendMessageText(InitMessage.Chat.Id, "Что вы хотите скачать?", keyboard: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио").AddNewRow().AddButton("Отмена"));
            else
                await EditMessageText(Message!.Chat.Id, Message.MessageId, "Что вы хотите скачать?", keyboard: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио").AddNewRow().AddButton("Отмена"));
        }
        protected override async Task TaskTypeChooseComplete(string? mes)
        {
            try
            {
                switch (mes)
                {
                    case "Видео":
                        await Bot.DeleteMessageAsync(Message!.Chat.Id, Message.MessageId, cancellationToken: CTokenSource!.Token);
                        OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.YouTubeVideo));
                        return;
                    case "Аудио":
                        await Bot.DeleteMessageAsync(Message!.Chat.Id, Message.MessageId, cancellationToken: CTokenSource!.Token);
                        OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.YouTubeMusic));
                        return;
                    default:
                        await ErrorNotification();
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

        protected override Task TaskQualityChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }

        protected override Task TaskFormatChooseComplete(string? mes)
        {
            return Task.CompletedTask;
        }
    }
}
