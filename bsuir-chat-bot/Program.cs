using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace bsuir_chat_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("botconfig.json");

            var configuration = builder.Build();

            Console.WriteLine($"{configuration["appid"]}");
            Console.WriteLine($"{configuration["login"]}");
            Console.WriteLine($"{configuration["password"]}");
            Console.WriteLine($"{configuration["accesstoken"]}");
            Console.WriteLine($"{configuration["shortenerapikey"]}");
            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }
    }
}