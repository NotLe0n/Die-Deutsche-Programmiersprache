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
                if (arguments[i].GetType() != SymbolTypZuTyp(declaration.argumente[i].typ.typ))
                {
                    throw new Laufzeitfehler(declaration.name, "Falscher Argument typ!");
                }

                environment.Define(declaration.argumente[i].arg.lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(declaration.körper, environment);
            }
            catch (Rückgabe returnValue)
            {
                if (returnValue == null || returnValue.wert.GetType() != SymbolTypZuTyp(declaration.typ.typ))
                    throw new Laufzeitfehler(declaration.name, Fehlermeldungen.returnTypeWrong);

                return returnValue.wert;
            }

            return null;
        }

        private System.Type SymbolTypZuTyp(SymbolTyp symbolTyp)
        {
            return symbolTyp switch
            {
                SymbolTyp.ZAHL => typeof(int),
                SymbolTyp.KOMMAZAHL => typeof(double),
                SymbolTyp.TEXT => typeof(string),
                SymbolTyp.BUCHSTABE => typeof(char),
                SymbolTyp.BOOLEAN => typeof(bool),
                SymbolTyp.ZAHLEN => typeof(int[]),
                SymbolTyp.KOMMAZAHLEN => typeof(double[]),
                SymbolTyp.TEXTE => typeof(string[]),
                SymbolTyp.BUCHSTABEN => typeof(char[]),
                SymbolTyp.BOOLEANS => typeof(bool[]),
                _ => throw new Laufzeitfehler(declaration.name, "Ungültiger Typ!"),
            };
        }
    }
}
