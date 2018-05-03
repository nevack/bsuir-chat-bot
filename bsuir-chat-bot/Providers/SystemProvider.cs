using System;
using System.Collections.Generic;
using System.Linq;
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
            if (names.Count == 1 && names[0].ToLower() == "all")
            {
                _bot.LoadAll();
                return "All modules were loaded";
            }
            
            var s = new StringBuilder();
            
            foreach (var name in names.Select(name => name.ToLower()))
            {
                s.Append($"Module \"{name}\" was ");
                s.AppendLine(_bot.LoadModule(name) ? "loaded" : "not loaded");
            }

            return s.ToString();
        }
        
        private string UnloadModule(List<string> names)
        {
            if (names.Count == 1 && names[0].ToLower() == "all")
            {     
                _bot.UnloadAll();

                return "All modules were unloaded";
            }
            
            var s = new StringBuilder();
            
            foreach (var name in names.Select(name => name.ToLower()))
            {
                s.Append($"Module \"{name}\" was ");
                s.AppendLine(_bot.UnloadModule(name) ? "unloaded" : "not unloaded");
            }

            return s.ToString();
        }
    }
}