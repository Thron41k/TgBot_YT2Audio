using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;

namespace TgBot_YT2Audio.DownloadTask.Tasks
{
    public class YouTubeStartTask : YouTubeTaskBase
    {
        public YouTubeStartTask(Message initMessage, TelegramBotClient bot) : base(initMessage, bot)
        {
            TaskTypeChooseCompleteVar = TaskTypeChooseComplete;
            TaskState = TaskStatesEnum.Created;
            Message = Bot.SendTextMessageAsync(InitMessage.Chat, "Что вы хотите скачать?", replyMarkup: new InlineKeyboardMarkup().AddButtons("Видео", "Аудио")).Result;
        }

        private async Task TaskTypeChooseComplete(string? mes)
        {
            switch (mes)
            {
                case "Видео":
                    await Bot.DeleteMessageAsync(Message!.Chat.Id, Message.MessageId);
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage,Bot,TaskResultEnum.YouTubeVideo));
                    break;
                case "Аудио":
                    await Bot.DeleteMessageAsync(Message!.Chat.Id, Message.MessageId);
                    OnTaskComplete(new YouTubeTaskBaseEventArgs(InitMessage, Bot, TaskResultEnum.YouTubeMusic));
                    break;
                default:
                    await ErrorNotification();
                    break;
            }
        }
    }
}
