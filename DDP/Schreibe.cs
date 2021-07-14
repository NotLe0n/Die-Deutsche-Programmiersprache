using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDP
{
    class Schreibe : ICallable
    {
        public int Arity => 1;

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Console.WriteLine(arguments[0]);
            return null;
        }

        public override string ToString() => "<native fn>";
    }
}
