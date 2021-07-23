using System;
using System.Collections.Generic;
using System.Globalization;

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
                    if (arguments[0] is string)
                    {
                        if (double.TryParse(Erweiterungen.Stringify(arguments[0]), NumberStyles.Integer, new CultureInfo("de-DE"),  out double result))
                            return (int)result;
                    }
                    return Convert.ToInt32(arguments[0]);
                }
                catch
                {
                    throw new Laufzeitfehler(null, Fehlermeldungen.castInvalid(Erweiterungen.Stringify(arguments[0]), "eine Zahl"));
                }
            }
        }

        public class Kommazahl : ICallable
        {
            public int Arity => 1;

            public object Call(Interpreter interpreter, List<object> arguments)
            {
                try
                {
                    if (arguments[0] is string)
                    {
                        if (double.TryParse(Erweiterungen.Stringify(arguments[0]), out double result))
                            return result;
                    }
                    return (double)Convert.ToDouble(arguments[0]);
                }
                catch
                {
                    throw new Laufzeitfehler(null, Fehlermeldungen.castInvalid(Erweiterungen.Stringify(arguments[0]), "eine Kommazahl"));
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
                    // fick dich hendrik
                    if (arguments[0] is bool)
                    {
                        return (bool)arguments[0] ? 'w' : 'f';
                    }

                    if (arguments[0] is string)
                    {
                        return (arguments[0] as string)[0];
                    }

                    return Convert.ToChar(arguments[0]);
                }
                catch
                {
                    throw new Laufzeitfehler(null, Fehlermeldungen.castInvalid(Erweiterungen.Stringify(arguments[0]), "einen Zeichen"));
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
                    return Erweiterungen.Stringify(arguments[0]);
                }
                catch
                {
                    throw new Laufzeitfehler(null, Fehlermeldungen.castInvalid(Erweiterungen.Stringify(arguments[0]), "eine Zeichenkette"));
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
                        if (Erweiterungen.Stringify(arguments[0]) == "wahr")
                        {
                            return true;
                        }
                        else if (Erweiterungen.Stringify(arguments[0]) == "falsch")
                        {
                            return false;
                        }
                    }
                    return Convert.ToBoolean(arguments[0]);
                }
                catch
                {
                    throw new Laufzeitfehler(null, Fehlermeldungen.castInvalid(Erweiterungen.Stringify(arguments[0]), "einen Boolean"));
                }
            }
        }
    }
}
