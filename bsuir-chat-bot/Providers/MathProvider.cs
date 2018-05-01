using System;
using System.Collections.Generic;
using NCalc;

namespace bsuir_chat_bot
{
    public class MathProvider: IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }
        
        public MathProvider()
        {
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"calc", list =>
                    {
                        var expr = new Expression(string.Join(' ', list));
                        expr.Parameters["Pi"] = Math.PI;
                        expr.Parameters["pi"] = Math.PI;
                        expr.Parameters["E"] = Math.E;
                        expr.Parameters["e"] = Math.E;
                        return expr.Evaluate().ToString();
                    }
                }
            };
        }
    }
}