using System;
using System.Collections;
using System.Collections.Concurrent;
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
        
        private static ConcurrentQueue<Task> _queue;
        public static bool Kill = false;

        internal Worker(ConcurrentQueue<Task> queue)
        {
            _queue = queue;
        }

        public void Work()
        {
            while (!Kill)
            {
                if (_queue.TryDequeue(out var task))
                {
                    var returnValue = task.Function(task.Args);
                    Console.WriteLine(returnValue);
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}