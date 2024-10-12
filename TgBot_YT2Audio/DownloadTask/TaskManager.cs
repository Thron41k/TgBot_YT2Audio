using Telegram.Bot;
using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Enums;
using TgBot_YT2Audio.DownloadTask.Tasks;

namespace TgBot_YT2Audio.DownloadTask
{
    public class TaskManager
    {
        private readonly TaskQueue _tasks = new();

        public async Task<bool> UpdateTask(CallbackQuery query)
        {
            return await _tasks.Update(query);
        }

        public void AddTask(Message initMessage, TelegramBotClient bot, UrlTypesEnum urlType)
        {
            switch (urlType)
            {
                case UrlTypesEnum.None:
                    break;
                case UrlTypesEnum.YouTubeVideo:
                    {
                        var task = new YouTubeTaskDownloadStart(initMessage, bot);
                        task.TaskComplete += TaskComplete;
                        _tasks.Add(task);
                    }
                    break;
                case UrlTypesEnum.YouTubeMusic:
                    {
                        var task = new YouTubeTaskDownloadMusic(initMessage, bot);
                        task.TaskComplete += TaskComplete;
                        _tasks.Add(task);
                    }
                    break;
                case UrlTypesEnum.YouTubeVideoPlaylist:
                    break;
                case UrlTypesEnum.YouTubeMusicPlaylist:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(urlType), urlType, null);
            }
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
                    {
                        task = new YouTubeTaskDownloadMusic(e.InitMessage, e.Bot);
                        task.TaskComplete += TaskComplete;
                        _tasks.Add(task);
                    }
                    break;
                case TaskResultEnum.YouTubeVideo:
                    {
                        task = new YouTubeTaskDownloadVideo(e.InitMessage, e.Bot);
                        task.TaskComplete += TaskComplete;
                        _tasks.Add(task);
                    }
                    break;
            }
        }
    }
}
