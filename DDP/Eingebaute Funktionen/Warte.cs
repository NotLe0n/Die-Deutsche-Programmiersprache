using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DDP.Eingebaute_Funktionen
{
    class Warte : IAufrufbar
    {
        public int Arity => 1;

        public object Aufrufen(Interpreter interpreter, List<object> argumente)
        {
            Thread.Sleep((int)((double)argumente[0] * 1000));
            return null;
        }
    }
}
