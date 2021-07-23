using System.Collections.Generic;

namespace DDP
{
    interface IAufrufbar
    {
        int Arity { get; }

        object Aufrufen(Interpreter interpreter, List<object> argumente);
    }
}
