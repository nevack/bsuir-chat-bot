using System;
using System.Collections.Generic;
using System.Threading;

namespace bsuir_chat_bot
{
    public class WaitProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }

        internal WaitProvider()
        {
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"wait", list => Wait(Convert.ToInt32(list[0]))},
                {"stop", list =>
                    {
                        var t = new Thread(() =>
                        {
                            Thread.Sleep(1000);
                            Environment.Exit(2);
                        });
                        t.Start();
                        return "Bye";
                    }
                }
            };
        }
        
        private string Wait(int N)
        {
            Thread.Sleep(N);
            return $"Waited {N}";
        }
        
    }
}