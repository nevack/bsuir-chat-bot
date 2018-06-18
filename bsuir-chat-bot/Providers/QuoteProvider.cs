using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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
                {"addauthor", "addauthor [nickname] [id] - add a new author to the quote list"},
                {"listauthors", "listauthors - list all authors"}
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
            if (!Directory.Exists("../quotes"))
                Directory.CreateDirectory("../quotes");
            _quotedict = new Dictionary<string, Author>();
            foreach (var file in Directory.GetFiles("../quotes"))
            {
                var json = File.ReadAllText(file);
                var a = JObject.Parse(json).ToObject<Author>();
                _quotedict.Add(a.AuthorName.ToLower(), a);
            }
        }

        private void SaveQuotes()
        {
            foreach (var author in _quotedict.Values)
                File.WriteAllText($"../quotes/{author.AuthorName}.json", JsonConvert.SerializeObject(author));
        }

        private string AddQuotes(Message msg, long sender)
        {
            if (msg.ForwardedMessages == null)
                return "";
            var output = new StringBuilder();
            foreach (var message in msg.ForwardedMessages)
            {
                if (_quotedict.Values.Any(x => x.AuthorId == message.UserId.ToString()))
                {
                    var target = _quotedict.First(pair => pair.Value.AuthorId == message.UserId.ToString()).Value;
                    if (target.Quotes.All(quote => quote.Text != message.Body))
                    {
                        if (message.Date != null && message.Body.Length != 0)
                            target.Quotes.Add(new Quote
                            {
                                AddedBy = sender.ToString(),
                                OriginalDate = ((DateTimeOffset) message.Date.Value).ToUnixTimeSeconds(),
                                AddedDate = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds(),
                                Text = message.Body
                            });
                        target.UpdateDate = ((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds();
                        output.AppendLine($"Added quote No. {target.Quotes.Count-1} by {target.AuthorName}");
                    }
                }
                output.AppendLine(AddQuotes(message, sender));
            }

            return output.ToString();
        }
        
        protected override MessagesSendParams _handle(Message command)
        {
            var (func, args) = command.ParseFunc();

            var message = "Error";

            switch (func)
            {
                case "quote":
                    message = GetQuoteString(args);
                    break;
                case "addquote":
                    if (command.UserId != null) message = AddQuotes(command, command.UserId.Value);
                    if (message == "") message = "No new quotes added";
                    else
                    {
                        SaveQuotes();
                        ReloadQuotes();
                    }
                    break;
                case "addauthor":
                    if (_quotedict.ContainsKey(args[0]) || _quotedict.Any(pair => pair.Value.AuthorId == args[1].ToString()))
                        throw new ArgumentException("This author is already present");
                    if (command.FromId != null)
                        _quotedict.Add(args[0], new Author
                        {
                            AuthorId = args[1],
                            AuthorName = args[0],
                            CreationDate = ((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds(),
                            Quotes = new List<Quote>(),
                            CreatorId = command.FromId.Value.ToString(),
                            UpdateDate = ((DateTimeOffset) DateTime.Now).ToUnixTimeSeconds()
                        });
                    SaveQuotes();
                    ReloadQuotes();
                    message = $"Added author [id{args[1]}|{args[0]}]";
                    break;
                case "listauthors":
                    _quotedict.ToList().ForEach(pair => message+=$"[id{pair.Value.AuthorId}|{pair.Value.AuthorName}] - {pair.Value.Quotes.Count}\n");
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