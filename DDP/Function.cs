using System.Collections.Generic;

namespace DDP
{
    class Function : ICallable
    {
        private readonly Statement.Function declaration;
        private readonly Environment closure;

        public Function(Statement.Function declaration, Environment closure)
        {
            this.closure = closure;

            this.declaration = declaration;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public int Arity()
        {
            return declaration.param.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.param.Count; i++)
            {
                environment.Define(declaration.param[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }

            return null;
        }
    }
}
