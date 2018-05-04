using System;
using System.Collections;

namespace bsuir_chat_bot
{
    public class Message
    {
        public string Author;
        public string Body;
        public string Chat;
        public IEnumerable ForwardedMessages;
        public ApiInteractor Interactor;
        public string MessageId;
        public DateTime Timestamp;

        public Message(ApiInteractor interactor, DateTime timestamp, string chat, string messageId, string author,
            string body = "", IEnumerable forwardedMessages = null)
        {
            Body = body;
            Timestamp = timestamp;
            Chat = chat;
            Author = author;
            MessageId = messageId;
            ForwardedMessages = forwardedMessages;
            Interactor = interactor;
        }
    }
}