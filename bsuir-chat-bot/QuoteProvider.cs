using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace bsuir_chat_bot
{
    public class QuoteProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
        
        private QuoteDictionary _quotedict;
        private string _quoteFile;

        internal QuoteProvider(string quoteFile)
        {
            ReloadQuotes(quoteFile);

            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"quote", list => GetQuote(Convert.ToInt32(list[0])).Text}
            };
        }

        private Quote GetQuote(int index)
        {
            if (index < 0) index = _quotedict.Quotes.Count + index;
            
            return _quotedict.Quotes[index];
        }

        public Quote this[int index] => GetQuote(index);

        public void ReloadQuotes()
        {
            var json = File.ReadAllText(_quoteFile);
            _quotedict = JObject.Parse(json).ToObject<QuoteDictionary>();
        }

        public void ReloadQuotes(string fileName)
        {
            _quoteFile = fileName;
            ReloadQuotes();
        }
    }
}