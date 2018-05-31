using System.Collections.Generic;
using System.Linq;
using VkNet;

namespace bsuir_chat_bot
{
    public static class Utility
    {
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static bool IsFromChat(this VkNet.Model.Message message)
        {
            return message.ChatId.HasValue;
        }

        public static long GetPeerId(this VkNet.Model.Message message)
        {
            return message.ChatId?.ToPeerId() ?? message.FromId ?? 0;
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
            var func = words[0].Substring(1);
            
            var args = words.Skip(1).ToArray();

            return (func, args);
        }

        public static long ToPeerId(this long id) => id + 2_000_000_000;

        public static long ToChatId(this long id) => id - 2_000_000_000;
    }
}