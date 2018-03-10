using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace bsuir_chat_bot
{
    public class QuoteProvider
    {
        private static Dictionary<string, Func<List<string>, string>> funcs;
        private QuoteDictionary quotedict;
        private string quoteFile;

        internal QuoteProvider(string quoteFile)
        {
            this.quoteFile = quoteFile;

            var json = File.ReadAllText(quoteFile);
            quotedict = JObject.Parse(json).ToObject<QuoteDictionary>();
        }

        private Quote GetQuote(List<string> args)
        {
            var index = int.Parse(args[0]);
            return GetQuote(index);
        }

        public Quote GetQuote(int index)
        {
            if (index < 0) index = quotedict.Quotes.Count + index;
            
            return quotedict.Quotes[index];
        }

        public Quote this[int index] => GetQuote(index);

        public void ReloadQuotes()
        {
            var json = File.ReadAllText(quoteFile);
            quotedict = JObject.Parse(json).ToObject<QuoteDictionary>();
        }

        public void ReloadQuotes(string fileName)
        {
            quoteFile = fileName;
            ReloadQuotes();
        }
    }
}