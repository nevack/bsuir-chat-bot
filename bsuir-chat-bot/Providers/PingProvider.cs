using System;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public class PingProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
        
        public PingProvider()
        {
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"ping", list => "pong"},
                {"pong", list => "ping"}
            };
        }
    }
}