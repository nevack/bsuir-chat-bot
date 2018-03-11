using System.Collections.Generic;
using Newtonsoft.Json;

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
                    parsed.Text = update[5];
                    parsed.MessageId = update[1].ToString();
                    parsed.Date = update[4];
                    if (update[3] > 2000000000)
                    {
                        parsed.ChatId = (update[3] - 2000000000).ToString();
                        parsed.IsChat = true;
                        parsed.AuthorId = update[6]["from"];
                        
                    }
                    else
                    {
                        parsed.IsChat = false;
                        parsed.ChatId = null;
                        parsed.AuthorId = update[3];
                    }

                    if (update.Count == 8)
                        for (int i = 0; i < update[7].Count; i += 2)
                            parsed.Attachments.Add(update[7][i]+update[7][i+1]);
                    messageList.Add(parsed);
                }
            }

            return messageList;
        }
    }
}