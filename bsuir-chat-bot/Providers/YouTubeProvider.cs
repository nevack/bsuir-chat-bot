using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
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

        private (string, Video) GetVid(string link)
        {   
            var matchLink = new Regex(@"(?!http).(?:(?:vi\/)|(?:favicon-)|(?:v(?:[\/=]|(?:%3D)))|(?:be\/)|^)((?:\w|-)+)");
            var match = matchLink.Match(link);
            
            if (!match.Success)
                throw new Exception("Could not match YouTube video ID");

            var id = match.Groups[1].Value;
            Console.WriteLine(id);
            var getJSON = Process.Start("youtube-dl",  $"--write-info-json --geo-bypass --max-filesize 2048m --skip-download -o \"../download/%(id)s/video.%(ext)s\" -f mp4 {id}");
            getJSON?.WaitForExit();
            
            StreamReader r = new StreamReader($"../download/{id}/video.info.json");
            string json = r.ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(json);

            var title = $"[{id}] {data["title"]}";
            var found = _api.Video.Search(new VideoSearchParams{Query = title});
            if (found.Count == 1)
                return ("", found[0]);
            
            var p = Process.Start("youtube-dl",  $"--write-info-json --geo-bypass --max-filesize 2048m -o \"../download/%(id)s/video.%(ext)s\" -f mp4 {id}");
            p?.WaitForExit();
            
            
            var vid = _api.Video.Save(
                new VideoSaveParams
                {
                    Name = title,
                    Description = data["description"],
                    NoComments = true
                });

            try
            {
                var wc = new WebClient();
                var resp = UploadVideo(vid.UploadUrl.ToString(), $"../download/{id}/video.mp4").Result;
                dynamic parsedResp = JsonConvert.DeserializeObject(resp);
                var t = parsedResp["video_hash"];
            }
            catch (KeyNotFoundException e)
            {
                throw new Exception("Upload failed");
            }
            finally
            {
               Directory.Delete($"../download/{id}", true);
            }

            return ("", vid);
        }
        
        private static async Task<string> UploadVideo(string url, string filepath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var requestContent = new MultipartFormDataContent();
                    var fileStreamContent = new StreamContent(new FileStream(filepath, FileMode.Open));

                    fileStreamContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    requestContent.Add(fileStreamContent);
                    client.Timeout = TimeSpan.MaxValue;

                    var response = await client.PostAsync(url, requestContent);

                    return Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync());
                }
                catch (Exception e)
                {
                    Console.Write(e);
                    throw;
                }
                
            }
        }
        
        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (func, args) = command.ParseFunc();
            
            
            string message;
            Video attachement;

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
                Attachments = new List<MediaAttachment>{attachement},
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}