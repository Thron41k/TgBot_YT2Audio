using System.Text.RegularExpressions;
using Telegram.Bot.Types.ReplyMarkups;
using TgBot_YT2Audio.DownloadTask.Enums;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace TgBot_YT2Audio.DownloadTask
{
    public static class Helpers
    {
        private static readonly string[] Formats = ["240p", "360p", "480p", "720p", "1080p", "720p60", "1080p60"];
        public static string[] AudioFormats => ["m4a", "mp3"];

        public static AudioConversionFormat GetAudioFormat(string format)
        {
            return format switch
            {
                "m4a" => AudioConversionFormat.M4a,
                "mp3" => AudioConversionFormat.Mp3,
                _ => AudioConversionFormat.Mp3
            };
        }

        public static (List<FormatData> FormatList, List<string> FormatNames) GetFormatList(List<FormatData> formats)
        {
            var formatList = new List<FormatData>();
            foreach (var format in Formats)
            {
                formatList.AddRange(formats.Where(x => x.FormatNote == format).ToList());
            }
            return (formatList, formatList.Select(x => x.FormatNote).Distinct().ToList());
        }

        public static UrlTypesEnum YouTubeUrlValidate(string url)
        {
            const string pattern = @"^((?:https?:)?\/\/)?((?:www|m|music)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
            if (!Regex.IsMatch(url, pattern)) return UrlTypesEnum.None;
            var rg = Regex.Match(url, pattern);
            if (rg.Groups[2].Value == "music.")
            {
                return rg.Groups[5].Value == "playlist" ? UrlTypesEnum.YouTubeMusicPlaylist : UrlTypesEnum.YouTubeMusic;
            }
            return rg.Groups[5].Value == "playlist" ? UrlTypesEnum.YouTubeVideoPlaylist : UrlTypesEnum.YouTubeVideo;
        }

        public static InlineKeyboardMarkup GetKeyboard(IEnumerable<string> buttons)
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
            keyboard.AddNewRow();
            keyboard.AddButton("Отмена");
            return keyboard;
        }
    }
}
