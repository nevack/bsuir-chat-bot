using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using VkNet;
using VkNet.Enums.Filters;
using NLog;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{      
    internal class Bot
    {
        internal enum State
        {
            Running,
            Sleep,
            Stoped
        }
        
        private const int NumberOfWorkerThreads = 4;

        public long[] Admins { get; }
        
        public State BotState { get; set; } = State.Stoped;
        
        private readonly HttpClient _client = new HttpClient();
        
        private readonly Regex _botCommandRegex;
        private readonly DateTime _startTime;
        
        public Dictionary<VkBotProvider, int> Providers { get; }
        public Dictionary<string, VkBotProvider> Functions { get; }
        
        public VkApi Api { get; }
        public ConcurrentQueue<Command> Requests { get; }
        public ConcurrentQueue<MessagesSendParams> Responses { get; }

        public string GetUptime() => (DateTime.Now - _startTime).ToString(@"d\.hh\:mm\:ss");

        public Bot(string configFileName)
        {
            _startTime = DateTime.Now;

            if (!File.Exists(configFileName))
            {
                throw new FileNotFoundException("Can't find config file", configFileName);
            }
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFileName);

            Api = new VkApi(new NullLogger(new LogFactory()));
            Api.RequestsPerSecond = 3;

            var configuration = builder.Build();
            
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
            
            Api.Authorize(auth);

            if (!Api.IsAuthorized)
            {
                Console.WriteLine("Failed to log in with credentials: ");
                PrintCredentials(configuration);
            }

            Console.WriteLine($"Started with token:\n{Api.Token}\n");
            
            _botCommandRegex = new Regex(@"^[\/\\\!](\w+)");

            Functions = new Dictionary<string, VkBotProvider>();

            Providers = new Dictionary<VkBotProvider, int>
            {
                [new PingProvider()] = 1,
                [new RedditProvider(Api)] = 1,
                [new SystemProvider(this)] = 1,
                [new QuoteProvider("Fuhrer.json")] = 1,
                [new MathProvider()] = 1,
                [new FlipcoinProvider()] = 1,
                [new HelpProvider(this)]= 1
            };
            
            Admins = configuration.GetSection("admins").GetChildren().Select(c => long.Parse(c.Value)).ToArray();
            
            foreach (var module in Providers)
            {
                foreach (var func in module.Key.Functions)
                {            
                    Functions[func.Key] = module.Key;
                }
            }
            
            Requests = new ConcurrentQueue<Command>();
            Responses = new ConcurrentQueue<MessagesSendParams>();
        }

//        public void LoadAll()
//        {
//            foreach (var name in Providers.Keys)
//            {
//                LoadModule(name);
//            }
//        }
//        
//        public void UnloadAll()
//        {
//            foreach (var name in Providers.Keys)
//            {
//                UnloadModule(name);
//            }
//        }
//
//        public bool LoadModule(string name)
//        {
//            if (!Providers.ContainsKey(name)) return false;
//            
//            foreach (var function in Providers[name].Functions)
//            {
//                Functions.TryAdd(function.Key, function.Value);
//            }
//
//            return true;
//        }
//
//        public bool UnloadModule(string name)
//        {
//            if (!Providers.ContainsKey(name)) return false;
//
//            if (name == "system") return false;
//            
//            foreach (var function in Providers[name].Functions.Keys)
//            {
//                        Functions.Remove(function);
//            }
//
//            return true;
//        }
    
        public void Start()
        {
            BotState = State.Running;
            
            for (var i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new Worker(this);
                var workerThread = new Thread(worker.Work);
                workerThread.Start();
            }
            
            var sender = new MessageSender(this);
            var senderThread = new Thread(sender.Work);
            senderThread.Start();
            
            long timestamp = -1;
//            var server = Api.Messages.GetLongPollServer();
            var longPool = Api.Messages.GetLongPollServer(true);

            while (BotState != State.Stoped)
            {
                
                var response = Api.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams {
                    Pts = longPool.Pts, Ts = longPool.Ts
                });
                longPool.Pts = response.NewPts;
                
//                
//                var responseString = response.Result.Content.ReadAsStringAsync().Result;
//                var responseDict = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(responseString);
//
//                try
//                {
//                    timestamp = responseDict["ts"];
//                }
//                catch (KeyNotFoundException)
//                {
//                    server = Api.Messages.GetLongPollServer();
//                    continue;
//                }
//
//                var messages = Api.ParseLongPollMessage(responseString);
                
                foreach (var message in response.Messages)
                {
                    message.FromId = message.Type == VkNet.Enums.MessageType.Sended ? Api.UserId : message.UserId;
                    
                    var s = message.Body.Split(" ").ToList();
                
                    var match = _botCommandRegex.Match(s[0]);

                    if (!match.Success) continue;
                
                    var command = match.Groups[1].Value.ToLower();
                   
                    if (Functions.ContainsKey(command))
                    {
                        var task = new Command(message, Functions[command].Handle);
                        Requests.Enqueue(task);
//                        Requests.Enqueue(message);
                    }
                }
                
                Thread.Sleep(350);
            }
            
            Console.WriteLine("System Halt! Bye.");
        }

        private static void PrintCredentials(IConfiguration configuration)
        {
            Console.WriteLine($"{configuration["appid"]}");
            Console.WriteLine($"{configuration["login"]}");
            Console.WriteLine($"{configuration["password"]}");
            Console.WriteLine($"{configuration["accesstoken"]}");
            // Console.WriteLine($"{configuration["shortenerapikey"]}");
        }

//        private static async void QrCodeGenImage(string text, string fileName)
//        {
//            var qrGenerator = new QRCodeGenerator();
//            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
//            var qrCode = new QRCode(qrCodeData);
//            var qrCodeImage = qrCode.GetGraphic(50);
//            await Task.Run(() => qrCodeImage.Save(fileName));
//        }
    }
}