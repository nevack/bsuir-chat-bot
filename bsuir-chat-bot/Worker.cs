using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VkNet.Model;

namespace bsuir_chat_bot
{
    
    public class Worker
    {        
        private static ConcurrentQueue<Command> _queue;
        private static ConcurrentQueue<Response> _outputQueue;
        public static bool Kill = false;

        internal Worker(ConcurrentQueue<Command> queue, ConcurrentQueue<Response> outputQueue)
        {
            _queue = queue;
            _outputQueue = outputQueue;
        }

        public void Work()
        {
            while (!Kill)
            {
                if (_queue.TryDequeue(out var task))
                {
                    string returnValue = task.Function(task.Args);
                    _outputQueue.Enqueue(new Response(task.Message, returnValue));
                    Console.WriteLine(returnValue);
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}