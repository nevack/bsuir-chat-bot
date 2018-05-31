//using System;
//using System.Collections.Generic;
//using System.Net.NetworkInformation;
//using VkNet.Model.RequestParams;
//
//namespace bsuir_chat_bot
//{   
//    public class StatsProvider : VkBotProvider
//    {
//        public StatsProvider()
//        {
//            Functions = new Dictionary<string, string>
//            {
//                {"rankwords", "rankwords [timespan] - top ten users in this chat ranked by word count"},
//                {"rankchars", "rankchars [timespan] - top ten users in this chat ranked by character count"},
//                {"rankmessages", "rankmessages [timespan] - top ten users in this chat ranked by message count"}
//            };
//        }
//
//        private IEnumerable<Message> GetHistory(long peerId, DateTime targeDateTime)
//        {
//            List<Message> output;
//            
//        }
//
//        protected override MessagesSendParams _handle(VkNet.Model.Message command)
//        {
//            var (func, _) = command.ParseFunc();
//
//            string message;
//            
//            switch (func.ToLowerInvariant())
//            {
//                case "words":
//                    message = Ping();
//                    break;
//                default:
//                    throw new KeyNotFoundException();
//            }
//
//
//            var param = new MessagesSendParams
//            {
//                Message = message,
//                PeerId = command.GetPeerId()
//            };
//
//            return param;
//        }
//    }
//}