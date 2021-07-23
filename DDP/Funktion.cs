using System.Collections.Generic;

namespace DDP
{
    class Funktion : IAufrufbar
    {
        private readonly Anweisung.Funktion declaration;
        private readonly Environment closure;

        public Funktion(Anweisung.Funktion declaration, Environment closure)
        {
            this.closure = closure;

            this.declaration = declaration;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }

        public int Arity => declaration.argumente.Count;
        
        public object Aufrufen(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.argumente.Count; i++)
            {
                environment.Define(declaration.argumente[i].lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.körper, environment);
            }
            catch (Rückgabe returnValue)
            {
                if (returnValue == null)
                    throw new Laufzeitfehler(declaration.name, Fehlermeldungen.returnTypeWrong);

                System.Type returntype;
                switch (declaration.typ.typ)
                {
                    case SymbolTyp.ZAHL:
                        returntype = typeof(int);
                        break;
                    case SymbolTyp.KOMMAZAHL:
                        returntype = typeof(double);
                        break;
                    case SymbolTyp.ZEICHENKETTE:
                        returntype = typeof(string);
                        break;
                    case SymbolTyp.ZEICHEN:
                        returntype = typeof(char);
                        break;
                    case SymbolTyp.BOOLEAN:
                        returntype = typeof(bool);
                        break;
                    default:
                        throw new Laufzeitfehler(declaration.name, Fehlermeldungen.returnTypeInvalid);
                }
                if (returnValue.wert.GetType() == returntype)
                {
                    return returnValue.wert;
                }
                else
                {
                    throw new Laufzeitfehler(declaration.name, Fehlermeldungen.returnTypeWrong);
                }
            }

            return null;
        }
    }
}
