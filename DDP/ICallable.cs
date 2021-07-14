using System.Collections.Generic;

namespace DDP
{
    interface ICallable
    {
        int Arity { get; }

        object Call(Interpreter interpreter, List<object> arguments);
    }
}
