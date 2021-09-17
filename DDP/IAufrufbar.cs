using System.Collections.Generic;

namespace DDP
{
    internal interface IAufrufbar
    {
        int Arity { get; }

        object Aufrufen(Interpreter interpreter, List<object> argumente);
    }
}
