using System.Collections.Generic;

namespace bsuir_chat_bot
{
    public class Message
    {
        public string MessageId;
        public string Text;
        public int Date;
        public string AuthorId;
        public bool IsChat;
        public string ChatId;
        public List<dynamic> Attachments;
        public Dictionary<string, dynamic> AdditionalData;
    }
}