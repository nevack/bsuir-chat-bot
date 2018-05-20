using System;
using System.Threading;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    
    It's bullshit
    // I did not hit hit her
    // I did not
    // ...
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
                        _bot.Responses.Enqueue(new MessagesSendParams()
                        {
                            PeerId = task.Message.GetPeerId(),
                            Message = e.Message
                        });
                    }
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}
