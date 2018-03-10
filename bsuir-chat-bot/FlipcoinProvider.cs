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
                {"flipcoin", list => coins[Random.Next() % coins.Length]},
                {"uptime", GetUptime}
            };
        }

        private string GetUptime(List<string> args)
        {
            var curTime = DateTime.Now;

            var uptime = curTime - Program.StartTime;

            return uptime.ToString(@"d\.hh\:mm\:ss");
        }
    }
}