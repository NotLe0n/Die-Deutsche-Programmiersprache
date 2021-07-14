using System.Collections.Generic;
using System.Globalization;

using static DDP.TokenType;

namespace DDP
{
    class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new();

        private readonly Dictionary<string, TokenType> keywords = new()
        {
            // Artikel
            { "der", DER },
            { "die", DIE },
            { "das", DAS },

            // Typen
            { "Zahl", ZAHL },
            { "Fließkommazahl", FLIEßKOMMAZAHL },
            { "Boolean", BOOLEAN },
            { "Zeichenkette", ZEICHENKETTE },
            { "Zeichen", ZEICHEN },

            // boolean
            { "wahr", WAHR },
            { "falsch", FALSCH },

            // mathematische operatoren
            { "ist", IST },
            { "plus", PLUS },
            { "minus", MINUS },
            { "mal", MAL },
            { "durch", DURCH },
            { "modulo", MODULO },
            { "hoch", HOCH },
            { "wurzel", WURZEL },
            { "ln", LOG },
            { "Betrag", BETRAG }, // oder Absolutwert (wir haben uns um den namen gestritten)

            // bool'sche vergleichs operatoren
            { "gleich", GLEICH },
            { "ungleich", UNGLEICH },
            { "kleiner", KLEINER },
            { "kleiner als, oder gleich", KLEINER_GLEICH },
            { "größer", GRÖßER },
            { "größer als, oder gleich", GRÖßER_GLEICH },
            { "als", ALS },

            // bitweise operatoren
            { "logisch", LOGISCH },
            { "kontra", KONTRA },
            { "um", UM },
            { "bit", BIT },
            { "nach", NACH },
            { "links", LINKS },
            { "rechts", RECHTS },
            { "verschoben", VERSCHOBEN },

            // konstante
            { "Pi", PI },
            { "e", E },
            { "Tau", TAU },

            // logische operatoren
            { "und", UND },
            { "oder", ODER },
            { "nicht", NICHT },

            // verzweigungen
            { "wenn", WENN },
            { "aber", ABER },
            { "dann", DANN },
            { "sonst", SONST },

            // schleifen
            { "für", FÜR },
            { "solange", SOLANGE },
            { "mache", MACHE },
            { "jede", JEDE },
            { "von", VON },
            { "bis", BIS },
            { "schrittgröße", SCHRITTGRÖßE },
            { "mit", MIT },

            // funktionen
            { "funktion", FUNKTION },
            { "macht", MACHT },
            { "gib", GIB },
            { "zurück", ZURÜCK },
            { "vom", VOM },
            { "Typ", TYP }
        };

        private bool IsAtEnd => current >= source.Length;

        private int start = 0;
        private int current = 0;
        private int line = 1;
        private int consecutiveSpaceCount = 0;

        public Scanner(string source)
        {
            this.source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd)
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, "", null, line, current));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            consecutiveSpaceCount = c == ' ' ? consecutiveSpaceCount + 1 : 0;

            switch (c)
            {
                case '(': AddToken(L_KLAMMER); break;
                case ')': AddToken(R_KLAMMER); break;

                case ',': AddToken(KOMMA); break;
                case '.': AddToken(PUNKT); break;
                case ':': AddToken(DOPPELPUNKT); break;

                case '-': AddToken(BANG_MINUS); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    }
                    break;

                // tabs have special meaning
                case '\t': AddToken(TAB); break;

                case ' ':
                    if (consecutiveSpaceCount == 4)
                    {
                        AddToken(TAB);
                        consecutiveSpaceCount = 0;
                    }
                    break;

                case '\r':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"': StringLiteral(); break;
                case '\'': CharLiteral(); break;

                default:
                    if (char.IsDigit(c))
                    {
                        NumberLiteral();
                    }
                    else if (c.IsAlpha())
                    {
                        Identifier();
                    }
                    else
                    {
                        DDP.Error(line, "Unexpected character: " + c + "/" + (int)c);
                    }
                    break;
            }
        }

        /// <summary>
        /// The advance() method consumes the next character in the source file and returns it.
        /// </summary>
        /// <returns>the next character in the source file</returns>
        private char Advance()
        {
            return source[current++];
        }

        /// <summary>
        /// creates a new token for the given type
        /// </summary>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        ///  grabs the text of the current lexeme and creates a new token for it
        /// </summary>
        private void AddToken(TokenType type, object literal)
        {
            string text = source[start..current];
            tokens.Add(new Token(type, text, literal, line, current));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        private bool Match(char expected)
        {
            if (IsAtEnd) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        /// <summary>
        /// It’s sort of like Advance(), but doesn’t consume the character.
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (IsAtEnd) return '\0';
            return source[current];
        }

        /// <summary>
        /// Handles String literals (e.g.: "text")
        /// </summary>
        private void StringLiteral()
        {
            while (Peek() != '"' && !IsAtEnd)
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd)
            {
                DDP.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = source.Substring(start + 1, (current - start) - 2);
            AddToken(STRING, value);
        }

        private void CharLiteral()
        {
            if (Peek() != '\'' && !IsAtEnd)
            {
                Advance();
            }

            if (IsAtEnd)
            {
                DDP.Error(line, "Unterminated char.");
                return;
            }

            // The closing ".
            if (Advance() != '\'')
            {
                DDP.Error(line, "Ein Zeichen kann nur einen zeichen groß sein! Benutzte eine Zeichenkette wenn du mehr willst.");
            }

            // Trim the surrounding quotes.
            char value = source[current - 2];
            AddToken(CHAR, value);
        }

        /// <summary>
        /// Handles Number Literals (e.g.: 12, 15.2, ect.)
        /// </summary>
        private void NumberLiteral()
        {
            while (char.IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == ',' && char.IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }

            if (source[start..current].Contains(","))
            {
                AddToken(FLOAT, double.Parse(source[start..current], NumberStyles.Float, new CultureInfo("de-DE"))); 
            }
            else
            {
                AddToken(INT, int.Parse(source[start..current]));
            }
        }

        /// <summary>
        /// Handles reserved words
        /// </summary>
        private void Identifier()
        {
            while (Peek().IsAlphaNumeric()) Advance();

            TokenType type;
            string text = source[start..current];
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            else
            {
                type = IDENTIFIER;
            }

            AddToken(type);
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }
    }
}
