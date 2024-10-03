using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgBot_YT2Audio.DownloadTask
{
    public class TaskManager
    {
        private readonly List<DownloadTask> _tasks = [];

        public void AddTask(string url, Message message, TelegramBotClient bot)
        {
            _tasks.Add(new DownloadTask(url,message, bot));
        }

        public void UpdateTask(CallbackQuery query)
        {
            var task = _tasks.FirstOrDefault(x => x!.Check(query.Message!.MessageId, query.From.Id), null);
            task?.Update(query);
        }
    }
}
