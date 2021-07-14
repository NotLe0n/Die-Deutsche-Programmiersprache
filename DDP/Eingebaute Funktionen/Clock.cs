using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DDP.Eingebaute_Funktionen
{
    class Clock : ICallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalMilliseconds / 1000.0;
        }

        public override string ToString() => "<native fn>";
    }
}
