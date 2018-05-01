using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VkNet;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    
    public class MessageSender
    {

        private static ConcurrentQueue<Response> _outputQueue;
        private static VkApi _api;
        public static bool Kill = false;

        internal MessageSender(ConcurrentQueue<Response> outputQueue, VkApi api)
        {
            _api = api;
            _outputQueue = outputQueue;
        }

        public void Work()
        {
            var sleeptime = 200;
            const int checksPerSecond = 100;
            while (!Kill)
            {
                if (_outputQueue.TryDequeue(out var mess))
                {
                    try
                    {
                        _api.Messages.Send(new MessagesSendParams
                        {
                            PeerId = mess.InputMessage.ChatId?.ToPeerId() ?? mess.InputMessage.FromId,
                            Message = $"{mess.FuncOutput} [id{mess.InputMessage.FromId}|©]"
                        });

                        if (sleeptime > 200) sleeptime /= 2;
                    }
                    catch (CaptchaNeededException)
                    {    
                        Thread.Sleep(60 /*seconds*/ * 1000);
                        _outputQueue.Enqueue(mess);
                    }
                    catch (Exception e)
                    {
                        sleeptime *= sleeptime < 6_400 
                            ? 2 
                            : throw e;
                        _outputQueue.Enqueue(mess);
                    }
                    
                    Console.WriteLine($"Sleep: {sleeptime}ms");
                    Thread.Sleep(sleeptime);
                }
                else
                    Thread.Sleep(1000 / checksPerSecond);
            }
        }
    }
}