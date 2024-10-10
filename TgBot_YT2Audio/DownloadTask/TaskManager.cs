using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgBot_YT2Audio.DownloadTask
{
    public class TaskManager
    {
        private readonly List<DownloadTask> _tasks = [];
        public bool UpdateTask(CallbackQuery query)
        {
            _tasks.RemoveAll(x => x.Fail);
            var task = _tasks.FirstOrDefault(x => x!.Check(query.Message!.MessageId, query.From.Id), null);
            if (task == null) return false;
            _ = task.Update(query);
            return true;
        }
        public void AddTask(string url, long fromId, Message mesMessageId, TelegramBotClient bot)
        {
            _tasks.Add(new DownloadTask(url, fromId, mesMessageId, bot));
        }
    }
}
