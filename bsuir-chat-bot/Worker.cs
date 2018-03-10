using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace bsuir_chat_bot
{
    
    public class Worker
    {
        public class Task
        {
            public Func<List<string>, string> Function;
            public List<string> Args;

            public Task(Func<List<string>, string> func, List<string> list)
            {
                Function = func;
                Args = list;
            }
        }
        
        private static Queue<Task> _queue;
        private static Queue<string> _returnQueue;
        public static bool Kill = false;

        internal Worker(Queue<Task> queue, Queue<string> returnQueue)
        {
            _queue = queue;
            _returnQueue = returnQueue;
        }

        public void Work()
        {
            while (!Kill)
            {
                lock (_queue)
                {
                    if (_queue.Count != 0)
                    {
                        var task = _queue.Dequeue();
                        string returnValue = task.Function(task.Args);
                        lock (_returnQueue)
                        {
                            _returnQueue.Enqueue(returnValue);
                        }
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }
                }
            }
        }
    }
}