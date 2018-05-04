using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using VkNet;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class RedditProvider : VkBotProvider
    {
        private readonly VkApi _api;
        private readonly Reddit _reddit;

        internal RedditProvider(VkApi api)
        {
            Functions = new Dictionary<string, string>
            {
                {"r", "syntax: /r [hot|top|new] subreddit - get subreddit picture"}
            };

            _api = api;
            _reddit = new Reddit(false);
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            command.MarkAsRead(_api);
            var (_, args) = command.ParseFunc();

            var sub = _reddit.GetSubreddit(args[0]);
            var posts = sub.GetTop(FromTime.Day).Take(20).Where(p => p.Url.ToString().EndsWith(".jpg")).ToList();
            
            var rand = new Random();

            if (posts.Count == 0)
            {
                return new MessagesSendParams
                {
                    Message = "Error getting image",
                    PeerId = command.GetPeerId()
                };
            }

            var post = posts[rand.Next(0, posts.Count)];
            var image = post.Url.ToString();

            if (post.NSFW)
            {
                return new MessagesSendParams
                {
                    Message = $"Link for 'friend': {image}",
                    PeerId = command.GetPeerId()
                };
            }

            var server = _api.Photo.GetMessagesUploadServer(command.GetPeerId());
            var wc = new WebClient();
                
            var imageBytes = wc.DownloadData(image);

            var responseFile = UploadImage(server.UploadUrl, imageBytes).Result;

            var photos = _api.Photo.SaveMessagesPhoto(responseFile);

            return new MessagesSendParams
            {
                Attachments = photos,
                PeerId = command.GetPeerId()
            };
        }
        
        private async Task<string> UploadImage(string url, byte[] data)
        {
            using (var client = new HttpClient())
            {
                var requestContent = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(data);
                
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                requestContent.Add(imageContent, "photo", "image.jpg");

                var response = await client.PostAsync(url, requestContent);

                return Encoding.ASCII.GetString(await response.Content.ReadAsByteArrayAsync());
            }
        }
    }
}