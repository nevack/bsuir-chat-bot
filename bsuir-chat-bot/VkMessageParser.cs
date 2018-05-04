using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VkNet;

namespace bsuir_chat_bot
{
    public static class VkMessageParser
    {
        private const int Message = 4;

        public static List<VkNet.Model.Message> ParseLongPollMessage(this VkApi api,  string content)
        {
            var responseDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(content);
            if (!responseDict.ContainsKey("updates")) return null;
            var messageList = new List<VkNet.Model.Message>();
            foreach (var update in responseDict["updates"])
            {
                if (update[0] == Message)
                {
                    if (update[6].ContainsKey("fwd"))
                    {
                        messageList.Add(api.Messages.GetById(new ulong[] {update[1]})[0]);
                        continue;
                    }

                    var parsed = new VkNet.Model.Message()
                    {
                        Body = update[5],
                        Id = update[1],
                        Date = DateTimeOffset.FromUnixTimeSeconds((long) update[4]).UtcDateTime
                    };

                    if (update[3] > 2_000_000_000)
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

        public static long GetPeerId(this VkNet.Model.Message message)
        {
            return message.ChatId?.ToPeerId() ?? message.FromId.Value;
        }
        
        public static bool MarkAsRead(this VkNet.Model.Message message, VkApi api)
        {
            if (!message.Id.HasValue) return false;
            
            var ids = new List<long>() { message.Id.Value };
            
            return api.Messages.MarkAsRead(ids, message.GetPeerId().ToString()); 
        }
        
        public static (string, string[]) ParseFunc(this VkNet.Model.Message command)
        {
            var words = command.Body.Split();
            var func = words[0];
            
            var args = words.Skip(1).ToArray();

            return (func, args);
        }

        public static long ToPeerId(this long id) => id + 2_000_000_000;

        public static long ToChatId(this long id) => id - 2_000_000_000;
    }
}