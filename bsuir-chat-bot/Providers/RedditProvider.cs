using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RedditSharp;

namespace bsuir_chat_bot
{
    public class RedditProvider: IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }

        internal RedditProvider()
        {
            var reddit = new Reddit(false);
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"r", list =>
                {
                    var sub = reddit.GetSubreddit(list[0]);
                    return sub.Hot.First().Title;
                }}
            };
        }
        
    }
}