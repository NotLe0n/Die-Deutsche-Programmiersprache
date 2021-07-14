using System;
using System.Collections.Generic;

namespace DDP.Eingebaute_Funktionen
{
    class LeseZeile : ICallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return Console.ReadLine();
        }

        public override string ToString() => "<native fn>";
    }
}
