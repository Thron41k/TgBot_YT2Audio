using Telegram.Bot.Types;
using TgBot_YT2Audio.DownloadTask.Tasks;

namespace TgBot_YT2Audio.DownloadTask
{
    public class TaskQueue
    {
        private readonly List<YouTubeTaskBase> _workingList = [];
        private readonly List<YouTubeTaskBase> _waitingList = [];

        public void Add(YouTubeTaskBase task)
        {
            if (_workingList.Count >= Configuration.GetInstance().MaxTaskCount)
            {
                _waitingList.Add(task);
                _ = task.Wait();
            }
            else
            {
                _workingList.Add(task);
                task.Start();
            }
        }

        public Task<bool> Update(CallbackQuery query)
        {
            var task = _workingList.FirstOrDefault(x => x!.Check(query.Message!.MessageId, query.From.Id), null);
            if (task == null) return Task.FromResult(false);
            new Task(() => _ = task.Update(query)).Start();
            return Task.FromResult(true);
        }

        public void Remove(YouTubeTaskBase task)
        {
            try
            {
                if (_workingList.All(x => x != task))
                {
                    if(_waitingList.All(x => x != task)) return;
                    _waitingList.Remove(task);
                }
                _workingList.Remove(task);
                if (_waitingList.Count <= 0 || !(_workingList.Count < Configuration.GetInstance().MaxTaskCount)) return;
                _workingList.Add(_waitingList[0]);
                _waitingList[0].Start();
                _waitingList.RemoveAt(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
