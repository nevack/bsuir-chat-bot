﻿namespace bsuir_chat_bot
{
    internal static class Program
    {
        private static void Main()
        {
            var bot = new Bot("botconfig.json");
            bot.Start();
        }
    }
}