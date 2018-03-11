using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VkNet.Model;

namespace bsuir_chat_bot
{
    public static class VkMessageParser
    {
        private const int MESSAGE = 4;

        public static List<Message> ParseLongPollMessage(string content)
        {
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(content);
            if (!responseDict.ContainsKey("updates")) return null;
            var messageList = new List<Message>();
            foreach (var update in responseDict["updates"])
            {
                if (update[0] == MESSAGE)
                {
                    var parsed = new Message
                    {
                        Body = update[5],
                        Id = update[1],
                        Date = DateTimeOffset.FromUnixTimeSeconds((long) update[4]).UtcDateTime
                    };

                    if (update[3] > 2000000000)
                    {
                        parsed.ChatId = ((long) update[3]).ToChatId();
                        parsed.FromId = update[6]["from"];
                    }
                    else
                    {
                        parsed.FromId = update[3];
                    }

                    messageList.Add(parsed);
                }
            }

            return messageList;
        }

        private static long ToPeerId(this long id)
        {
            return id + 2000000000;
        }

        private static long ToChatId(this long id)
        {
            return id - 2000000000;
        }
    }
}