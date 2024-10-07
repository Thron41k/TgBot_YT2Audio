using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;

namespace TgBot_YT2Audio.DownloadTask.GDrive
{
    public class UploadFile
    {
        public async void Init()
        {
            var service = new DiscoveryService(new BaseClientService.Initializer
            {
                ApplicationName = "TG-YOUTUBE-DL",
                ApiKey = TokenFileReader.Tokens?.GDriveToken,
            });
            var result = await service.Apis.List().ExecuteAsync();
            if (result.Items != null)
            {
                foreach (DirectoryList.ItemsData api in result.Items)
                {
                    Console.WriteLine(api.Id + " - " + api.Title);
                }
            }
        }
    }
}
