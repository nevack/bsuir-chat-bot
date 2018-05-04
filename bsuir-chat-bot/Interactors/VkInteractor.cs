using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using VkNet;
using VkNet.Enums.Filters;

namespace bsuir_chat_bot.Interactors
{
    public class VkInteractor : ApiInteractor
    {
        private VkApi _api;
        private readonly HttpClient _client = new HttpClient();
        private const int INBOX_LP_FLAG = 4;
        
        //  Converts from VkNet.Model.Message to Message
        private Message VkMessageToProper(VkNet.Model.Message vkMessage)
        {
            var fwdMessages = new List<Message>();
            if (vkMessage.ForwardedMessages != null)
                foreach (var vkFwdMessage in vkMessage.ForwardedMessages)
                    fwdMessages.Add(VkMessageToProper(vkFwdMessage));
            else
                fwdMessages = null;
            return new Message(
                body: vkMessage.Body,
                messageId: vkMessage.Id.ToString(),
                timestamp: vkMessage.Date ?? DateTime.Now,
                chat: vkMessage.ChatId.ToString(),
                author: vkMessage.FromId.ToString(),
                interactor: this
            );
        }
        private Message ParseLongPollMessage([CanBeNull] Dictionary<dynamic, dynamic> inputDictionary)
        {
            if (inputDictionary == null)
                return null;
            
            if (inputDictionary[6].ContainsKey("fwd") || inputDictionary[6].ContainsKey("attach1"))
            {
                return VkMessageToProper(_api.Messages.GetById(new ulong[] {inputDictionary[1]})[0]);
            }

            return new Message(
                body: inputDictionary[5],
                messageId: inputDictionary[1],
                timestamp: DateTimeOffset.FromUnixTimeSeconds((long) inputDictionary[4]).UtcDateTime,
                chat: inputDictionary[3],
                author: inputDictionary[3] > 2_000_000_000?inputDictionary[6]["from"]:inputDictionary[3],
                interactor: this
            );
        }
        
        List<Message> StartGettingMessages()
        {
            long timestamp = -1;
            var server = _api.Messages.GetLongPollServer();
            
            while (true)
            {
                var response = _client.PostAsync($"https://{server.Server}?act=a_check&key={server.Key}&ts={timestamp}&wait=25&mode=2&version=2", null);
                response.Wait();
                
                var responseString = response.Result.Content.ReadAsStringAsync().Result;
                var responseDict = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(responseString);

                try
                {
                    timestamp = responseDict["ts"];
                }
                catch (KeyNotFoundException)
                {
                    server = _api.Messages.GetLongPollServer();
                    continue;
                }

                
                foreach (var update in responseDict["updates"])
                    if (update[0] == INBOX_LP_FLAG)
                        _outputQueue.Enqueue(ParseLongPollMessage(update));
            }
        }
        
        public override void SendMessage(Message message)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<Message> GetHistory(string chat)
        {
            throw new System.NotImplementedException();
        }

        public VkInteractor(ConcurrentQueue<Message> outputQueue, IConfiguration configuration) : base(outputQueue)
        {
            
            _api = new VkApi(new NullLogger(new LogFactory()));
            
            var token = configuration["token"];

            var auth = new ApiAuthParams
            {
                ApplicationId = ulong.Parse(configuration["appid"]),
                Settings = Settings.All
            };


            if (token != null)
            {
                auth.AccessToken = token;
            }
            else
            {
                auth.Login = configuration["login"];
                auth.Password = configuration["password"];
            }
            
            _api.Authorize(auth);

            if (!_api.IsAuthorized)
            {
                Console.WriteLine("Failed to log in with credentials: ");
                Console.WriteLine($"{configuration["appid"]}");
                Console.WriteLine($"{configuration["login"]}");
                Console.WriteLine($"{configuration["password"]}");
                Console.WriteLine($"{configuration["accesstoken"]}");
            }

            Console.WriteLine($"Started with token:\n{_api.Token}\n");
        }
    }
}