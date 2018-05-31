using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                {"rank", "rank [words, chars, messages] - top ten users in this chat ranked by count"},
            };
        }

        private (DateTime, IReadOnlyCollection<Message>) GetHistory(long peerId, DateTime when)
        {
            var output = new List<Message>();
            
            long offset = 0;
            
            while (true)
            {
                var messages = _api.Messages.GetHistory(new MessagesGetHistoryParams
                {
                    Offset = offset, 
                    Count = 200, 
                    PeerId = peerId
                });
                
                foreach (var message in messages.Messages)
                {
                    offset++;
                    
                    if (!message.Date.HasValue) continue;
                    
                    if (message.Date < when || message.Action == MessageAction.ChatInviteUser)
                    {
                        return (message.Date.Value, output);
                    }

                    if (message.Type == MessageType.Received && message.Action == null)
                        output.Add(message);
                }
            }
        }

        private string Top(IEnumerable<Message> messages, Func<string, uint> counter)
        {
            var top = new Dictionary<long, long>();
            
            ulong total = 0;
            
            foreach (var message in messages)
            {
                if (message.FromId == null) continue;
                
                var w = counter(message.Body);
                
                if (!top.ContainsKey(message.FromId.Value))
                {
                    top.Add(message.FromId.Value, w);
                }
                else
                {
                    top[message.FromId.Value] += w;
                }
                
                total += w;
            }

            var result = top.OrderByDescending(pair => pair.Value)
                .Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);
            
            var output = new StringBuilder();

            var users = _api.Users.Get(result.Keys);

            var i = 0;
            foreach (var item in result)
            {
                var user = users[i++];
                output.AppendLine($"{i} - {user.FirstName} {user.LastName}: " +
                                  $"{item.Value} ({1f * item.Value / total:P})");
            }

            return output.ToString();

        }

        protected override MessagesSendParams _handle(Message command)
        {
            var (_, args) = command.ParseFunc();

            var argcount = args.Length;

            var entity = "";
            DateTime fromDate;
            
            if (argcount == 0)
            {
                fromDate = DateTime.Now - TimeSpan.FromDays(7);
            }
            else
            {
                entity = args[0];
                fromDate = DateTime.Now - TimeSpan.Parse(args[argcount - 1]);
            }
            
            var (when, top) = GetHistory(command.GetPeerId(), fromDate);

            var message = $"Top 10 users in this chat ranked by {entity} " +
                          $"count from {when:dd.MM.yyyy HH:mm:ss}";
            
            switch (entity.ToLowerInvariant())
            {
                case "word":
                case "words":
                    message += Top(top, s => (uint) s.Split().Length);
                    break;
                default:
                    message += Top(top, s => 1);
                    break;
                case "char":
                case "chars":
                    message += Top(top, s => (uint) s.Length);
                    break;
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