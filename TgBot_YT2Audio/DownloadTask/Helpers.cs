using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeDLSharp.Metadata;

namespace TgBot_YT2Audio.DownloadTask
{
    public static class Helpers
    {
        private static readonly string[] Formats = ["240p", "360p", "480p", "720p", "1080p", "720p60", "1080p60"];
        public static (List<FormatData> FormatList, List<string> FormatNames) GetFormatList(IEnumerable<FormatData> formats)
        {
            var formatList = new List<FormatData>();
            foreach (var format in Formats)
            {
                formatList.AddRange(formats.Where(x => x.FormatNote == format).ToList());
            }
            return (formatList, formatList.Select(x=>x.FormatNote).Distinct().ToList());
        }
    }
}
