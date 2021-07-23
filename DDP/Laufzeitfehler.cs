using System;

namespace DDP
{
    public class Laufzeitfehler : Exception
    {
        public readonly Token token;

        public Laufzeitfehler(Token token, string nachricht) : base(nachricht)
        {
            this.token = token;
        }
    }
}
