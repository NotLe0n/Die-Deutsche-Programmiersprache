using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class Lese : ICallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return Console.Read();
        }

        public override string ToString() => "<native fn>";
    }
}
