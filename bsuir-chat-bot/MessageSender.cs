using System;
using System.Threading;
using Serilog;
using VkNet.Exception;

namespace bsuir_chat_bot
{
    /// <summary>
    /// Singleton class sending out messages
    /// </summary>
    public class MessageSender
    {
        private const int ChecksPerSecond = 100;
        private readonly Bot _bot;

        internal MessageSender(Bot bot)
        {
            _bot = bot;
        }

        /// <summary>
        /// Start sending messages
        /// </summary>
        public void Work()
        {
            var timeout = 200;

            while (_bot.BotState != Bot.State.Stoped)
            {
                if (_bot.BotState == Bot.State.Running && _bot.Responses.TryDequeue(out var messageSend))
                {
                    try
                    {
                        messageSend.Message = messageSend.Message.Truncate(4000);
                        _bot.Api.Messages.Send(messageSend);

                        if (timeout > 200) timeout /= 2;
                    }
                    catch (CaptchaNeededException e)
                    {    
                        Log.Error(e, "Sleeping a minute");
                        Thread.Sleep(60 /*seconds*/ * 1000);
                        _bot.Responses.Enqueue(messageSend);
                    }
                    catch (Exception e)
                    {
                        timeout *= timeout < 6_400 
                            ? 2 
                            : throw e;
                        _bot.Responses.Enqueue(messageSend);
                    }

                    Log.Debug($"{DateTime.Now:hh\\:mm\\:ss\\.fff} [ Message sender ]: Sent response " +
                              $"'{messageSend.Message.Replace(Environment.NewLine, "").Truncate(32)}'");
                    Thread.Sleep(timeout);
                }
                else
                    Thread.Sleep(1000 / ChecksPerSecond);
            }
        }
    }
}