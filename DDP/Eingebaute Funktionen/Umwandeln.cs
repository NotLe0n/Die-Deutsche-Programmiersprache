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
                    throw new RuntimeError(null, ErrorMessages.castInvalid(Extentions.Stringify(arguments[0]), "eine Zahl"));
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
                    throw new RuntimeError(null, ErrorMessages.castInvalid(Extentions.Stringify(arguments[0]), "eine Fließkommazahl"));
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
                    throw new RuntimeError(null, ErrorMessages.castInvalid(Extentions.Stringify(arguments[0]), "einen Zeichen"));
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
                    throw new RuntimeError(null, ErrorMessages.castInvalid(Extentions.Stringify(arguments[0]), "eine Zeichenkette"));
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
                    throw new RuntimeError(null, ErrorMessages.castInvalid(Extentions.Stringify(arguments[0]), "einen Boolean"));
                }
            }
        }
    }
}
