using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Model;

namespace bsuir_chat_bot
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Utility
    {
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public static bool IsFromChat(this Message message)
        {
            return message.ChatId.HasValue;
        }

        public static long GetPeerId(this Message message)
        {
            return message.ChatId?.ToPeerId() ?? message.FromId ?? 0;
        }
        
        public static void MarkAsRead(this Message message, VkApi api)
        {
            if (!message.Id.HasValue) return;

            var ids = new List<long> { message.Id.Value };

            api.Messages.MarkAsRead(ids, message.GetPeerId().ToString());
        }
        
        public static (string, string[]) ParseFunc(this Message command)
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