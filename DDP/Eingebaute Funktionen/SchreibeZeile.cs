using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class SchreibeZeile : IAufrufbar
    {
        public int Arity => 1;

        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            if (arguments[0] != null)
            {
                Console.WriteLine(Erweiterungen.Stringify(arguments[0]));
            }
            return null;
        }

        public override string ToString() => "<native fn>";
    }
}
