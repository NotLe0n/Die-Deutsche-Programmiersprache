using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Lese : IAufrufbar
    {
        public int Arity => 0;

        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            return (char)Console.Read();
        }

        public override string ToString() => "<native fn>";
    }
}
