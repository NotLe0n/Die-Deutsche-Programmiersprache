using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Stringmanipulation
    {
        public class Zuschneiden : IAufrufbar
        {
            public int Arity => 3;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is int a1 && argumente[2] is int a2)
                    return s.Substring(a1, a2);

                throw new Laufzeitfehler(null, "Zuschneiden() argument nimmt nur string, int, int");
            }
        }

        public class Spalten : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is string a1)
                    return s.Split(a1);

                if (argumente[0] is string s1 && argumente[1] is char ca1)
                    return s1.Split(ca1);

                throw new Laufzeitfehler(null, "Spalten() argument nimmt nur string, string/char");
            }
        }

        public class Ersetzten : IAufrufbar
        {
            public int Arity => 3;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is string a1 && argumente[2] is string a2)
                    return s.Replace(a1, a2);

                if (argumente[0] is string s1 && argumente[1] is char ca1 && argumente[2] is char ca2)
                    return s1.Replace(ca1, ca2);

                throw new Laufzeitfehler(null, "Ersetzten() argument nimmt nur string, string/char, string/char ");
            }
        }

        public class Entfernen : IAufrufbar
        {
            public int Arity => 3;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is int a1 && argumente[2] is int a2)
                    return s.Remove(a1, a2);

                throw new Laufzeitfehler(null, "Entfernen() argument nimmt nur string, int, int");
            }
        }

        public class Einfügen : IAufrufbar
        {
            public int Arity => 3;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is int a1 && argumente[2] is string a2)
                    return s.Insert(a1, a2);

                throw new Laufzeitfehler(null, "Einfügen() argument nimmt nur string, int, string");
            }
        }

        public class Enthält : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s && argumente[1] is string a1)
                    return s.Contains(a1);

                if (argumente[0] is string s1 && argumente[1] is char ca1)
                    return s1.Contains(ca1);

                throw new Laufzeitfehler(null, "Enthält() argument nimmt nur string, string/char");
            }
        }

        public class Beschneiden : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is string s)
                    return s.Trim();

                throw new Laufzeitfehler(null, "Beschneiden() argument muss ein string sein");
            }
        }
    }
}
