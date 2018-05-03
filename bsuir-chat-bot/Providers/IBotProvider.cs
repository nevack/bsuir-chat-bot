using System;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public enum ProviderState
    {
        Loaded,
        Unloaded,
        Unloadable
    }
    
    public interface IBotProvider
    {
        
        Dictionary<string, Func<List<string>, string>> Functions
        {
            get;
        }
    }

    public abstract class VkBotProvider : IBotProvider
    {
        public virtual ProviderState State { get; set; }
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
    }
}