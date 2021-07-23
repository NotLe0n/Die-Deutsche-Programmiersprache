namespace DDP
{
    public class Symbol
    {
        public SymbolTyp typ;
        public readonly string lexeme;
        public readonly object wert;
        public readonly int zeile;
        public readonly int position;

        public Symbol(SymbolTyp typ, string lexeme, object wert, int zeile, int position)
        {
            this.typ = typ;
            this.lexeme = lexeme;
            this.wert = wert;
            this.zeile = zeile;
            this.position = position;
        }

        public override string ToString() => $"type: {typ}{(!string.IsNullOrEmpty(lexeme) ? $"; lex: {lexeme}" : "")}{(wert != null ? $"; lit: {wert}" : "")}";
    }
}
