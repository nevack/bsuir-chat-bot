using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    public class YouTubeProvider : VkBotProvider
    {
        private readonly VkApi _api;
        
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
            var formats = new List<string>{
                "best[height<=1080]",
                "best[ext=mp4]",
                "bestvideo[ext=mp4]+bestaudio[ext=m4a]",
                "bestvideo[height<=720]+bestaudio",
                "bestvideo[height<=480]+bestaudio",
                "bestvideo[height<=360]+bestaudio"
            };
            
            var getJson = Process.Start("youtube-dl",  $"--write-info-json --geo-bypass --max-filesize 2048m --skip-download -o \"../download/%(id)s/video.%(ext)s\" -f mp4 {id}");
            getJson?.WaitForExit();
            
            var r = new StreamReader($"../download/{id}/video.info.json");
            var json = r.ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(json);

            var title = $"[{id}] {data["title"]}";
            var found = _api.Video.Search(new VideoSearchParams{Query = title});
            if (found.Count == 1)
                return ("", found[0]);
            
            foreach (var format in formats)
            {
                var p = Process.Start("youtube-dl",  $"--geo-bypass --max-filesize 2048m -o \"../download/%(id)s/video.%(ext)s\" -f {format} {id}");
                p?.WaitForExit();
                
                var vid = _api.Video.Save(
                    new VideoSaveParams
                    {
                        Name = title,
                        Description = "THIS VIDEO ON YOUTUBE: https://www.youtube.com/watch?v="+id+"    "+data["description"],
                        NoComments = true
                    });

                try
                {
                    var files = Directory.GetFiles($"../download/{id}");
                    var vidfile = files.Except(new List<string> {$"../download/{id}/video.info.json"}).First();
                    Console.WriteLine(vidfile);
                    var resp = UploadVideo(vid.UploadUrl.ToString(), vidfile).Result;
                    Console.WriteLine(resp);
                    File.Delete(vidfile);

                    if (!resp.Contains("video_hash"))
                        continue;
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                
                Directory.Delete($"../download/{id}", true);

                return (format, vid);
                
            }
            Directory.Delete($"../download/{id}", true);
            throw new ApplicationException("Upload failed");
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
                    requestContent.Add(fileStreamContent, "video_file", filepath);
                    client.Timeout = TimeSpan.FromMinutes(30);

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
            var (_, args) = command.ParseFunc();
            
            var (message, attachement) = GetVid(args[0]);
            
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