using System;

namespace DDP
{
    class ParseError : Exception
    {
        public readonly Token token;

        public ParseError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
