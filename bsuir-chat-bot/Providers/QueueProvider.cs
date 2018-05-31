using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    internal class LabQueue
    {
        public string LabId;
        public long Creator;
        public long Creation;
        public string Name;

        public List<long> Queue;
        
    }
    
    public class QueueProvider : VkBotProvider
    {
        private readonly Bot _bot;
        private readonly VkApi _api;

        internal QueueProvider(Bot bot,VkApi api)
        {
            Functions = new Dictionary<string, string>
            {
                {"queue", "lab queues (not stable)"},
                {"q", "same as queue"}
            };

            _bot = bot;
            _api = api;
        }

        private string JoinQueue(long id, long who, string which)
        {
            var filename = $"../queues/{id}.json";

            var exists = File.Exists(filename);

            if (!exists)
            {
                return "There's no queues for this chat!";
            }
            
            var labs = JsonConvert.DeserializeObject<List<LabQueue>>(File.ReadAllText(filename));

            var lab = labs.Last();

            if (lab.Queue.Contains(who))
            {
                return $"You are already in {lab.Name} queue";
            }
            
            lab.Queue.Add(who);
            
            File.WriteAllText(filename, JsonConvert.SerializeObject(labs));

            return $"Added you to queue with name {lab.Name}";
        }
        
        private string LeaveQueue(long id, long who, string which)
        {
            var filename = $"../queues/{id}.json";

            var exists = File.Exists(filename);

            if (!exists)
            {
                return "There's no queues for this chat!";
            }
            
            var labs = JsonConvert.DeserializeObject<List<LabQueue>>(File.ReadAllText(filename));

            var lab = labs.Last();

            if (!lab.Queue.Contains(who))
            {
                return $"You are not in {lab.Name} queue";
            }

            lab.Queue.Remove(who);
            
            File.WriteAllText(filename, JsonConvert.SerializeObject(labs));

            return $"Removed you from queue with name {lab.Name}";
        }

        private string LoadQueue(long id)
        {
            var name = $"../queues/{id}.json";

            var exists = File.Exists(name);

            if (!exists)
            {
                return "There's no queues for this chat!";
            }

            var output = new StringBuilder();
            using (var reader = File.OpenText(name))
            {
                var labs = JsonConvert.DeserializeObject<List<LabQueue>>(reader.ReadToEnd());

                var lab = labs.Last();
                output.AppendLine($"{lab.Name} created at {new DateTime(lab.Creation).ToShortDateString()}");

                var users = _api.Users.Get(lab.Queue);
                var i = 1;
                foreach (var user in users)
                {
                    output.AppendLine($"{i++}. {user.FirstName} {user.LastName}"); //[id{user.Id}|{user.FirstName} {user.LastName}]");
                }
            }

            return output.ToString();
        }

        private string AddQueue(long id, string name, long creator)
        {
            var filename = $"../queues/{id}.json";

            var exists = File.Exists(filename);

            var lab = new LabQueue()
            {
                Creation = DateTime.Now.Ticks,
                Creator = creator,
                Name = name,
                LabId = Guid.NewGuid().ToString(),
                Queue = new List<long>()
            };

            List<LabQueue> labs;
            
            if (exists)
            {            
                labs = JsonConvert.DeserializeObject<List<LabQueue>>(File.ReadAllText(filename));

                labs.Add(lab);
            }
            else
            {
                labs = new List<LabQueue>() { lab };
            }
                
            File.WriteAllText(filename, JsonConvert.SerializeObject(labs));

            return $"Added new lab queue with name {name}";
        }
        
        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var chatid = command.ChatId ?? throw new AccessViolationException("Available only in chats");
            var adminid = _api.Messages.GetChat(chatid).AdminId ?? 
                          throw new AccessViolationException("Only chat admin can use these method");
//            if (!command.ChatId.HasValue) throw new AccessViolationException("Available only in chats");
            
            
            var (_, args) = command.ParseFunc();

            if (args.Length == 0 || args[0] == "load")
            {
                return new MessagesSendParams()
                {
                    PeerId = command.GetPeerId(),
                    Message = LoadQueue(chatid)
                };
            }

            if (args[0] == "add")
            {
                if (command.FromId != adminid && !_bot.Admins.Contains(command.FromId ?? 0)) 
                    throw new AccessViolationException("Only chat admin can use these method");
                
                return new MessagesSendParams()
                {
                    PeerId = command.GetPeerId(),
                    Message = AddQueue(chatid, string.Join(' ', args.Skip(1)), adminid)
                };
            }
            
            if (args[0] == "join")
            {
                return new MessagesSendParams()
                {
                    PeerId = command.GetPeerId(),
                    Message = JoinQueue(chatid, command.FromId ?? 0, "last")
                };
            }
            
            if (args[0] == "leave")
            {
                return new MessagesSendParams()
                {
                    PeerId = command.GetPeerId(),
                    Message = LeaveQueue(chatid, command.FromId ?? 0, "last")
                };
            }
            
            return new MessagesSendParams()
            {
                PeerId = command.GetPeerId(),
                Message = "sosi"
            };
        }
    }
}