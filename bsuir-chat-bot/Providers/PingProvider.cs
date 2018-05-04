using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.NetworkInformation;
using VkNet.Model.RequestParams;

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
                {"pang", list => "HHЫЬЫТолыо!!тЛылЬЬ;27~~&@!"}
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
    
    public class Ping2Provider : VkBotProvider
    {
        public Ping2Provider()
        {
            Functions = new Dictionary<string, string>
            {
                {"ping", "ping - returns ping time for vk.com"},
                {"pong", "pong - returns 'ping'"},
                {"pang", "pang - konovalov's ping"}
            };
        }

        private string Ping()
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

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (func, _) = command.ParseFunc();

            string message;
            
            switch (func.ToLowerInvariant())
            {
                case "ping":
                    message = Ping();
                    break;
                case "pong":
                    message = "ping";
                    break;
                case "pang":
                    message = "HHЫЬЫТолыо!!тЛылЬЬ;27~~&@!";
                    break;
                default:
                    throw new KeyNotFoundException();
            }


            var param = new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}