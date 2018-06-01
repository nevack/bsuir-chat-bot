using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public enum ProviderState
    {
        Loaded,
        Unloaded,
        Unloadable
    }

    /// <summary>
    /// Abstract class providing interaction between Bot and Providers
    /// </summary>
    public abstract class VkBotProvider
    {
        public Dictionary<string, string> Functions { get; protected set; }

        public ProviderState State { get; set; } = ProviderState.Unloaded;
        
        public string GetAllHelp()
        {
            var help = new StringBuilder($"[{GetType().Name}]\n");

            foreach (var function in Functions)
            {
                help.AppendLine("/" + function.Value);
            }

            return help.ToString();
        }

        /// <summary>
        /// Logs the message before hadling it
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public MessagesSendParams Handle(VkNet.Model.Message command)
        {
            Log.Information($"[ {GetType().Name} ]: called '{command.Body}' by https://vk.com/id{command.FromId}");
            
            if (State == ProviderState.Unloaded)
                throw new KeyNotFoundException($"{GetType().Name} is not loaded");

            return _handle(command);
        }

        /// <summary>
        /// This function handles execution of a command
        /// </summary>
        /// <param name="command">Message to be handled</param>
        /// <returns>Response message</returns>
        protected abstract MessagesSendParams _handle(VkNet.Model.Message command);
    }
}