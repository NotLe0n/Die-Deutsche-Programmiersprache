using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Schreibe : IAufrufbar
    {
        public int Arity => 1;

        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            Console.Write(arguments[0]);
            return null;
        }

        public override string ToString() => "<native fn>";
    }
}
