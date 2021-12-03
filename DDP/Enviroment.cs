using System;
using System.Collections.Generic;

namespace DDP
{
    internal class Environment
    {
        public readonly Environment enclosing;
        private readonly Dictionary<string, object> werte = new();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public object Get(Symbol name)
        {
            if (werte.ContainsKey(name.lexeme))
            {
                return werte[name.lexeme];
            }

            if (enclosing != null)
            {
                return enclosing.Get(name);
            }

            throw new Laufzeitfehler(name, Fehlermeldungen.varNotDefined(name.lexeme));
        }

        public void Assign(Symbol name, object value)
        {
            if (werte.ContainsKey(name.lexeme))
            {
                werte[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new Laufzeitfehler(name, Fehlermeldungen.varNotDefined(name.lexeme));
        }

        public void AssignArray(Symbol name, int stelle, object value)
        {
            if (werte.ContainsKey(name.lexeme))
            {
                (werte[name.lexeme] as Array).SetValue(value, stelle);
                return;
            }

            if (enclosing != null)
            {
                enclosing.AssignArray(name, stelle, value);
                return;
            }

            throw new Laufzeitfehler(name, Fehlermeldungen.varNotDefined(name.lexeme));
        }

        public void Define(string name, object value)
        {
            werte[name] = value;
        }

        public Environment Ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing; // [coupled]
            }

            return environment;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).werte[name];
        }

        public void AssignAt(int distance, Symbol name, object value)
        {
            Ancestor(distance).werte[name.lexeme] = value;
        }

        public void AssignArrayAt(int distance, Symbol name, int stelle, object value)
        {
            Array arr = Ancestor(distance).werte[name.lexeme] as Array;
            arr.SetValue(value, stelle);
        }

        public override string ToString()
        {
            string result = werte.ToString();
            if (enclosing != null)
            {
                result += " -> " + enclosing.ToString();
            }

            return result;
        }
    }
}
