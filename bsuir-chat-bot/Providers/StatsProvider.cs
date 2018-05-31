using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    public class StatsProvider : VkBotProvider
    {
        private readonly VkApi _api;
        
        public StatsProvider(VkApi api)
        {
            _api = api;
            Functions = new Dictionary<string, string>
            {
                {"rankwords", "rankwords [timespan] - top ten users in this chat ranked by word count"},
                {"rankchars", "rankchars [timespan] - top ten users in this chat ranked by character count"},
                {"rankmessages", "rankmessages [timespan] - top ten users in this chat ranked by message count"}
            };
        }

        private (DateTime, IEnumerable<Message>) GetHistory(long peerId, DateTime targeDateTime)
        {
            var output = new List<Message>();
            long offset = 0;
            var messageBuffer = _api.Messages.GetHistory(new MessagesGetHistoryParams {Offset = 0, Count = 1, PeerId = peerId});
            while (true)
            {
                foreach (var message in messageBuffer.Messages)
                {
                    offset++;
                    Console.WriteLine(message.Body);
                    if (message.Date < targeDateTime || message.Action == MessageAction.ChatInviteUser)
                    {
                        return (message.Date.Value, output);
                    }

                    if (message.Type == MessageType.Received && message.Action != null)
                        output.Add(message);
                }
                messageBuffer = _api.Messages.GetHistory(new MessagesGetHistoryParams {Offset = offset, Count = 100, PeerId = peerId});
            }
        }

        public delegate int Evaluate(String s);

        private IEnumerable<string> Top(IEnumerable<Message> set, Evaluate evaluator)
        {
            var wordMap = new Dictionary<long, long>();
            long total = 0;
            foreach (var message in set)
            {
                if (message.FromId != null && !wordMap.ContainsKey(message.FromId.Value))
                    wordMap.Add(message.FromId.Value, 0);
                var w = evaluator(message.Body);
                wordMap[message.FromId.Value] += w;
                total += w;
            }

            var result = wordMap.OrderByDescending(s => s.Value);
            var output = new List<string>();
            var count = result.Count();
            for (var i = 0; i < 10 && i < count; i++)
            {
                var user = _api.Users.Get(new List<long>{result.ElementAt(i).Key})[0];
                output.Add(
                    $"{i + 1} - {user.FirstName}" +
                    $" {user.LastName}:" +
                    $" {result.ElementAt(i) .Value}" +
                    $" ({Math.Round(100f * result.ElementAt(i) .Value / total, 2)}%)");
            }

            return output;

        }

        protected override MessagesSendParams _handle(Message command)
        {
            var (func, args) = command.ParseFunc();


            var jArgs = args.Aggregate("", (current, e) => current + " " + e);

            var (dt, top) = GetHistory(command.GetPeerId(), DateTime.Now - TimeSpan.Parse(jArgs));
            string message;
            switch (func.ToLowerInvariant())
            {
                case "rankwords": 
                    message = Top(top, s => s.Split().Length).Aggregate("", (current, e) => current + "\n" + e);
                    break;
                case "rankmessages":
                    message = Top(top, s => 1).Aggregate("", (current, e) => current + "\n" + e);
                    break;
                case "rankchars":
                    message = Top(top, s => s.Length).Aggregate("", (current, e) => current + "\n" + e);
                    break;
                default:
                    throw new KeyNotFoundException();
            }
            message = $"Top 10 users in this chat ranked by {func.Substring(4, func.Length-5)} count from {dt.ToShortDateString()} {dt.ToShortTimeString()}"+message;


            var param = new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}