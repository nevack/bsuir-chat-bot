using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using NLog.Targets;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model.RequestParams;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using VkNet.Model;

namespace bsuir_chat_bot
{
    class Program
    {
        public static readonly HttpClient Client = new HttpClient();
        public static DateTime StartTime;
        private const int NumberOfWorkerThreads = 4;

        public static string GetUptime() => (DateTime.Now - Program.StartTime).ToString(@"d\.hh\:mm\:ss");
        
        static void Main(string[] args)
        {
            StartTime = DateTime.Now;
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("botconfig.json");

            var configuration = builder.Build();

//            Console.WriteLine($"{configuration["appid"]}");
//            Console.WriteLine($"{configuration["login"]}");
//            Console.WriteLine($"{configuration["password"]}");
//            Console.WriteLine($"{configuration["accesstoken"]}");
//            Console.WriteLine($"{configuration["shortenerapikey"]}");
            
            var api = new VkApi(new NullLogger(new LogFactory()));
	
            api.Authorize(new ApiAuthParams
            {
                ApplicationId = ulong.Parse(configuration["appid"]),
                AccessToken = configuration["accesstoken"],
                Settings = Settings.All
            });
            
            var botCommandRegex = new Regex(@"^[\/\\\!](\w+)");

            var funcs = new Dictionary<string, Func<List<string>, string>>();

            var quote = new QuoteProvider("Fuhrer.json");
            var ping = new PingProvider();
            var wait = new WaitProvider();
            var flipcoin = new FlipcoinProvider();
            
            foreach (var func in quote.Functions)
            {
                funcs[func.Key] = func.Value;
            }            
            
            foreach (var func in ping.Functions)
            {
                funcs[func.Key] = func.Value;
            }         
            
            foreach (var func in flipcoin.Functions)
            {
                funcs[func.Key] = func.Value;
            }
            
            foreach (var func in wait.Functions)
            {
                funcs[func.Key] = func.Value;
            }
            
//            var jsons = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.json").ToList();
//            jsons.ForEach(Console.WriteLine);
            
            var requestQueue = new ConcurrentQueue<Command>();
            var outputQueue = new ConcurrentQueue<Response>();
            
            for (var i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new Worker(requestQueue, outputQueue);
                var workerThread = new Thread(worker.Work);
                workerThread.Start();
            }
            
            var sender = new MessageSender(outputQueue, api);
            var senderThread = new Thread(sender.Work);
            senderThread.Start();
            
            long timestamp = -1;
            var server = api.Messages.GetLongPollServer();
            
            while (true)
            {
                var longpolluri = $"https://{server.Server}?act=a_check&key={server.Key}&ts={timestamp}&wait=25&mode=2&version=2";
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
                    server = api.Messages.GetLongPollServer();
                    continue;
                }

                var messages = VkMessageParser.ParseLongPollMessage(responseString);
                if (messages == null) continue;
                foreach (var message in messages)
                {
                    var s = message.Body.Split(" ").ToList();
                
                    var match = botCommandRegex.Match(s[0]);

                    if (!match.Success) continue;
                
                    var command = match.Groups[1].Value.ToLower();

                    if (funcs.ContainsKey(command))
                    {
                        var task = new Command(message, funcs[command], s.Skip(1).ToList());
                        requestQueue.Enqueue(task);
                    }
                    Console.WriteLine("Request accepted");
                }
            }
        }
    }
}