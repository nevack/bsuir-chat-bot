using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace bsuir_chat_bot
{
    public class SystemProvider : IBotProvider
    {
        private readonly Bot _bot;
        public Dictionary<string, Func<List<string>, string>> Functions { get; }

        internal SystemProvider(Bot bot)
        {
            _bot = bot;
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {
                    "stop", list =>
                    {
                        var t = new Thread(() =>
                        {
                            Thread.Sleep(1000);
                            bot.BotState = Bot.State.Stoped;
                        });
                        t.Start();
                        return "Bye";
                    }
                },
                {
                    "sleep", list => { 
                        Sleep();
                        return "zZzzzzZzzz";
                    }
                },
                {
                    "wakeup", list => { 
                        WakeUp();
                        return "Ready!";
                    }
                },
                {
                    "uptime", list => GetUptime()
                },
                {
                    "load", LoadModule
                },
                {
                    "unload", UnloadModule
                }
            };
        }

        private void Sleep()
        {
            _bot.BotState = Bot.State.Sleep;
        }

        private void WakeUp()
        {
            _bot.BotState = Bot.State.Running;
        }
        
        private string GetUptime() => _bot.GetUptime();

        private string LoadModule(List<string> names)
        {
            var s = new StringBuilder();
            
            foreach (var name in names)
            {
                if (!_bot.Providers.ContainsKey(name))
                {
                    s.AppendLine($"Module \"{name} not found");
                    continue;
                }

                foreach (var function in _bot.Providers[name].Functions)
                {
                    _bot.Functions.Add(function.Key, function.Value);
                }

                s.AppendLine($"Module \"{name} loaded");
            }

            return s.ToString();
        }
        
        private string UnloadModule(List<string> names)
        {
            var s = new StringBuilder();
            
            foreach (var name in names)
            {
                if (!_bot.Providers.ContainsKey(name))
                {
                    s.AppendLine($"Module \"{name} not found");
                    continue;
                }

                foreach (var function in _bot.Providers[name].Functions)
                {
                    _bot.Functions.Remove(function.Key);
                }

                s.AppendLine($"Module \"{name} unloaded");
            }

            return s.ToString();
        }
    }
}