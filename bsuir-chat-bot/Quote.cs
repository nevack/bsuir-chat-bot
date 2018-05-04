using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace bsuir_chat_bot
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [UsedImplicitly]
    public class Quote
    {
        public string Text { get; set; }
        public string AddedBy { get; set; }
        public long AddedDate { get; set; }
        public long OriginalDate { get; set; }
    }
}