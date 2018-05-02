using System;
using System.Collections.Generic;
using System.Net.Http;

namespace bsuir_chat_bot
{
    internal static class Program
    {
        private static void Main()
        {
            var bot = new Bot();
            bot.Start();
            
        }
    }
}