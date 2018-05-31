using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class QuoteProvider : VkBotProvider
    {
        private Dictionary<string, Author> _quotedict;
        private readonly Random _random;

        internal QuoteProvider()
        {
            ReloadQuotes();
            _random = new Random();
            
            var sum = 0;
            foreach (var author in _quotedict)
            {
                sum += author.Value.Quotes.Count;
            }
            
            Functions = new Dictionary<string, string>
            {
                {"quote", $"quote [author] [number] - get a quote by this author with a specified number (random if no number given) (quotes available: {sum - 1})"},
                {"addquote", "addquote - add new quotes from forwarded messages"},
                {"addauthor", "addauthor [nickname] [id] - add a new author to the quote list"}
            };
        }

        private string GetQuoteString(IReadOnlyList<string> args)
        {
            var author = args[0].ToLower();
            var index = args.Count == 1 ? _random.Next(0, _quotedict[author].Quotes.Count) : int.Parse(args[1]);
            
            if (index < 0) index = _quotedict[author].Quotes.Count + index;

            if (index >= _quotedict[author].Quotes.Count)
                throw new ArgumentException(
                    $"There are only [0..{_quotedict[author].Quotes.Count - 1}] quotes in the list of this author");

            var quote = _quotedict[author].Quotes[index];
            
            return $"{quote.Text}<br> -- {(_quotedict[author].AuthorId == ""?_quotedict[author].AuthorName:$"[id{_quotedict[author].AuthorId}|{_quotedict[author].AuthorName}]")} [{index}]";
        }

        private void ReloadQuotes()
        {
            _quotedict = new Dictionary<string, Author>();
            foreach (var file in Directory.GetFiles("Quote Library"))
            {
                var json = File.ReadAllText(file);
                var a = JObject.Parse(json).ToObject<Author>();
                _quotedict.Add(a.AuthorName.ToLower(), a);
            }
        }

        private string AddQuotes(Message msg)
        {
            if (msg.ForwardedMessages == null)
                return "";
            foreach (var message in msg.ForwardedMessages)
            {
                if (_quotedict.Values.Any(x => x.AuthorId == message.FromId.ToString()))
                {
                    if (_quotedict.Value.Quotes.Count)
                }
                    
            }
        }
        

        protected override MessagesSendParams _handle(Message command)
        {
            var (func, args) = command.ParseFunc();

            string message;

            switch (func)
            {
                case "quote":
                    message = GetQuoteString(args);
                    break;
                case "addquote":
                    message = AddQuotes(command);
                    break;
                default:
                    throw new ArgumentException("No matching command found");
            } 

            return new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };
        }
    }
}