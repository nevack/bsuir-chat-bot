using System.Collections.Concurrent;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public abstract class ApiInteractor
    {
        protected ConcurrentQueue<Message> _outputQueue;
        
        protected ApiInteractor(ConcurrentQueue<Message> outputQueue)
        {
            _outputQueue = outputQueue;
        }
        
        public abstract void SendMessage(Message message);

        public abstract IEnumerable<Message> GetHistory(string chat);
    }
}