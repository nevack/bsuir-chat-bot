using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VkNet.Model;

namespace bsuir_chat_bot
{
    public static class VkMessageParser
    {
        public static List<Message> ParseLongPollMessage(string content)
        {
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(content);
            if (!responseDict.ContainsKey("updates")) return null;
            List<Message> messageList = new List<Message>();
            foreach (var update in responseDict["updates"])
            {
                if (update[0] == 4)
                {
                    Message parsed = new Message();
                    parsed.Body = update[5];
                    parsed.Id = update[1];
                    parsed.Date = DateTimeOffset.FromUnixTimeSeconds((long)update[4]).UtcDateTime;
                    if (update[3] > 2000000000)
                    {
                        parsed.ChatId = (update[3] - 2000000000).ToString();
                        parsed.FromId = update[6]["from"];
                    }
                    else
                        parsed.FromId = update[3];
                    messageList.Add(parsed);
                }
            }
            return messageList;
        }
    }
}