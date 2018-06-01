using System;
using System.Threading;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{

    public class MessageWorker
    {
        private readonly Bot _bot;

        internal MessageWorker(Bot bot)
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
                        _bot.Responses.Enqueue(new MessagesSendParams
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
