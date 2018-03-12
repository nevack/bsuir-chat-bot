using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace bsuir_chat_bot
{
    public class PingProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
        
        public PingProvider()
        {
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"ping", Ping},
                {"pong", list => "ping"},
                {"pAnG", list => "HHЫЬЫТолыо!!тЛылЬЬ;27~~&@!"}
            };
        }

        private string Ping(List<string> args)
        {
            var pingSender = new Ping ();
            var reply = pingSender.Send("87.240.129.71");

            var message = "pong 🏓";

            if (reply != null && reply.Status == IPStatus.Success)
            {
                message += $" -- {reply.RoundtripTime * 2}ms";
            }

            return message;
        }
    }
}