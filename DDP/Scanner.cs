using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using static DDP.SymbolTyp;

namespace DDP
{
    class Scanner
    {
        private readonly string quelle;
        private readonly List<Symbol> symbole = new();

        private readonly Dictionary<string, SymbolTyp> schlüsselwörter = new()
        {
            // Artikel
            { "der", DER },
            { "die", DIE },
            { "das", DAS },

            // Typen
            { "Zahl", ZAHL },
            { "Kommazahl", KOMMAZAHL },
            { "Boolean", BOOLEAN },
            { "Zeichenkette", ZEICHENKETTE },
            { "Zeichen", ZEICHEN },
            { "Zahlen", ZAHLEN },
            { "Kommazahlen", KOMMAZAHLEN },
            { "Zeichenketten", ZEICHENKETTEN },

            // boolean
            { "wahr", WAHR },
            { "falsch", FALSCH },

            // mathematische operatoren
            { "ist", IST },
            { "sind", SIND },
            { "plus", PLUS },
            { "minus", MINUS },
            { "mal", MAL },
            { "durch", DURCH },
            { "modulo", MODULO },
            { "hoch", HOCH },
            { "wurzel", WURZEL },
            { "ln", LOG },
            { "Betrag", BETRAG }, // oder Absolutwert (wir haben uns um den namen gestritten)
            { "an", AN },
            { "Stelle", STELLE },

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
            { "pi", PI },
            { "e", E },
            { "tau", TAU },
            { "phi", PHI },

            // trigonometrische funktionen
            { "Sinus", SINUS },
            { "Kosinus", KOSINUS },
            { "Tangens", TANGENS },
            { "Arkussinus", ARKUSSINUS },
            { "Arkuskosinus", ARKUSKOSINUS },
            { "Arkustangens", ARKUSTANGENS },
            { "Hyperbelsinus", HYPERBELSINUS },
            { "Hyperbelkosinus", HYPERBELKOSINUS },
            { "Hyperbeltangens", HYPERBELTANGENS },

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
            { "Funktion", FUNKTION },
            { "macht", MACHT },
            { "gib", GIB },
            { "zurück", ZURÜCK },
            { "vom", VOM },
            { "Typ", TYP }
        };

        private bool AmEnde => current >= quelle.Length;

        private int start = 0;
        private int current = 0;
        private int zeile = 1;
        private int zeilenposition = 0;
        private int aufeinanderfolgendeLeerzeichen = 0;
        private int tiefe;

        public Scanner(string quelle)
        {
            this.quelle = quelle;
        }

        public List<Symbol> ScanTokens()
        {
            while (!AmEnde)
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            symbole.Add(new Symbol(EOF, "", null, zeile, current, 0));
            return symbole;
        }

        private void ScanToken()
        {
            char c = Advance();
            aufeinanderfolgendeLeerzeichen = c == ' ' ? aufeinanderfolgendeLeerzeichen + 1 : 0;

            switch (c)
            {
                case '(': AddToken(L_KLAMMER); break;
                case ')': AddToken(R_KLAMMER); break;
                case '[': ArrayLiteral(); break;

                case ',': AddToken(KOMMA); break;
                case '.': AddToken(PUNKT); break;
                case ':': AddToken(DOPPELPUNKT); break;

                case '-': AddToken(BANG_MINUS); break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !AmEnde) Advance();
                    }
                    if (Match('*'))
                    {
                        while (Peek() != '*' && PeekNext() != '/' && !AmEnde) Advance();
                        Advance();
                        Advance();
                    }
                    break;

                // tabs have special meaning
                case '\t':
                    tiefe++;
                    break;

                case ' ':
                    // wenn 4 leerzeichen hintereinander auftreten, füge einen tab hinzu
                    if (aufeinanderfolgendeLeerzeichen == 4)
                    {
                        tiefe++;
                        aufeinanderfolgendeLeerzeichen = 0;
                    }
                    break;

                case '\r':
                    // ignorieren
                    break;

                case '\n':
                    tiefe = 0;
                    zeile++;
                    zeilenposition = current;
                    break;

                case '"': StringLiteral(); break;
                case '\'': CharLiteral(); break;

