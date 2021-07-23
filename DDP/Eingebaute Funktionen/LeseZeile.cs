using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class LeseZeile : IAufrufbar
    {
        public int Arity => 0;

        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            return Console.ReadLine();
        }

        public override string ToString() => "<native fn>";
    }
}
