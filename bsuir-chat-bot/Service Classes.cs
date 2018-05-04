using System;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public class Command
    {
        public VkNet.Model.Message Message;
        public Func<List<string>, string> Function;
        public List<string> Args;
        public DateTime RecievedTime { get; }
        
        public Command(VkNet.Model.Message message, Func<List<string>, string> function, List<string> args)
        {
            Args = args;
            Function = function;
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