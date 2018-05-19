using System;
using System.Collections.Generic;
using System.Text;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public enum ProviderState
    {
        Loaded,
        Unloaded,
        Unloadable
    }

    public abstract class VkBotProvider
    {
        public Dictionary<string, string> Functions { get; protected set; }

        public ProviderState State { get; set; } = ProviderState.Unloaded;
        
        public string GetAllHelp()
        {
            var help = new StringBuilder($"[{GetType().Name}]\n");

            foreach (var function in Functions)
            {
                help.AppendLine("/" + function.Value);
            }

            return help.ToString();
        }

        public MessagesSendParams Handle(VkNet.Model.Message command)
        {
//            var color = Console.ForegroundColor;
//            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now:hh\\:mm\\:ss\\.fff} [ {GetType().Name.PadLeft(20)} ]: called '{command.Body}' by https://vk.com/id{command.FromId}");
//            Console.ForegroundColor = color;
            
            if (State == ProviderState.Unloaded)
                throw new Exception($"{GetType().Name} is not loaded");
//                return new MessagesSendParams()
//                {
//                    Message = $"{GetType().Name.PadLeft(20)} is not loaded",
//                    PeerId = command.GetPeerId()
//                };

            return _handle(command);
        }

        public string GetName()
        {
            return GetType().Name.ToLowerInvariant().Replace("provider", "");
        }
        
        protected abstract MessagesSendParams _handle(VkNet.Model.Message command);
    }

    public abstract class CommandHandler
    {
        public string Name;
        public string Help;
        public VkBotProvider Provider;

        public abstract MessagesSendParams Handle(string func, List<string> args);
    }
}