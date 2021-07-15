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

        public int Arity => declaration.param.Count;

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
                System.Type returntype;
                switch (declaration.type.type)
                {
                    case TokenType.ZAHL:
                        returntype = typeof(int);
                        break;
                    case TokenType.FLIEßKOMMAZAHL:
                        returntype = typeof(double);
                        break;
                    case TokenType.ZEICHENKETTE:
                        returntype = typeof(string);
                        break;
                    case TokenType.ZEICHEN:
                        returntype = typeof(char);
                        break;
                    case TokenType.BOOLEAN:
                        returntype = typeof(bool);
                        break;
                    default:
                        throw new RuntimeError(declaration.name, "invalid type");
                }
                if (returnValue.value.GetType() == returntype)
                {
                    return returnValue.value;
                }
                else
                {
                    throw new RuntimeError(declaration.name, "wrong return type");
                }
            }

            return null;
        }
    }
}
