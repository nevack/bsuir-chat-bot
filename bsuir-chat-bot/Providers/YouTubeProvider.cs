using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    public class YouYubeProvider : VkBotProvider
    {
        public YouYubeProvider()
        {
            Functions = new Dictionary<string, string>
            {
                {"yt", "yt [video link] - upload selected video to Vkontakte"}
            };
        }

        private static string GetVid(string videoId)
        {
            Process.Start(
                "youtube-dl --write-info-json --geo-bypass --max-filesize 300m -o \"Download/%(id)s/video.%(ext)s\" -f mp4 {0}");
            return "";

        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (func, link) = command.ParseFunc();
            
            var matchLink = new Regex(@"(?:(?:vi\/)|(?:favicon-)|(?:v(?:[\/=]|(?:%3D)))|(?:be\/))((?:\w|-)+)");
            var match = matchLink.Match(link[0]);
            
            string message;
            if (match.Success)
            {

                var id = match.Groups[1].ToString();

                switch (func.ToLowerInvariant())
                {
                    case "yt":
                        message = GetVid(id);
                        break;
                    default:
                        throw new KeyNotFoundException();
                }
            }
            else
                message = "Invalid video URL/ID";

            var param = new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}