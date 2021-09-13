using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Mathematik
    {
        public class Min : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                Type t0 = argumente[0].GetType();
                Type t1 = argumente[1].GetType();

                object a0 = Convert.ChangeType(argumente[0], t0);
                object a1 = Convert.ChangeType(argumente[1], t1);

                return a0 switch
                {
                    int i when a1 is int i1 => Math.Min(i, i1),
                    double d when a1 is double d1 => Math.Min(d, d1),
                    int i2 when a1 is double d2 => Math.Min(i2, d2),
                    double d3 when a1 is int i3 => Math.Min(d3, i3),
                    _ => throw new Laufzeitfehler(null, Fehlermeldungen.opOnlyNum)
                };
            }
        }

        public class Max : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                Type t0 = argumente[0].GetType();
                Type t1 = argumente[1].GetType();

                object a0 = Convert.ChangeType(argumente[0], t0);
                object a1 = Convert.ChangeType(argumente[1], t1);

                return a0 switch
                {
                    int i when a1 is int i1 => Math.Max(i, i1),
                    double d when a1 is double d1 => Math.Max(d, d1),
                    int i2 when a1 is double d2 => Math.Max(i2, d2),
                    double d3 when a1 is int i3 => Math.Max(d3, i3),
                    _ => throw new Laufzeitfehler(null, Fehlermeldungen.opOnlyNum)
                };
            }
        }

        public class Clamp : IAufrufbar
        {
            public int Arity => 3;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                Type t0 = argumente[0].GetType();
                Type t1 = argumente[1].GetType();
                Type t2 = argumente[2].GetType();

                object a0 = Convert.ChangeType(argumente[0], t0);
                object a1 = Convert.ChangeType(argumente[1], t1);
                object a2 = Convert.ChangeType(argumente[2], t2);

                try
                {
                    return a0 switch
                    {
                        int i when a1 is int i1 && a2 is int => Math.Clamp(i, i1, (int)a2),
                        double d when a1 is double d1 && a2 is double => Math.Clamp(d, d1, (double)a2),
                        int i2 when a1 is double d2 && a2 is int => Math.Clamp(i2, d2, (int)a2),
                        int i2 when a1 is double d2 && a2 is double => Math.Clamp(i2, d2, (double)a2),
                        double d3 when a1 is int i3 && a2 is int => Math.Clamp(d3, i3, (int)a2),
                        double d3 when a1 is int i3 && a2 is double => Math.Clamp(d3, i3, (double)a2),
                        _ => throw new Laufzeitfehler(null, Fehlermeldungen.opOnlyNum)
                    };
                }
                catch { throw new Laufzeitfehler(null, "min war größer als max!"); }
            }
        }

        public class Trunkiert : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is double d) return Math.Truncate(d);

                throw new Laufzeitfehler(null, "man kann nur Kommazahlen trunkieren!");
            }
        }

        public class Rund : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is double d && argumente[1] is int i) return Math.Round(d, i);

                throw new Laufzeitfehler(null, "die funktion Rund() nimmt eine Kommazahl als ersten argument und eine Zahl als zweiten!");
            }
        }

        public class Decke : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is double d) return Math.Ceiling(d);

                throw new Laufzeitfehler(null, "man kann nur Kommazahlen Decke!");
            }
        }

        public class Boden : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is double d) return Math.Floor(d);
                
                throw new Laufzeitfehler(null, "man kann nur Kommazahlen Boden!");
            }
        }
    }
}
