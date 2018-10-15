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
using Serilog;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{      
    /// <summary>
    /// Class that controls Providers and message I/O
    /// </summary>
    internal class Bot
    {
        /// <summary>
        /// Bot state
        /// </summary>
        internal enum State
        {
            Running,
            Sleep,
            Stoped
        }
        
        /// <summary>
        /// Number of threads processing commands
        /// </summary>
        /// <value>Positive integer</value>
        private const int NumberOfWorkerThreads = 4;

        /// <summary>
        /// Admins with rights to control bot service functions like
        /// /stop
        /// </summary>
        public long[] Admins { get; }
        
        public State BotState { get; set; } = State.Stoped;
        
        private readonly HttpClient _client = new HttpClient();
        
        /// <summary>
        /// A regular expression for filtering commands from normal messages
        /// </summary>
        private readonly Regex _botCommandRegex;
        
        private readonly DateTime _startTime;
        
        /// <summary>
        /// A dictionary for associating provider name and provider class 
        /// </summary>
        public Dictionary<string, VkBotProvider> Providers { get; }
        
        /// <summary>
        /// A dictionary for associating functions with the respective provider class
        /// </summary>
        public Dictionary<string, VkBotProvider> Functions { get; }
        
        /// <summary>
        /// The main API that interacts with VK
        /// </summary>
        public VkApi Api { get; }
        
        /// <summary>
        /// Injest queue that stores messages to be processed by worker threads
        /// </summary>
        public ConcurrentQueue<Command> Requests { get; }
        
        /// <summary>
        /// Output queue that stores messages to be sent out
        /// </summary>
        public ConcurrentQueue<MessagesSendParams> Responses { get; }

        public string GetUptime() => (DateTime.Now - _startTime).ToString(@"d\.hh\:mm\:ss");

        /// <summary>
        /// Configure a new instance of the Bot
        /// </summary>
        /// <param name="configFileName">Path to a .json file with configuration</param>
        /// <exception cref="FileNotFoundException">Selected file is not available</exception>
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

            // Initialize a new VkApi based on selected config
            Api = new VkApi {RequestsPerSecond = 3};

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
                Log.Error("Failed to log in with credentials: ");
                PrintCredentials(configuration);
                return;
            }

            Log.Information($"Token: {Api.Token}");
            
            _botCommandRegex = new Regex(@"^[\/\\\!](\w+)");

            Functions = new Dictionary<string, VkBotProvider>();

            var names = configuration.GetSection("modules").GetChildren().Select(name => name.Value).ToList();

            Providers = new Dictionary<string, VkBotProvider>
            {
                ["ping"] = new PingProvider(),
                ["reddit"] = new RedditProvider(Api),
                ["system"] = new SystemProvider(this),
                ["quote"] = new QuoteProvider(),
                ["math"] = new MathProvider(),
                ["flipcoin"] = new FlipcoinProvider(),
                ["help"] = new HelpProvider(this),
                ["queue"] = new QueueProvider(this, Api),
                ["youtube"] = new YouTubeProvider(Api),
                ["stats"] = new StatsProvider(this),
                ["bsuir"] = new BsuirProvider()
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

            pollerThread.Join();
        }

        private void StartLongPolling()
        {
            var longPoll = Api.Messages.GetLongPollServer(true);
            while (BotState != State.Stoped)
            {
                try
                {
                    _client
                        .PostAsync(
                            $"https://{longPoll.Server}?act=a_check&key={longPoll.Key}&ts={longPoll.Pts}&wait=25&mode=2&version=2",
                            null).Wait();

                    var response = Api.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams
                    {
                        Pts = longPoll.Pts,
                        Ts = ulong.Parse(longPoll.Ts)
                    });
                    longPoll.Pts = response.NewPts;

                    foreach (var message in response.Messages)
                    {
                        if (message.Type == VkNet.Enums.MessageType.Sended) continue;
                        message.FromId = message.UserId;
                        var s = message.Body.Split(" ").ToList();

                        var match = _botCommandRegex.Match(s[0]);

                        if (!match.Success) continue;

                        var command = match.Groups[1].Value.ToLower();

                        if (!Functions.ContainsKey(command)) continue;
                        var task = new Command(message, Functions[command].Handle);
                        Requests.Enqueue(task);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TooManyRequestsException ||
                        ex is PublicServerErrorException ||
                        ex is HttpRequestException)
                    Thread.Sleep(1000);
                }
            }
        }
        

        private static void PrintCredentials(IConfiguration configuration)
        {
            Console.WriteLine($"{configuration["appid"]}");
            Console.WriteLine($"{configuration["login"]}");
            Console.WriteLine($"{configuration["password"]}");
            Console.WriteLine($"{configuration["accesstoken"]}");
        }
    }
}