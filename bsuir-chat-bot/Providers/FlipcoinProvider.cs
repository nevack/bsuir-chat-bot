using System;
using System.Collections.Generic;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class FlipcoinProvider : VkBotProvider
    {
        private static readonly Random Random = new Random();

        private static readonly string[] Coins = {"Орёл", "Решка"};

        public FlipcoinProvider()
        {
            Functions = new Dictionary<string, string>
            {
                {"flipcoin", "flipcoin - get Head or Tails"}
            };
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {            
            return new MessagesSendParams()
            {
                Message = Coins[Random.Next() % 2],
                PeerId = command.GetPeerId()
            };
        }
    }
}