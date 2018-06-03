using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NCalc;
using VkNet.Model.RequestParams;

namespace bsuir_chat_bot
{ 
    public class MathProvider: VkBotProvider
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>
        {
            ["Pi"] = Math.PI,
            ["pi"] = Math.PI,
            ["E"] = Math.E,
            ["e"] = Math.E
        };

        public MathProvider()
        {
            Functions = new Dictionary<string, string>
            {
                {"v", "v someexpr - evaluate expression.\n" +
                      " Available expressions:\n" +
                      " · Pow(x,y) - x in power of y.\n" +
                      " · Sin(x) - sinus of x (x in radians)\n" +
                      " · Cos(x) - cosinus of x\n" +
                      " · Abs(x) - absolute value of x\n" +
                      " · Fact(x) - factorial of x (must be in range [0..1491])\n" +
                      " · Repeat(what, times) - repeat 'what' expression\n"
                }
            };
        }

        protected override MessagesSendParams _handle(VkNet.Model.Message command)
        {
            var (_, args) = command.ParseFunc();
            var expr = new Expression(string.Join(' ', args)) {Parameters = _parameters};

            void Repeat(string name, FunctionArgs argz)
            {
                if (name.ToLowerInvariant() == "repeat")
                {
                    var ex = argz.Parameters[0];
                    ex.EvaluateFunction += Repeat;
                    
                    var what = ex.Evaluate().ToString();
                    var times = Math.Min((int) argz.Parameters[1].Evaluate(), 4096 / what.Length + 1);
                    
                    var o = new StringBuilder(times * what.Length);
                    for (var i = 0; i < times; i++)
                    {
                        o.Append(what);
                    }

                    argz.Result = o.ToString();
                }
            }
            
            void Factorial(string name, FunctionArgs argz)
            {
                if (name.ToLowerInvariant() == "fact")
                {
                    var fact = (int) argz.Parameters[0].Evaluate();
                    
                    if (fact < 0 || fact > 1491) 
                        throw new ArgumentOutOfRangeException(nameof(fact), "Must be positive integer [0..1491].");
                    
                    BigInteger result = 1;
                    
                    for (var i = 2; i <= fact; i++)
                    {
                        result *= i;
                    }

                    argz.Result = result;
                }
            }

            expr.EvaluateFunction += Repeat;
            expr.EvaluateFunction += Factorial;

            var message = "Error! ";
            try
            {
                message = "Out[0] = " + expr.Evaluate();
            }
            catch (Exception e)
            {
                message += e.Message;
            }
            
            return new MessagesSendParams
            {
                PeerId = command.GetPeerId(),
                Message =  message
            };
        }
    }
}