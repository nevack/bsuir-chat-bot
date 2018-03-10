using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    class Program
    {
        private const int NumberOfWorkerThreads = 4;
        
        static void Main(string[] args)
        {
//            var builder = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("botconfig.json");
//
//            var configuration = builder.Build();
//
//            Console.WriteLine($"{configuration["appid"]}");
//            Console.WriteLine($"{configuration["login"]}");
//            Console.WriteLine($"{configuration["password"]}");
//            Console.WriteLine($"{configuration["accesstoken"]}");
//            Console.WriteLine($"{configuration["shortenerapikey"]}");
//            Console.WriteLine("Press a key...");
//            Console.ReadKey();
//            
//            var api = new VkApi();
//	
//            api.Authorize(new ApiAuthParams
//            {
//                ApplicationId = ulong.Parse(configuration["appid"]),
//                Login = configuration["login"],
//                Password = configuration["password"],
//                Settings = Settings.All
//            });
//            
//            Console.WriteLine(api.Token);
//
//            var n = 0;
//            try
//            {
//                while (!Console.KeyAvailable)
//                {
//                    var r = api.Groups.Get(new GroupsGetParams());
//                    n++;
//                }
//
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.StackTrace);
//                Console.WriteLine($"N = {n}");
//            }

            var funcs = new Dictionary<string, Func<List<string>, string>>();

            var quote = new QuoteProvider("Fuhrer.json");
            var ping = new PingProvider();
            var wait = new WaitProvider();

            foreach (var func in quote.Functions)
            {
                funcs.Add(func.Key, func.Value);
            }            
            
            foreach (var func in ping.Functions)
            {
                funcs.Add(func.Key, func.Value);
            }
            
            foreach (var func in wait.Functions)
            {
                funcs.Add(func.Key, func.Value);
            }
            
//            var jsons = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.json").ToList();
//            jsons.ForEach(Console.WriteLine);
            
            var requestQueue = new Queue<Worker.Task>();
            var returnQueue = new Queue<string>();
            var workersList = new List<Thread>();
            
            for (int i = 0; i < NumberOfWorkerThreads; i++)
            {
                var worker = new Worker(requestQueue, returnQueue);
                var workerThread = new Thread(worker.Work);
                workersList.Add(workerThread);
                workerThread.Start();
            }
            
            string x;
            while (!string.IsNullOrEmpty(x = Console.ReadLine()))
            {
                var s = x.Split(" ").ToList();
                var task = new Worker.Task(funcs[s[0]], s.Skip(1).ToList());
                requestQueue.Enqueue(task);
                
                Console.WriteLine("Request accepted");
            }

            while (returnQueue.Count != 0)
            {
                Console.WriteLine(returnQueue.Dequeue());
            }

            Worker.Kill = true;
//            while (int.TryParse(Console.ReadLine(), out var x))
//            {
//                try
//                {
//                    Console.WriteLine(quote[x].Text);
//                }
//                catch (ArgumentOutOfRangeException e)
//                {
//                    Console.WriteLine("Столько цитат ещё не добавлено!");
//                }
//            }
            
            Console.ReadKey();
        }
    }
}