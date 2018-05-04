using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace bsuir_chat_bot
{
    public class QuoteProvider : IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
        
        private QuoteDictionary _quotedict;
        private string _quoteFile;
        private Random Random;

        internal QuoteProvider(string quoteFile)
        {
            ReloadQuotes(quoteFile);

            Random = new Random();
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"quote", GetQuoteString}
                
            };
        }

        private Quote GetQuote(int index)
        {
            if (index < 0) index = _quotedict.Quotes.Count + index;
            
            return _quotedict.Quotes[index];
        }
        
        private string GetQuoteString(List<string> args)
        {
            var index = !args.Any() ? Random.Next(0, _quotedict.Quotes.Count) : int.Parse(args[0]);
            
            if (index < 0) index = _quotedict.Quotes.Count + index;

            var quote = _quotedict.Quotes[index];
            
            return $"{quote.Text}<br> -- [id{_quotedict.AuthorId}|{_quotedict.AuthorName}] [{index}]";
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