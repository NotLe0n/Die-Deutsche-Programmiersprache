namespace DDP
{
    public class Token
    {
        public TokenType type;
        public readonly string lexeme;
        public readonly object literal;
        public readonly int line;
        public readonly int position;

        public Token(TokenType type, string lexeme, object literal, int line, int position)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
            this.position = position;
        }

        public override string ToString()
        {
            return $"type: {type}{(!string.IsNullOrEmpty(lexeme) ? $"; lex: {lexeme}" : "")}{(literal != null ? $"; lit: {literal}" : "")}";
        }
    }
}
