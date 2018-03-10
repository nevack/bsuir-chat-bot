using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    class Program
    {
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

            var quote = new QuoteProvider("Fuhrer.json");
            while (int.TryParse(Console.ReadLine(), out var x))
            {
                try
                {
                    Console.WriteLine(quote[x].Text);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("Столько цитат ещё не добавлено!");
                }
            }
            
            
            
            Console.ReadKey();
        }
    }
}