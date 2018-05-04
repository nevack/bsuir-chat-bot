using System.Collections.Concurrent;

namespace bsuir_chat_bot
{
    public interface IApiInteractor
    {
        void SendMessage(Message message);

        
        
        void StoreMessages(ConcurrentQueue<Command> queue);
        
        
    }
}