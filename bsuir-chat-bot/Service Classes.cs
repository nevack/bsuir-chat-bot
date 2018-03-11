using System;
using System.Collections.Generic;
using VkNet.Model;

namespace bsuir_chat_bot
{
    public class Command
    {
        public Message Message;
        public Func<List<string>, string> Function;
        public List<string> Args;
        public DateTime RecievedTime { get; }
        
        public Command(Message message, Func<List<string>, string> function, List<string> args)
        {
            Args = args;
            Function = function;
            Message = message;
            RecievedTime = DateTime.Now;
        }
    }

    public class Response
    {
        public Message InputMessage;
        public string FuncOutput;

        public Response(Message inputMessage, string funcOutput)
        {
            InputMessage = inputMessage;
            FuncOutput = funcOutput;
        }
    }
}