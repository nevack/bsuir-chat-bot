using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Enums.Filters;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;

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

        public State BotState = State.Stoped;
        
        private readonly HttpClient Client = new HttpClient();
        private readonly DateTime _startTime;
        public Dictionary<string, Func<List<string>, string>> Functions;
        private ConcurrentQueue<Command> Requests;
        private ConcurrentQueue<Response> Responses;
        
        private VkApi Api;
        private Regex BotCommandRegex;
        
        public Dictionary<string, IBotProvider> Providers;

        private const int NumberOfWorkerThreads = 4;

        public string GetUptime() => (DateTime.Now - _startTime).ToString(@"d\.hh\:mm\:ss");

        public Bot()
        {
            _startTime = DateTime.Now;
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("botconfig.json");

            var configuration = builder.Build();
            
            Api = new VkApi(new NullLogger(new LogFactory()));
            
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
            
            BotCommandRegex = new Regex(@"^[\/\\\!](\w+)");

            Functions = new Dictionary<string, Func<List<string>, string>>();
            
            var system = new SystemProvider(this);

            Providers = new Dictionary<string, IBotProvider>
            {
                ["system"] = new SystemProvider(this),
                ["quote"] = new QuoteProvider("Fuhrer.json"),
                ["ping"] = new PingProvider(),
                ["wait"] = new WaitProvider(),
                ["flipcoin"] = new FlipcoinProvider(),
//                ["reddit"] = new RedditProvider(),
                ["math"] = new MathProvider()
            };

            foreach (var func in system.Functions)
            {
                Functions[func.Key] = func.Value;
            }

            var modules = configuration.GetSection("modules").GetChildren().Select(c => c.Value).ToArray();

            foreach (var module in modules)
            {
                foreach (var func in Providers[module].Functions)
                {            
                    Functions[func.Key] = func.Value;
                }
            }
            
            Requests = new ConcurrentQueue<Command>();
            Responses = new ConcurrentQueue<Response>();
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
            
            foreach (var function in Providers[name].Functions)
            {
                Functions.TryAdd(function.Key, function.Value);
            }

            return true;
        }

        public bool UnloadModule(string name)
        {
            if (!Providers.ContainsKey(name)) return false;

            if (name == "system") return false;
            
            foreach (var function in Providers[name].Functions.Keys)
            {
                        Functions.Remove(function);
            }

            return true;
        }
    
        public void Start()
        {
            BotState = State.Running;
            
            for (var i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new Worker(Requests, Responses);
                var workerThread = new Thread(worker.Work);
                workerThread.Start();
            }
            
            var sender = new MessageSender(Responses, Api);
            var senderThread = new Thread(sender.Work);
            senderThread.Start();
            
            long timestamp = -1;
            var server = Api.Messages.GetLongPollServer();
            
            var reddit = new RedditProvider(Api);
            
            while (true)
            {
                var response = Client.PostAsync($"https://{server.Server}?act=a_check&key={server.Key}&ts={timestamp}&wait=25&mode=2&version=2", null);
                response.Wait();
                
                var responseString = response.Result.Content.ReadAsStringAsync().Result;
                var responseDict = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(responseString);

                try
                {
                    timestamp = responseDict["ts"];
                }
                catch (KeyNotFoundException)
                {
                    server = Api.Messages.GetLongPollServer();
                    continue;
                }

                var messages = Api.ParseLongPollMessage(responseString);
                if (messages == null) continue;
                
                foreach (var message in messages)
                {
                    var s = message.Body.Split(" ").ToList();
                
                    var match = BotCommandRegex.Match(s[0]);

                    if (!match.Success) continue;

                    if (s[0] == "/r") reddit.Handle(message);
                
                    var command = match.Groups[1].Value.ToLower();

                    if (Functions.ContainsKey(command))
                    {
                        var task = new Command(message, Functions[command], s.Skip(1).ToList());
                        Requests.Enqueue(task);
                        Console.WriteLine("Request accepted");
                    }
                }
            }
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