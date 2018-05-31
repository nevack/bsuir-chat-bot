using System;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class Command
    {
        public readonly VkNet.Model.Message Message;
        public readonly Func<VkNet.Model.Message, MessagesSendParams> Function;
        
        public Command(VkNet.Model.Message message, Func<VkNet.Model.Message, MessagesSendParams> f)
        {
            Function = f;
            Message = message;
        }
    }
}