using System.Collections.Generic;

namespace DDP
{
    interface ICallable
    {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
