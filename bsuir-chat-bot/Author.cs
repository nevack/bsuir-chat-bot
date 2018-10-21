using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace bsuir_chat_bot
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class Author
    {
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public long CreationDate { get; set; }
        public long UpdateDate { get; set; }
        public string CreatorId { get; set; }

        public List<Quote> Quotes { get; set; }
    }
}