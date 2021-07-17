using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Umwandeln
    {
        public class Zahl : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    if (arguments[0] is string || arguments[0] is char)
                    {
                        if (int.TryParse(arguments[0].ToString(), out int result))
                            return result;
                    }
                    return Convert.ToInt32(arguments[0]);
                }
                catch
                {
                    throw new RuntimeError(null, "man kann '" + arguments[0].ToString() + "' nicht in eine Zahl umwandeln");
                }
            }
        }

        public class Fließkommazahl : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    if (arguments[0] is string || arguments[0] is char)
                    {
                        if (double.TryParse(arguments[0].ToString(), out double result))
                            return result;
                    }
                    return (double)Convert.ToDouble(arguments[0]);
                }
                catch
                {
                    throw new RuntimeError(null, "man kann '" + arguments[0].ToString() + "' nicht in eine Fließkommazahl umwandeln");
                }
            }
        }

        public class Zeichen : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    return Convert.ToChar(arguments[0]);
                }
                catch
                {
                    throw new RuntimeError(null, "man kann '" + arguments[0].ToString() + "' nicht in einen Zeichen umwandeln");
                }
            }
        }

        public class Zeichenkette : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    return arguments[0].ToString();
                }
                catch
                {
                    throw new RuntimeError(null, "man kann '" + arguments[0].ToString() + "' nicht in eine Zeichenkette umwandeln");
                }
            }
        }

        public class Boolean : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    if (arguments[0] is string)
                    {
                        if (bool.TryParse(arguments[0].ToString(), out bool result))
                            return result;
                    }
                    return Convert.ToBoolean(arguments[0]);
                }
                catch
                {
                    throw new RuntimeError(null, "man kann '" + arguments[0].ToString() + "' nicht in einen Boolean umwandeln");
                }
            }
        }
    }
}
