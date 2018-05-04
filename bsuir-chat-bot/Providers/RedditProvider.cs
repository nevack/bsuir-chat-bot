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
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class RedditProvider
    {
        private readonly VkApi _api;
        private readonly Reddit _reddit;

        public Dictionary<string, string> Functions;

        internal RedditProvider(VkApi api)
        {
            Functions = new Dictionary<string, string>();
            Functions.Add("r", "syntax: /r [hot|top|new] subreddit - get subreddit picture");
            
            _api = api;
            _reddit = new Reddit(false);
        }

        public Message Handle(VkNet.Model.Message command)
        {
            var (func, args) = command.ParseFunc();

            var sub = _reddit.GetSubreddit(args[0]);
            var posts = sub.Hot.Take(20);

            string image = null;
            foreach (var post in posts)
            {
                if (post.Thumbnail.ToString().Length > 10)
                    if (post.Url.ToString().EndsWith(".jpg"))
                    {
                        image = post.Url.ToString();
                        break;
                    }
            }
            
            
            var server = _api.Photo.GetMessagesUploadServer(command.ChatId?.ToPeerId() ?? command.FromId.Value);
            var wc = new WebClient();
            
//            if (!string.IsNullOrEmpty(image)) wc.DownloadFile(image, "1.jpg");
            
            byte[] imageBytes = wc.DownloadData(image);
            Console.WriteLine(imageBytes.Length);
            
            var responseFile = UploadImage(server.UploadUrl, imageBytes).Result;
            Console.WriteLine(responseFile);

//            var responseFile = Encoding.ASCII.GetString(wc.UploadFile(server.UploadUrl, 
//                !string.IsNullOrEmpty(image) ? "1.jpg" : @"C:\Users\dimch\Desktop\93jwxgJXMiY.jpg"));

            var photos = _api.Photo.SaveMessagesPhoto(responseFile);

            _api.Messages.Send(new MessagesSendParams
            {
                Attachments = photos,
                PeerId = command.ChatId?.ToPeerId() ?? command.FromId,
            });
            
            return null;
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

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}