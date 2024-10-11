using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using TgBot_YT2Audio.DownloadTask.Tasks;

namespace TgBot_YT2Audio.DownloadTask
{
    public class TaskManager
    {
        private readonly List<YouTubeTaskBase> _tasks = [];

        public bool UpdateTask(CallbackQuery query)
        {
            var task = _tasks.FirstOrDefault(x => x!.Check(query.Message!.MessageId, query.From.Id), null);
            if (task == null) return false;
            _ = task.Update(query);
            return true;
        }

        public void AddTask(Message initMessage, TelegramBotClient bot, UrlTypesEnum urlType)
        {
            switch (urlType)
            {
                case UrlTypesEnum.None:
                    break;
                case UrlTypesEnum.YouTubeVideo:
                    _tasks.Add(new YouTubeStartTask(initMessage, bot));
                    break;
                case UrlTypesEnum.YouTubeMusic:
                    _tasks.Add(new YouTubeDownloadTaskMusic(initMessage, bot));
                    break;
                case UrlTypesEnum.YouTubeVideoPlaylist:
                    break;
                case UrlTypesEnum.YouTubeMusicPlaylist:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(urlType), urlType, null);
            }
            _tasks[^1].TaskComplete += TaskComplete;
        }

        private void TaskComplete(object? sender, YouTubeTaskBase.YouTubeTaskBaseEventArgs e)
        {
            if (sender is YouTubeTaskBase task)
            {
                task.TaskComplete -= TaskComplete;
                _tasks.Remove(task);
            }
            switch (e.Result)
            {
                case TaskResultEnum.YouTubeMusic:
                    _tasks.Add(new YouTubeDownloadTaskMusic(e.InitMessage, e.Bot));
                    _tasks[^1].TaskComplete += TaskComplete;
                    break;
                case TaskResultEnum.YouTubeVideo:
                    _tasks.Add(new YouTubeDownloadTaskVideo(e.InitMessage, e.Bot));
                    _tasks[^1].TaskComplete += TaskComplete;
                    break;
            }
        }
    }
}
