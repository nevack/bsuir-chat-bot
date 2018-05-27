using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace bsuir_chat_bot
{   
    public class YouTubeProvider : VkBotProvider
    {
        private VkApi _api;
        
        public YouTubeProvider(VkApi api)
        {
            _api = api;
            Functions = new Dictionary<string, string>
            {
                {"yt", "yt [video link] - upload selected video to Vkontakte"}
            };
        }

        private (string, long) GetVid(string link)
        {   
            var matchLink = new Regex(@"(?!http).(?:(?:vi\/)|(?:favicon-)|(?:v(?:[\/=]|(?:%3D)))|(?:be\/)|^)((?:\w|-)+)");
            var match = matchLink.Match(link);
            
            if (!match.Success)
                return ("Invalid video URL/ID", 0);

            var id = match.Groups[1].Value;
            Console.WriteLine(id);
            var p = Process.Start("youtube-dl",  $"--write-info-json --geo-bypass --max-filesize 2048m -o \"../download/%(id)s/video.%(ext)s\" -f mp4 {id}");
            p?.WaitForExit();
            
            StreamReader r = new StreamReader($"../download/{id}/video.info.json");
            string json = r.ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(json);
            
            var server = _api.Video.Save(
                new VideoSaveParams
                {
                    Name = data["uploader"]+" - "+data["title"],
                    Description = data["description"],
                    NoComments = true
                });
            
            dynamic parsedResp;
            try
            {
                var resp = UploadVideo(server.UploadUrl.ToString(), $"../download/{id}/video.mp4").Result;
                parsedResp = JsonConvert.DeserializeObject(resp);
            }
            finally
            {
                Directory.Delete($"../download/{id}", true);
            }
            return ("", parsedResp["video_id"]);
        }
        
        private static async Task<string> UploadVideo(string url, string filepath)
        {
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var fileStreamContent = new StreamContent(new FileStream(filepath, FileMode.Open));
                
                fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(fileStreamContent, "video_file", "video.mp4");

                var response = await client.PostAsync(url, requestContent);

                return Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (func, args) = command.ParseFunc();
            
            
            string message;
            long attachement;

            switch (func.ToLowerInvariant())
            {
                case "yt":
                    (message, attachement) = GetVid(args[0]);
                    break;
                default:
                    throw new KeyNotFoundException();
            }
            
            var param = new MessagesSendParams
            {
                Message = message,
                Attachments = new List<MediaAttachment>{new Video{OwnerId = _api.UserId, Id = attachement}},
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}