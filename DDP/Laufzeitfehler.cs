using System;

namespace DDP
{
    internal class Laufzeitfehler : Exception
    {
        public readonly Symbol symbol;

        public Laufzeitfehler(Symbol symbol, string nachricht) : base(nachricht)
        {
            this.symbol = symbol;
        }
    }
}
