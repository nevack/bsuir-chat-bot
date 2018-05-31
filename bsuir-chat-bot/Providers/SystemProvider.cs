using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{   
    public class SystemProvider : VkBotProvider
    {
        private readonly Bot _bot;

        internal SystemProvider(Bot bot)
        {
            State = ProviderState.Unloadable;
            _bot = bot;
            Functions = new Dictionary<string, string>
            {
                {
                    "stop", "stop - stop the bot"
                },
                {
                    "sleep", "sleep - make bot sleep"
                },
                {
                    "wakeup", "wakeup - it's september"
                },
                {
                    "uptime", "uptime - get bot time running"
                },
                {
                    "load", "load - load a module"
                },
                {
                    "unload", "unload - unload a module"
                }
            };
        }

        private void Stop()
        {
            var t = new Thread(() =>
            {
                Thread.Sleep(1000);
                _bot.BotState = Bot.State.Stoped;
            });
            t.Start();
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

        private string LoadModule(IReadOnlyList<string> names)
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
        
        private string UnloadModule(IReadOnlyList<string> names)
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
        
        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            if (command.FromId.HasValue && !_bot.Admins.Contains(command.FromId.Value))
            {
                throw new AccessViolationException("Permission denied!");
            }
            
            var (func, args) = command.ParseFunc();

            string message;
            
            switch (func.ToLowerInvariant())
            {
                case "sleep":
                    Sleep();
                    message = "zZzzzzZzzz";
                    break;
                
                case "wakeup":
                    WakeUp();
                    message = "Ready!";
                    break;
                
                case "stop":
                    Stop();
                    message = "Bye";
                    break;
                
                case "uptime":
                    message = GetUptime();
                    break;
                
                case "load":
                    message = LoadModule(args);
                    break;
                
                case "unload":
                    message = UnloadModule(args);
                    break;
                
                default:
                    message = "Not implemented yet";
                    break;
            }

            var param = new MessagesSendParams
            {
                Message = message,
                PeerId = command.GetPeerId()
            };

            return param;
        }
    }
}