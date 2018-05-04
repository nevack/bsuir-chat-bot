using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class QuoteProvider : VkBotProvider
    {
        private QuoteDictionary _quotedict;
        private string _quoteFile;
        private readonly Random _random;

        internal QuoteProvider(string quoteFile)
        {
            ReloadQuotes(quoteFile);

            _random = new Random();
            Functions = new Dictionary<string, string>
            {
                {"quote", "quote - get random quote"}
                
            };
        }

        private string GetQuoteString(IReadOnlyList<string> args)
        {
            int index;
            if (!args.Any())
                index = _random.Next(0, _quotedict.Quotes.Count);
            else
            {
                if (int.TryParse(args[0], out var i))
                {
                    index = i;
                }
                else
                {
                    return "Incorrenct format - integer expected";
                }
            }
            
            if (index < 0) index = _quotedict.Quotes.Count + index;
            
            if (index >= _quotedict.Quotes.Count) return $"There's only [0..{_quotedict.Quotes.Count - 1}] quotes in database";

            var quote = _quotedict.Quotes[index];
            
            return $"{quote.Text}<br> -- [id{_quotedict.AuthorId}|{_quotedict.AuthorName}] [{index}]";
        }

        private void ReloadQuotes()
        {
            var json = File.ReadAllText(_quoteFile);
            _quotedict = JObject.Parse(json).ToObject<QuoteDictionary>();
        }

        private void ReloadQuotes(string fileName)
        {
            _quoteFile = fileName;
            ReloadQuotes();
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();
            return new MessagesSendParams()
            {
                Message = GetQuoteString(args),
                PeerId = command.GetPeerId()
            };
        }
    }
}