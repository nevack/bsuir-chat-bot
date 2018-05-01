using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;

namespace bsuir_chat_bot
{
    public class MathProvider: IBotProvider
    {
        public Dictionary<string, Func<List<string>, string>> Functions { get; }

        public MathProvider()
        {
            var parameters = new Dictionary<string, object>()
            {
                ["Pi"] = Math.PI,
                ["pi"] = Math.PI,
                ["E"] = Math.E,
                ["e"] = Math.E
            };
            
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"calc", list =>
                    {
                        var expr = new Expression(string.Join(' ', list)) {Parameters = parameters};



                        void Repeat(string name, FunctionArgs args)
                        {
                            if (name == "Repeat")
                            {
                                var ex = args.Parameters[0];
                                ex.EvaluateFunction += Repeat;

                                args.Result = string.Concat(Enumerable.Repeat(ex.Evaluate().ToString(),
                                    (int) args.Parameters[1].Evaluate()));
                            }
                        }

//                        expr.EvaluateFunction += delegate(string name, FunctionArgs args)
//                        {
//                            if (name == "Repeat")
//                                args.Result = string.Concat(Enumerable.Repeat(args.Parameters[0].Evaluate().ToString(),
//                                    (int) args.Parameters[1].Evaluate()));
//                        };
                        
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
                        return s;
                    }
                }
            };
        }
    }
}