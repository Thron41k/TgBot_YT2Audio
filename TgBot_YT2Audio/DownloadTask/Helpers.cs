using System.Text.RegularExpressions;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace TgBot_YT2Audio.DownloadTask
{
    public static class Helpers
    {
        private static readonly string[] Formats = ["240p", "360p", "480p", "720p", "1080p", "720p60", "1080p60"];
        public static string[] AudioFormats => new[] {"m4a", "mp3"};

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
            return (formatList, formatList.Select(x=>x.FormatNote).Distinct().ToList());
        }
        public static bool YouTubeUrlValidate(string url)
        {
            const string pattern = @"^((?:https?:)?\/\/)?((?:www|m)\.)?((?:youtube\.com|youtu.be))(\/(?:[\w\-]+\?v=|embed\/|v\/)?)([\w\-]+)(\S+)?$";
            return Regex.IsMatch(url, pattern);
        }
    }
}
