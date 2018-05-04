using System;
using System.Collections.Generic;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class Command
    {
        public VkNet.Model.Message Message;
        public Func<VkNet.Model.Message, MessagesSendParams> Function;
        public DateTime RecievedTime { get; }
        
        public Command(VkNet.Model.Message message, Func<VkNet.Model.Message, MessagesSendParams> f)
        {
            Function = f;
            Message = message;
            RecievedTime = DateTime.Now;
        }
    }

    public class Response
    {
        public VkNet.Model.Message InputMessage;
        public string FuncOutput;

        public Response(VkNet.Model.Message inputMessage, string funcOutput)
        {
            InputMessage = inputMessage;
            FuncOutput = funcOutput;
        }
    }
}