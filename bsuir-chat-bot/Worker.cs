using System;
using System.Threading;

namespace bsuir_chat_bot
{
    // Oh, hi mark!    
    public class Worker
    {
        private readonly Bot _bot;

        internal Worker(Bot bot)
        {
            _bot = bot;
        }

        public void Work()
        {
            while (_bot.BotState != Bot.State.Stoped)
            {
                if (_bot.Requests.TryDequeue(out var task))
                {
                    try
                    {
                        _bot.Responses.Enqueue(task.Function(task.Message));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR!: "+e.Message);
                    }
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}
