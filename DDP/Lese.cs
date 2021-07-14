using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDP
{
    class Lese : ICallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return Console.ReadLine();
        }

        public override string ToString() => "<native fn>";
    }
}
