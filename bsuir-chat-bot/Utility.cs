using System;
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
        
        /// <summary>
        /// Extension to truncate very long strings with triple dots
        /// </summary>
        /// <param name="value">Message to truncate</param>
        /// <param name="maxChars">Threshold for determining long strings</param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }

        public static bool IsFromChat(this Message message)
        {
            return message.ConversationMessageId.HasValue;
        }

        public static long GetPeerId(this Message message)
        {
            return message.PeerId ?? throw new Exception("Peer Id can't be null");
        }
        
        public static void MarkAsRead(this Message message, VkApi api)
        {
            if (!message.Id.HasValue) return;

            api.Messages.MarkAsRead(message.PeerId.ToString());
        }
        
        /// <summary>
        /// Extension for VkNet.Model.Message to extract bot command and arguments
        /// </summary>
        /// <param name="command">Message to extract from</param>
        /// <returns>Tuple of function name and list of it's args</returns>
        public static (string, string[]) ParseFunc(this Message command)
        {
            var words = command.Text.Split();
            var func = words[0].Substring(1);
            
            var args = words.Skip(1).ToArray();

            return (func, args);
        }

        public static long ToPeerId(this long id) => id + 2_000_000_000;

        public static long ToChatId(this long id) => id - 2_000_000_000;
    }
}