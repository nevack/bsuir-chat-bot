using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Enums.Filters;
using NLog;
using VkNet.Exception;
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
        
        public Dictionary<string, VkBotProvider> Providers { get; }
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
                TokenExpireTime = 0,
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
                return;
            }

            Console.WriteLine($"Started with token:\n{Api.Token}\n");
            
            _botCommandRegex = new Regex(@"^[\/\\\!](\w+)");

            Functions = new Dictionary<string, VkBotProvider>();

            var names = configuration.GetSection("modules").GetChildren().Select(name => name.Value).ToList();

            Providers = new Dictionary<string, VkBotProvider>
            {
                ["ping"] = new PingProvider(),
                ["reddit"] = new RedditProvider(Api),
                ["system"] = new SystemProvider(this),
//                ["quote"] = new QuoteProvider("Fuhrer.json"),
                ["math"] = new MathProvider(),
                ["flipcoin"] = new FlipcoinProvider(),
                ["help"] = new HelpProvider(this),
                ["queue"] = new QueueProvider(this, Api),
                ["yt"] = new YouTubeProvider(Api),
                ["stats"] = new StatsProvider(Api)
            };

            foreach (var provider in Providers)
            {
                if (names.Contains(provider.Key)) provider.Value.State = ProviderState.Loaded;
            }
                        
            Admins = configuration.GetSection("admins").GetChildren().Select(c => long.Parse(c.Value)).ToArray();
            
            foreach (var module in Providers.Values)
            {
                foreach (var func in module.Functions)
                {            
                    Functions[func.Key] = module;
                }
            }
            
            Requests = new ConcurrentQueue<Command>();
            Responses = new ConcurrentQueue<MessagesSendParams>();
        }

        public void LoadAll()
        {
            foreach (var name in Providers.Keys)
            {
                LoadModule(name);
            }
        }
        
        public void UnloadAll()
        {
            foreach (var name in Providers.Keys)
            {
                UnloadModule(name);
            }
        }
        
        public bool LoadModule(string name)
        {
            if (!Providers.ContainsKey(name)) return false;
            
            if (Providers[name].State != ProviderState.Unloaded) return false;

            Providers[name].State = ProviderState.Loaded;

            return true;
        }

        public bool UnloadModule(string name)
        {
            if (!Providers.ContainsKey(name)) return false;
            
            if (Providers[name].State != ProviderState.Loaded) return false;
            
            Providers[name].State = ProviderState.Unloaded;

            return true;
        }
    
        public void Start()
        {
            BotState = State.Running;
            
            for (var i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new MessageWorker(this);
                var workerThread = new Thread(worker.Work);
                workerThread.Start();
            }
            
            var sender = new MessageSender(this);
            var senderThread = new Thread(sender.Work);
            var pollerThread = new Thread(StartLongPolling);
            pollerThread.Start();
            senderThread.Start();
        }

        private void StartLongPolling()
        {
            var longPool = Api.Messages.GetLongPollServer(true);
            while (BotState != State.Stoped)
            {
                try
                {
                    var r = _client.PostAsync($"https://{longPool.Server}?act=a_check&key={longPool.Key}&ts={longPool.Pts}&wait=25&mode=2&version=2", null);	
                    r.Wait();
                
                    var response = Api.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams {
                        Pts = longPool.Pts, Ts = longPool.Ts
                    });
                    longPool.Pts = response.NewPts;
                    
                    foreach (var message in response.Messages)
                    {
                        if (message.Type == VkNet.Enums.MessageType.Sended) continue;
                        
                        message.FromId = message.UserId;
                
                        var match = _botCommandRegex.Match(message.Body);

                        if (!match.Success) continue;
                
                        var command = match.Groups[1].Value.ToLower();
                   
                        if (Functions.ContainsKey(command))
                        {
                            var task = new Command(message, Functions[command].Handle);
                            Requests.Enqueue(task);
                        }
                    }
                }
                catch (TooManyRequestsException e)
                {
                    Thread.Sleep(500);
                    continue;
                }
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