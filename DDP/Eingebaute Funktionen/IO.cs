using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DDP.Eingebaute_Funktionen
{
    class IO
    {
        public class ExistiertDatei : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string)
                    throw new Laufzeitfehler(null, "existiertDatei() nimmt nur einen Text");

                return File.Exists(argumente[0] as string);
            }
        }

        public class LeseDatei : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string)
                    throw new Laufzeitfehler(null, "leseDatei() nimmt nur einen Text");

                if (!File.Exists(argumente[0] as string))
                    throw new Laufzeitfehler(null, "Datei existiert nicht!");

                return File.ReadAllText(argumente[0] as string);
            }
        }

        public class LeseBytes : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string)
                    throw new Laufzeitfehler(null, "leseBytes() nimmt nur einen Text");

                if (!File.Exists(argumente[0] as string))
                    throw new Laufzeitfehler(null, "Datei existiert nicht!");

                return Array.ConvertAll(File.ReadAllBytes(argumente[0] as string), Convert.ToInt32);
            }
        }

        public class SchreibeDatei : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string && argumente[1] is not string)
                    throw new Laufzeitfehler(null, "schreibeDatei() nimmt nur Text, Text");

                File.WriteAllText(argumente[0] as string, argumente[1] as string);
                return null;
            }
        }

        public class SchreibeBytes : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string && argumente[1] is not int[])
                    throw new Laufzeitfehler(null, "schreibeBytes() nimmt nur Text, Zahlen");

                var intArray = (int[])argumente[1];
                byte[] result = intArray.Select(i => (byte)i).ToArray();

                File.WriteAllBytes(argumente[0] as string, result);
                return null;
            }
        }

        public class BearbeiteDatei : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string && argumente[1] is not string)
                    throw new Laufzeitfehler(null, "bearbeiteDatei() nimmt nur Text, Text");

                if (!File.Exists(argumente[0] as string))
                    throw new Laufzeitfehler(null, "Datei existiert nicht!");

                File.AppendAllText(argumente[0] as string, argumente[1] as string);
                return null;
            }
        }

        public class BearbeiteBytes : IAufrufbar
        {
            public int Arity => 2;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is not string && argumente[1] is not int[])
                    throw new Laufzeitfehler(null, "bearbeiteBytes() nimmt nur Text, Zahlen");

                if (!File.Exists(argumente[0] as string))
                    throw new Laufzeitfehler(null, "Datei existiert nicht!");

                // concatinate 2 arrays
                var orig = File.ReadAllBytes(argumente[0] as string);
                var bytes = (argumente[1] as int[]).Select(i => (byte)i).ToArray();
                var concatinated = new byte[orig.Length + bytes.Length];

                orig.CopyTo(concatinated, 0);
                bytes.CopyTo(concatinated, orig.Length);

                File.WriteAllBytes(argumente[0] as string, concatinated);
                return null;
            }
        }
    }
}
