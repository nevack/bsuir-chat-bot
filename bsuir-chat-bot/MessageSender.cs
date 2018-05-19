using System;
using System.Threading;
using VkNet.Exception;

namespace bsuir_chat_bot
{
    public class MessageSender
    {
        private const int ChecksPerSecond = 100;
        private readonly Bot _bot;
        private int _millisecondsTimeout;

        internal MessageSender(Bot bot)
        {
            _bot = bot;
        }

        public void Work()
        {
            _millisecondsTimeout = 0;

            while (_bot.BotState != Bot.State.Stoped)
            {
                if (_bot.BotState == Bot.State.Running && _bot.Responses.TryDequeue(out var messageSend))
                {
                    try
                    {
                        _bot.Api.Messages.Send(messageSend);

                        if (_millisecondsTimeout > 200) _millisecondsTimeout /= 2;
                    }
                    catch (CaptchaNeededException)
                    {    
                        Thread.Sleep(60 /*seconds*/ * 1000);
                        _bot.Responses.Enqueue(messageSend);
                    }
                    catch (Exception e)
                    {
                        _millisecondsTimeout *= _millisecondsTimeout < 6_400 
                            ? 2 
                            : throw e;
                        _bot.Responses.Enqueue(messageSend);
                    }
                    
                    Console.WriteLine($"{DateTime.Now:hh\\:mm\\:ss\\.fff} [ {"Bot message sender".PadLeft(20)} ]: Sent response '{messageSend.Message.Truncate(32)}'");
                    Thread.Sleep(_millisecondsTimeout);
                }
                else
                    Thread.Sleep(1000 / ChecksPerSecond);
            }
        }
    }
}