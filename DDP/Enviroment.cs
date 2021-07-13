using System.Collections.Generic;

namespace DDP
{
    public class Environment
    {
        public readonly Environment enclosing;
        private readonly Dictionary<string, object> values = new();

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public object Get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values.Get(name.lexeme);
            }

            if (enclosing != null)
            {
                return enclosing.Get(name);
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void Assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values.Put(name.lexeme, value);
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        public void Define(string name, object value)
        {
            values.Put(name, value);
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
            return Ancestor(distance).values.Get(name);
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).values.Put(name.lexeme, value);
        }

        public override string ToString()
        {
            string result = values.ToString();
            if (enclosing != null)
            {
                result += " -> " + enclosing.ToString();
            }

            return result;
        }
    }
}
