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
            Functions = new Dictionary<string, Func<List<string>, string>>
            {
                {"calc", list =>
                    {
                        var expr = new Expression(string.Join(' ', list));
                        expr.Parameters["Pi"] = Math.PI;
                        expr.Parameters["pi"] = Math.PI;
                        expr.Parameters["E"] = Math.E;
                        expr.Parameters["e"] = Math.E;

                        Action<string, FunctionArgs> repeat = null;
                        repeat = delegate(string name, FunctionArgs args)
                        {
                            if (name == "Repeat")
                            {
                                var ex = args.Parameters[0];
                                ex.EvaluateFunction += (ss, a) => repeat(ss, a);
                                
                                args.Result = string.Concat(Enumerable.Repeat(ex.Evaluate().ToString(),
                                    (int) args.Parameters[1].Evaluate()));
                            }
                        };

//                        expr.EvaluateFunction += delegate(string name, FunctionArgs args)
//                        {
//                            if (name == "Repeat")
//                                args.Result = string.Concat(Enumerable.Repeat(args.Parameters[0].Evaluate().ToString(),
//                                    (int) args.Parameters[1].Evaluate()));
//                        };
                        
                        expr.EvaluateFunction += (name, args) => repeat(name, args);

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