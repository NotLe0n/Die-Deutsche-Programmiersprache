using System;

namespace DDP
{
    class ParseFehler : Exception
    {
        public readonly Symbol symbol;

        public ParseFehler(Symbol symbol, string message) : base(message)
        {
            this.symbol = symbol;
        }
    }
}
