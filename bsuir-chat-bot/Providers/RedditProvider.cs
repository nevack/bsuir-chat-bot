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
                {"r", "/r subreddit [hot|top|new] - get subreddit picture (default hot)"}
            };

            _api = api;
            _reddit = new Reddit(false);
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            command.MarkAsRead(_api);
            var (_, args) = command.ParseFunc();


            Subreddit sub;
            
            try
            {
                sub = _reddit.GetSubreddit(args[0]);
            }
            catch (WebException)
            {
                return new MessagesSendParams
                {
                    Message = $"No sush subreddit: {args[0]}.",
                    PeerId = command.GetPeerId()
                };
            }
            
            if (sub == null)
            {
                return new MessagesSendParams
                {
                    Message = $"No sush subreddit: {args[0]}.",
                    PeerId = command.GetPeerId()
                };
            }
            
            Listing<Post> listing;
            
            if (args.Length > 1)
            {
                switch (args[1].ToLowerInvariant())
                {
                    case "top":
                        listing = sub.GetTop(FromTime.Day);
                        break;

                    default: // "hot" also
                        listing = sub.Hot;
                        break;
                    
                    case "new":
                        listing = sub.New;
                        break;
                }
            }
            else
            {
                listing = sub.Hot;
            }

            var posts = listing.Take(50).Where(p => p.Url.ToString().EndsWith(".jpg")).ToList();

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

            if (command.IsFromChat() && post.NSFW)
            {
                return new MessagesSendParams
                {
                    Message = $"Link for 'friend': {image}",
                    PeerId = command.GetPeerId()
                };
            }

            var server = _api.Photo.GetMessagesUploadServer(command.GetPeerId());
            var wc = new WebClient();

            try
            {
                var imageBytes = wc.DownloadData(image);

                var responseFile = UploadImage(server.UploadUrl, imageBytes).Result;

                var photos = _api.Photo.SaveMessagesPhoto(responseFile);

                return new MessagesSendParams
                {
                    Message = $"Reddit [/r/{sub.Name}] {post.Title}\nLink: {post.Shortlink}",
                    Attachments = photos,
                    PeerId = command.GetPeerId()
                };
            }
            catch (WebException)
            {
                return new MessagesSendParams
                {
                    Message = "Error getting image",
                    PeerId = command.GetPeerId()
                };
            }

        }
        
        private static async Task<string> UploadImage(string url, byte[] data)
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