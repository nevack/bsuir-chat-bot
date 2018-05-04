using System;
using System.Collections.Concurrent;
using System.Threading;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    
    public class Worker
    {        
        private static ConcurrentQueue<Command> _queue;
        private static ConcurrentQueue<MessagesSendParams> _outputQueue;

        internal Worker(ConcurrentQueue<Command> queue, ConcurrentQueue<MessagesSendParams> outputQueue)
        {
            _queue = queue;
            _outputQueue = outputQueue;
        }

        public void Work()
        {
            while (true)
            {
                if (_queue.TryDequeue(out var task))
                {
                    _outputQueue.Enqueue(task.Function(task.Message));
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}