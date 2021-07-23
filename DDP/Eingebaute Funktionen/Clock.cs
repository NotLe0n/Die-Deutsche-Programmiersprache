using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DDP.Eingebaute_Funktionen
{
    class Clock : IAufrufbar
    {
        public int Arity => 0;

        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            return (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds / 1000.0;
        }

        public override string ToString() => "<native fn>";
    }
}
