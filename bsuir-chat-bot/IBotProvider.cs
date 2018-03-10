using System;
using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public interface IBotProvider
    {
        Dictionary<string, Func<List<string>, string>> GetFunctions();
    }
}