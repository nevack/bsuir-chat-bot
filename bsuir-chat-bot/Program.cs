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

            Console.WriteLine($"{configuration["appid"]}");
            Console.WriteLine($"{configuration["login"]}");
            Console.WriteLine($"{configuration["password"]}");
            Console.WriteLine($"{configuration["accesstoken"]}");
            Console.WriteLine($"{configuration["shortenerapikey"]}");
            
            var api = new VkApi(logger: null);
	
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
            
            var requestQueue = new ConcurrentQueue<Worker.Task>();
            
            for (int i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new Worker(requestQueue);
                var workerThread = new Thread(worker.Work);
                workerThread.Start();
            }
            
            string x = "kek";
            long timestamp = -1;
            var server = api.Messages.GetLongPollServer();
            
            while (!string.IsNullOrEmpty(x))
            {
                var longpolluri = $"https://{server.Server}?act=a_check&key={server.Key}&ts={timestamp}&wait=25&mode=2&version=2";
                var response = Client.PostAsync($"https://{server.Server}?act=a_check&key={server.Key}&ts={timestamp}&wait=25&mode=2&version=2", null);
                response.Wait();
                var responseString = response.Result.Content.ReadAsStringAsync().Result;
                var responseDict = JsonConvert.DeserializeObject<Dictionary<dynamic, dynamic>>(responseString);
                
                timestamp = responseDict["ts"];

                var messages = VkMessageParser.ParseLongPollMessage(responseString);
                if (messages == null) continue;
                foreach (var message in messages)
                {
                    var s = message.Text.Split(" ").ToList();
                
                    var match = botCommandRegex.Match(s[0]);

                    if (!match.Success) continue;
                
                    var command = match.Groups[1].Value;

                    if (funcs.ContainsKey(command))
                    {
                        var task = new Worker.Task(funcs[command], s.Skip(1).ToList());
                        requestQueue.Enqueue(task);
                    }
                    Console.WriteLine("Request accepted");
                }

            }
            
            Console.ReadKey();
            Worker.Kill = true;
        }
    }
}