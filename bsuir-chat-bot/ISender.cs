using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public interface ISender
    {
        void Send(string message);
    }

    public abstract class VkSender : ISender
    {
        private readonly VkApi _api;

        public VkSender(VkApi api)
        {
            _api = api;
        }

        public void Send(string message)
        {
            throw new System.NotImplementedException();
        }
        
        public void Send(Message message)
        {
            _api.Messages.Send(new MessagesSendParams
            {
                PeerId = message.ChatId?.ToPeerId() ?? message.FromId,
                Message = $"{message.Body} [id{message.FromId}|©]"
            });
        }
    }
}