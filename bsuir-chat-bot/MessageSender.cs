﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using VkNet;
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
            while (!Kill)
            {
                if (_outputQueue.TryDequeue(out var mess))
                {
                    _api.Messages.Send(new MessagesSendParams
                    {
                        PeerId = mess.InputMessage.ChatId == null?mess.InputMessage.FromId:2000000000+mess.InputMessage.ChatId,
                        Message = mess.FuncOutput
                    });
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}