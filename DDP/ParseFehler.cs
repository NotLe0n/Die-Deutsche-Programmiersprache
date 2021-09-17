using System;

namespace DDP
{
    internal class ParseFehler : Exception
    {
        public readonly Symbol symbol;

        public ParseFehler(Symbol symbol, string message) : base(message)
        {
            this.symbol = symbol;
        }
    }
}
