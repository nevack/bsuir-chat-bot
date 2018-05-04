using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{
    public class MathProvider: VkBotProvider
    {
        private readonly Dictionary<string, object> _parameters;

        public MathProvider()
        {
            _parameters = new Dictionary<string, object>()
            {
                ["Pi"] = Math.PI,
                ["pi"] = Math.PI,
                ["E"] = Math.E,
                ["e"] = Math.E
            };

            Functions = new Dictionary<string, string>
            {
                {"calc", "calc - evaluate expression" }
            };
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();
            var expr = new Expression(string.Join(' ', args)) {Parameters = _parameters};

            void Repeat(string name, FunctionArgs argz)
            {
                if (name == "Repeat")
                {
                    var ex = argz.Parameters[0];
                    ex.EvaluateFunction += Repeat;

                    argz.Result = string.Concat(Enumerable.Repeat(ex.Evaluate().ToString(),
                        (int) argz.Parameters[1].Evaluate()));
                }
            }
                        
            expr.EvaluateFunction += Repeat;

            var s = "Error! ";
            try
            {
                s = expr.Evaluate().ToString();
            }
            catch (Exception e)
            {
                s += e.Message;
            }
            
            return new MessagesSendParams()
            {
                PeerId = command.GetPeerId(),
                Message =  s
            };
        }
    }
}