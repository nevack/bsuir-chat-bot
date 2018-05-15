using System.Collections.Concurrent;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public abstract class ApiInteractor
    {
        protected readonly ConcurrentQueue<Message> OutputQueue;
        
        protected ApiInteractor(ConcurrentQueue<Message> outputQueue)
        {
            OutputQueue = outputQueue;
        }
        
        public abstract void SendMessage(Message message);

        public abstract IEnumerable<Message> GetHistory(string chat);
    }
}