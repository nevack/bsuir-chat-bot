using System.Collections.Generic;
using System.Linq;
using System.Text;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class HelpProvider : VkBotProvider
    {
        private readonly Bot _bot;

        internal HelpProvider(Bot bot)
        {
            _bot = bot;
            Functions = new Dictionary<string, string>
            {
                {
                    "help", "help [modulename|all] - get help for module (default all)."
                }
            };
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();

            if (!args.Any() || args[0].ToLowerInvariant() == "all")
            {
                var s = new StringBuilder();

                foreach (var provider in _bot.Providers.Keys)
                {
                    if (provider.State == ProviderState.Unloaded) continue;
                    s.Append(provider.GetAllHelp());
                    s.AppendLine();
                }
                
                return new MessagesSendParams
                {
                    Message = s.ToString(),
                    PeerId = command.GetPeerId()
                };
            }
            
            if (!_bot.Functions.ContainsKey(args[0])) 
                throw new KeyNotFoundException("No such module.");
//                return new MessagesSendParams
//                {
//                    Message = "No suсh module",
//                    PeerId = command.GetPeerId()
//                };

            return new MessagesSendParams
            {
                Message = _bot.Functions[args[0]].Functions[args[0]],
                PeerId = command.GetPeerId()
            };
        }
    }
}