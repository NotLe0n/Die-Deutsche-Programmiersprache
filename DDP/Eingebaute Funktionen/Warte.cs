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
            if (argumente[0] is not int or double)
                throw new Laufzeitfehler(null, "warte() argument nimmt nur zahl oder kommazahl");

            var arg = Convert.ToDouble(argumente[0]);

            Thread.Sleep((int)(arg * 1000));
            return null;
        }
    }
}
