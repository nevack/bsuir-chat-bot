using System;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public class FlipcoinProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }

        private static Random Random = new Random();

        private static string[] coins = {"Орёл", "Решка"};

        public FlipcoinProvider()
        {
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"flipcoin", list => coins[Random.Next() % coins.Length]}
            };
        }
    }
}