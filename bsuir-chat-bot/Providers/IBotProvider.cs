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
    
    public interface IBotProvider
    {
        
        Dictionary<string, Func<List<string>, string>> Functions
        {
            get;
        }
    }

    public abstract class VkBotProvider
    {
        public Dictionary<string, string> Functions { get; protected set; }

        public ProviderState State { get; set; } = ProviderState.Loaded;
        
        public string GetAllHelp()
        {
            var help = new StringBuilder();

            foreach (var function in Functions)
            {
                help.AppendLine(function.Value);
            }

            return help.ToString();
        }

        public MessagesSendParams Handle(VkNet.Model.Message command)
        {
            if (State != ProviderState.Loaded) return null;
//            var color = Console.ForegroundColor;
//            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} [ {GetType().Name.PadLeft(20)} ]: called '{command.Body}'");
//            Console.ForegroundColor = color;

            return _handle(command);
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