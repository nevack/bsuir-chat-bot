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
                {"wait", list => Wait(Convert.ToInt32(list[0]))}
            };
        }
        
        private string Wait(int N)
        {
            Thread.Sleep(N);
            return $"Waited {N}";
        }
        
    }
}