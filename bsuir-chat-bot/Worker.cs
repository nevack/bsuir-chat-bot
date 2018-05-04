﻿using System.Threading;

namespace bsuir_chat_bot
{
    
    public class Worker
    {
        private readonly Bot _bot;

        internal Worker(Bot bot)
        {
            _bot = bot;
        }

        public void Work()
        {
            while (_bot.BotState == Bot.State.Running)
            {
                if (_bot.Requests.TryDequeue(out var task))
                {
                    _bot.Responses.Enqueue(task.Function(task.Message));
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}