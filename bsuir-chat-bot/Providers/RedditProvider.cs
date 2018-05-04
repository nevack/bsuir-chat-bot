using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class RedditProvider : VkBotProvider
    {
        private readonly VkApi _api;
        private readonly Reddit _reddit;

        internal RedditProvider(VkApi api)
        {
            Functions = new Dictionary<string, string>();
            Functions.Add("r", "syntax: /r [hot|top|new] subreddit - get subreddit picture");
            
            _api = api;
            _reddit = new Reddit(false);
        }

        public string GetAllHelp()
        {
            var help = new StringBuilder();

            foreach (var function in Functions)
            {
                help.AppendLine(function.Value);
            }

            return help.ToString();
        }

        public override MessagesSendParams Handle(VkNet.Model.Message command)
        {
            command.MarkAsRead(_api);
            var (func, args) = command.ParseFunc();

            var sub = _reddit.GetSubreddit(args[0]);
            var posts = sub.GetTop(FromTime.Day).Take(20);

            string image = null;
            var nsfw = false;
            foreach (var post in posts)
            {
                if (post.Url.ToString().EndsWith(".jpg"))
                {
                    image = post.Url.ToString();
                    nsfw = post.NSFW;
                    break;
                }
            }

            if (nsfw)
            {
                return new MessagesSendParams
                {
                    Message = $"Link for 'friend': {image}",
                    PeerId = command.GetPeerId()
                };
            }

            if (!string.IsNullOrEmpty(image))
            {
                var server = _api.Photo.GetMessagesUploadServer(command.GetPeerId());
                var wc = new WebClient();
                
                byte[] imageBytes = wc.DownloadData(image);

                var responseFile = UploadImage(server.UploadUrl, imageBytes).Result;

//            var responseFile = Encoding.ASCII.GetString(wc.UploadFile(server.UploadUrl, 
//                !string.IsNullOrEmpty(image) ? "1.jpg" : @"C:\Users\dimch\Desktop\93jwxgJXMiY.jpg"));

                var photos = _api.Photo.SaveMessagesPhoto(responseFile);

                return new MessagesSendParams
                {
                    Attachments = photos,
                    PeerId = command.GetPeerId()
                };
            }

            return null;
        }
        
        private async Task<string> UploadImage(string url, byte[] data)
        {
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(data);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
//                imageContent.Headers.ContentEncoding.Add("UTF-8");
                requestContent.Add(imageContent, "photo", "image.jpg");

                var response = await client.PostAsync(url, requestContent);

                return Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }
    }
}