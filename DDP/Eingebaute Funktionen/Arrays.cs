using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Arrays
    {
        public class Länge : IAufrufbar
        {
            public int Arity => 1;

            public object Aufrufen(Interpreter interpreter, List<object> argumente)
            {
                if (argumente[0] is object[] arr)
                    return arr.Length;

                if (argumente[0] is string s)
                    return s.Length;

                throw new Laufzeitfehler(null, "Länge() argument muss ein string oder ein Array sein");
            }
        }
    }
}
