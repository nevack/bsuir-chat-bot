using System.Collections.Generic;
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
                    "help", "help - get help for module"
                }
            };
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();
            
            if (!_bot.Functions.ContainsKey(args[0]))
                return new MessagesSendParams
                {
                    Message = "No sush module",
                    PeerId = command.GetPeerId()
                };

            return new MessagesSendParams
            {
                Message = _bot.Functions[args[0]].GetAllHelp(),
                PeerId = command.GetPeerId()
            };
        }
    }
}