                default:
                    if (char.IsDigit(c))
                    {
                        NumberLiteral();
                    }
                    else if (c.IstDeutsch())
                    {
                        Identifier();
                    }
                    else
                    {
                        DDP.Fehler(zeile, "Unerwarteter character: " + c + " / " + (int)c);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles String literals (e.g.: "text")
        /// </summary>
        private void StringLiteral()
        {
            while (Peek() != '"' && !AmEnde)
            {
                if (Peek() == '\n') zeile++;
                Advance();
            }

            if (AmEnde)
            {
                DDP.Fehler(zeile, Fehlermeldungen.stringUnterminated);
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string wert = quelle.Substring(start + 1, (current - start) - 2);
            AddToken(STRING, wert);
        }

        private void CharLiteral()
        {
            char? wert = null;
            if (Peek() != '\'' && !AmEnde)
            {
                wert = Advance();
            }

            if (AmEnde)
            {
                DDP.Fehler(zeile, Fehlermeldungen.charUnterminated);
                return;
            }

            // The closing ".
            if (Advance() != '\'')
            DDP.Fehler(zeile, Fehlermeldungen.charTooLong);

            if (wert == null)
            DDP.Fehler(zeile, "leerer zeichen");

            AddToken(CHAR, wert);
        }

        /// <summary>
        /// Handles Number Literals (e.g.: 12, 15.2, ect.)
        /// </summary>
        private void NumberLiteral()
        {
            while (char.IsDigit(Peek())) Advance();

            // wenn es einen komma gefolgt von einer zahl findet
            if (Peek() == ',' && char.IsDigit(PeekNext()))
            {
                // Komma essen
                Advance();

                while (char.IsDigit(Peek())) Advance();
            }

            // wenn ein komma existiert, dann wird es ein double sonst wird es ein int
            if (quelle[start..current].Contains(","))
            AddToken(FLOAT, double.Parse(quelle[start..current], NumberStyles.Float, new CultureInfo("de-DE")));
            else
            {
                AddToken(INT, int.Parse(quelle[start..current]));
            }
        }

        private void ArrayLiteral()
        {
            while (Peek() != ']') Advance();

            string str = quelle[++start..current];
            var list = str.Split("; ");

            System.Type type;
            if (list.All(x => x.IstNumerisch() && !x.Contains(','))) type = typeof(int);
            else if (list.All(x => x.Contains(','))) type = typeof(double);
            else if (list.All(x => x.Contains('"'))) type = typeof(string);
            else if (list.All(x => x.Contains('\''))) type = typeof(char);
            else if (list.All(x => x == "wahr" || x == "falsch")) type = typeof(bool);
            else
            {
                DDP.Fehler(zeile, "invalid array type");
                return;
            }

            var li = new List<object>();
            foreach (var entry in list)
            {
                if (type == typeof(int)) li.Add(int.Parse(entry));
                else if (type == typeof(double)) li.Add(double.Parse(entry, NumberStyles.Float, new CultureInfo("de-DE")));
                else if (type == typeof(char)) li.Add(entry[0]);
                else if (type == typeof(string)) li.Add(entry);
                else if (type == typeof(bool)) li.Add(entry == "wahr" ? true : entry == "falsch" ? false : null);
            }

            if (Advance() != ']')
            {
                DDP.Fehler(zeile, "geschlossene eckige klammer fehlt");
                return;
            }

            if (type == typeof(int)) AddToken(INTARR, li.ToArray());
            else if (type == typeof(double)) AddToken(FLOATARR, li.ToArray());
            else if (type == typeof(char)) AddToken(CHARARR, li.ToArray());
            else if (type == typeof(string)) AddToken(STRINGARR, li.ToArray());
            else if (type == typeof(bool)) AddToken(BOOLEANARR, li.ToArray());
        }

        /// <summary>
        /// Handles reserved words
        /// </summary>
        private void Identifier()
        {
            while (Peek().IsAlphaNumeric()) Advance();

            SymbolTyp typ;
            string text = quelle[start..current];
            if (schlüsselwörter.ContainsKey(text))
            {
                typ = schlüsselwörter[text];
            }
            else if (text == "binde")
            {
                Einbindung();
                return;
            }
            else
            {
                typ = IDENTIFIER;
            }

            AddToken(typ);
        }

        private void Einbindung()
        {
            if (Advance() == ' ' && Match('"'))
            {
                string str = string.Empty;
                while (Peek() != '"' && Peek() != '\n' && !AmEnde) str += Advance();

                if (Match('"'))
                {
                    if (Advance() == ' ' && Advance() == 'e' && Advance() == 'i' && Advance() == 'n')
                    {
                        var path = Path.GetFullPath(Path.Combine(Directory.GetParent(DDP.dateiPfad).FullName, str)) + ".ddp";

                        if (File.Exists(path))
                        {
                            var tokens = new Scanner(File.ReadAllText(path)).ScanTokens();
                            tokens.RemoveAt(tokens.Count - 1);
                            symbole.AddRange(tokens);
                        }
                        else DDP.Fehler(zeile, "Datei zum einbinden nicht gefunden!");
                    }
                }
            }
            if (!Match('.')) DDP.Fehler(zeile, Fehlermeldungen.dotAfterExpression);
        }

        /// <summary>
        /// The advance() method consumes the next character in the source file and returns it.
        /// </summary>
        /// <returns>the next character in the source file</returns>
        private char Advance()
        {
            return quelle[current++];
        }

        /// <summary>
        /// creates a new token for the given type
        /// </summary>
        private void AddToken(SymbolTyp typ)
        {
            AddToken(typ, null);
        }

        /// <summary>
        ///  grabs the text of the current lexeme and creates a new token for it
        /// </summary>
        private void AddToken(SymbolTyp typ, object literal)
        {
            string text = quelle[start..current];
            symbole.Add(new Symbol(typ, text, literal, zeile, current - zeilenposition, tiefe));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="erwartet"></param>
        /// <returns></returns>
        private bool Match(char erwartet)
        {
            if (AmEnde) return false;
            if (quelle[current] != erwartet) return false;

            current++;
            return true;
        }

        /// <summary>
        /// It’s sort of like Advance(), but doesn’t consume the character.
        /// </summary>
        /// <returns></returns>
        private char Peek()
        {
            if (AmEnde) return '\0';
            return quelle[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= quelle.Length) return '\0';
            return quelle[current + 1];
        }
    }
}
