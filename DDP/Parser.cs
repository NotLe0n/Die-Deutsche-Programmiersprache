using System.Collections.Generic;

using static DDP.SymbolTyp;

namespace DDP
{
    public class Parser
    {
        private readonly List<Symbol> symbole;
        private int current = 0;
        private int depth = 0;

        private bool IsAtEnd => Peek().typ == EOF;

        public Parser(List<Symbol> symbole)
        {
            this.symbole = symbole;
        }

        public List<Anweisung> Parse()
        {
            List<Anweisung> anweisungen = new();
            while (!IsAtEnd)
            {
                anweisungen.Add(Deklaration());
            }

            return anweisungen;
        }

        private Anweisung Deklaration()
        {
            try
            {
                if (Match(DER)) return VarDeklaration(DER);
                if (Match(DIE)) return Match(FUNKTION) ? Funktion() : VarDeklaration(DIE);

                if (Match(DAS)) return VarDeklaration(DAS);

                return Anweisung();
            }
            catch (ParseFehler fehler)
            {
                Synchronize();
                _ = fehler; // nervige warnung entfernen
                return null;
            }
        }

        private Anweisung Anweisung()
        {
            if (Match(FÜR)) return FürAnweisung();
            if (Match(WENN)) return WennAnweisung();
            if (Match(GIB)) return RückgabeAnweisung();
            if (Match(SOLANGE)) return SolangeAnweisung();
            if (Match(MACHE)) return MacheSolangeAnweisung();
            if (Match(DOPPELPUNKT)) return new Anweisung.Block(Block());

            return AusdruckAnweisung();
        }

        private Anweisung AusdruckAnweisung()
        {
            Ausdruck ausdr = Ausdruck();
            Consume(PUNKT, Fehlermeldungen.dotAfterExpression);
            return new Anweisung.Ausdruck(ausdr);
        }

        private Ausdruck Ausdruck()
        {
            return Zuweisung();
        }

        private Anweisung FürAnweisung()
        {
            Consume(JEDE, Fehlermeldungen.tokenMissing("einer für anweisung", "'jede'"));

            // "Typ von n bis n" syntax
            Anweisung.Var initializer;
            Ausdruck min;
            if (Match(out Symbol matched, ZAHL, KOMMAZAHL, TEXT, BUCHSTABE))
            {
                Symbol name = Consume(IDENTIFIER, Fehlermeldungen.varNameExpected);
                Consume(VON, Fehlermeldungen.tokenMissing("der Variablendeklaration in einer für anweisung", "'von'"));
                min = Ausdruck();

                initializer = new Anweisung.Var(DIE, matched, name, min);
            }
            else throw Error(Previous(), Fehlermeldungen.forNoVar);

            Consume(BIS, Fehlermeldungen.tokenMissing("dem minimum in einer für anweisung", "'bis'"));
            Ausdruck max = Ausdruck();

            // Schrittgröße syntax
            Ausdruck schrittgröße = new Ausdruck.Wert(1);
            if (Match(MIT))
            {
                Consume(SCHRITTGRÖßE, Fehlermeldungen.tokenMissing("'mit' in einer für anweisung", "'schrittgröße'"));

                schrittgröße = Ausdruck();
            }

            Consume(KOMMA, "Es wird ein komma erwartet!");
            Consume(MACHE, Fehlermeldungen.tokenMissingAtEnd("einer für Anweisung", "ein 'mache'"));

            Anweisung körper = Anweisung();

            // Schrittgröße hinzufügen
            körper = new Anweisung.Block(new List<Anweisung>
            {
                körper,
                new Anweisung.Ausdruck(new Ausdruck.Zuweisung(initializer.name, new Ausdruck.Binär(
                    new Ausdruck.Variable(initializer.name),
                    new Symbol(PLUS, "plus", null, initializer.name.zeile, initializer.name.position, depth),
                    schrittgröße)))
            });

            körper = new Anweisung.Für(initializer, min, max, schrittgröße, körper);

            körper = new Anweisung.Block(new List<Anweisung>
            {
                initializer,
                körper
            });

            return körper;
        }

        private Anweisung WennAnweisung()
        {
            var token = Previous();

            // Bedingung
            Ausdruck condition = Ausdruck();
            Consume(KOMMA, Fehlermeldungen.ifKommaMissing);
            Consume(DANN, Fehlermeldungen.ifDannMissing);

            Anweisung thenBranch = Anweisung();

            // wenn aber
            Anweisung elseBranch = null;
            if (Match(WENN))
            {
                if (Match(ABER))
                {
                    elseBranch = WennAnweisung();
                }
                else current--;
            }

            // sonst
            if (Match(SONST))
            {
                elseBranch = Anweisung();
            }

            var stmt = new Anweisung.Wenn(condition, thenBranch, elseBranch);
            stmt.token = token;
            return stmt;
        }

        private Anweisung VarDeklaration(SymbolTyp artikel)
        {
            // der Boolean/die Zahl/die Kommazahl/der Text/der Buchstabe/der Boolean x ist (wahr wenn) y.

            Symbol type = CheckArtikel(artikel);
            Symbol name = Consume(IDENTIFIER, Fehlermeldungen.varNameExpected);

            Ausdruck initializer = null;
            if (type.typ == BOOLEAN || type.typ == ZAHL || type.typ == KOMMAZAHL || type.typ == TEXT || type.typ == BUCHSTABE)
            {
                if (Match(IST))
                {
                    // falls zu einem Boolean zugewiesen wird, braucht der Syntax: "wahr/falsch wenn"
                    bool negate = false;
                    if (type.typ == BOOLEAN)
                    {
                        if (Match(out Symbol matched, WAHR, FALSCH))
                        {
                            if (Match(WENN))
                            {
                                if (matched.typ == FALSCH)
                                {
                                    negate = true;
                                }
                            }
                            else current--;
                        }
                        else Error(Peek(), "fehlt wahr/falsch wenn");
                    }

                    initializer = Ausdruck();

                    // falls "der Boolean x ist falsch wenn y" wird y negiert
                    if (negate)
                    {
                        initializer = new Ausdruck.Unär(new Symbol(NICHT, "nicht", null, name.zeile, name.position, depth), initializer);
                    }
                }
                else Error(Peek(), "nach dem variablen name muss ein 'ist' stehen!");
            }
            else
            {
                if (Match(SIND))
                {
                    initializer = Ausdruck();
                }
                else Error(Peek(), "nach dem variablen name einer Array deklaration muss ein 'sind' stehen!");
            }
            Consume(PUNKT, Fehlermeldungen.dotAfterVarDeclaration);
            return new Anweisung.Var(artikel, type, name, initializer);
        }

        private Symbol CheckArtikel(SymbolTyp artikel)
        {
            Symbol type;
            switch (artikel)
            {
                case DER:
                    if (Match(out var matched, BOOLEAN, BUCHSTABE, TEXT))
                    {
                        type = matched;
                        break;
                    }
                    else throw Error(Previous(), Fehlermeldungen.wrongArtikel("'der'", "zu den Typen Boolean, Buchstabe, oder Text"));
                case DIE:
                    if (Match(out var matched2, ZAHL, KOMMAZAHL, ZAHLEN, KOMMAZAHLEN, BOOLEANS, TEXTE, BUCHSTABEN))
                    {
                        type = matched2;
                        break;
                    }
                    else throw Error(Previous(), Fehlermeldungen.wrongArtikel("'die'", "zu den Typen Zahl, Kommazahl, Zahlen, Kommazahlen, Booleans, Texte, oder Buchstaben"));
                case DAS:
                default:
                    throw Error(Previous(), Fehlermeldungen.noArtikel);
            }

            return type;
        }

        private Anweisung SolangeAnweisung()
        {
            var token = Previous();

            // solange x:{\n\t}y
            Ausdruck condition = Ausdruck();
            Consume(KOMMA, Fehlermeldungen.tokenMissing("der Bedingung einer solange-Anweisung", "ein komma"));
            Consume(MACHE, Fehlermeldungen.tokenMissingAtEnd("einer solange Anweisung", "ein 'mache'"));
            Anweisung body = Anweisung();

            var stmt = new Anweisung.Solange(condition, body);
            stmt.symbol = token;
            return stmt;
        }

        private Anweisung MacheSolangeAnweisung()
        {
            var token = Previous();

            // mache:{\n\t}x{\n}solange y.
            Anweisung body = Anweisung();
            Consume(SOLANGE, Fehlermeldungen.tokenMissing("einem mache-block", "'solange'"));
            Ausdruck condition = Ausdruck();
            Consume(PUNKT, Fehlermeldungen.dotAfterExpression);

            var stmt = new Anweisung.MacheSolange(condition, body);
            stmt.symbol = token;
            return stmt;
        }

        private Anweisung.Funktion Funktion()
        {
            Symbol name = Consume(IDENTIFIER, Fehlermeldungen.funcNameExpected);
            Symbol typ = null;
            List<Symbol> parameters = new();

            // Argumente
            Consume(L_KLAMMER, Fehlermeldungen.tokenMissing("dem Funktionsname", "eine Klammer auf"));

            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), Fehlermeldungen.tooManyArguments);
                }

                if (Match(ZAHL, KOMMAZAHL, BOOLEAN, BUCHSTABE, TEXT, ZAHLEN, KOMMAZAHLEN, BOOLEANS, TEXTE, BUCHSTABEN))
                {
                    parameters.Add(Consume(IDENTIFIER, Fehlermeldungen.argumentNameExpected));
                }
            } while (Match(KOMMA));

            Consume(R_KLAMMER, Fehlermeldungen.tokenMissing("den Argumenten", "eine Klammer zu"));

            // rückgabe Typ
            if (Match(VOM))
            {
                Consume(TYP, Fehlermeldungen.tokenMissing("einer vom Anweisung", "ein Typ"));
                if (Match(out Symbol match, ZAHL, KOMMAZAHL, BOOLEAN, BUCHSTABE, TEXT, ZAHLEN, KOMMAZAHLEN, BOOLEANS, TEXTE, BUCHSTABEN))
                {
                    typ = match;
                }
                else
                {
                    Error(Peek(), Fehlermeldungen.returnTypeInvalid);
                }
            }

            // Funktionskörper
            Consume(MACHT, Fehlermeldungen.tokenMissingAtEnd("variablen deklaration", "ein 'macht'"));
            Consume(DOPPELPUNKT, Fehlermeldungen.tokenMissing("einer macht anweisung", "ein doppelpunkt"));

            List<Anweisung> body = Block();

            // Funktion braucht eine Rückgabe-Anweisung wenn es einen Rückgabe-Typ besitzt.
            if (typ != null)
            {
                foreach (var stmt in body)
                {
                    if (stmt is Anweisung.Rückgabe)
                        return new Anweisung.Funktion(name, parameters, typ, body);
                }
                Error(name, Fehlermeldungen.returnMissing);
            }

            return new Anweisung.Funktion(name, parameters, typ, body);
        }

        private Anweisung RückgabeAnweisung()
        {
            // "gib 2 zurück."

            Symbol keyword = Previous();
            Ausdruck value = null;
            if (!Match(PUNKT))
            {
                value = Ausdruck();
            }

            Consume(ZURÜCK, Fehlermeldungen.tokenMissing("einem Rückgabe-Wert", "'zurück'"));
            Consume(PUNKT, Fehlermeldungen.tokenMissing("einer Rückgabe-Anweisung", "ein punkt"));
            return new Anweisung.Rückgabe(keyword, value);
        }

        private List<Anweisung> Block()
        {
            depth++;
            List<Anweisung> statements = new();

            while (Peek().tiefe == depth && !IsAtEnd)
            {
                statements.Add(Deklaration());
            }

            depth--;

            return statements;
        }

        private Ausdruck Zuweisung()
        {
            Ausdruck expr = Oder();

            // "x ist y"
            if (Match(IST))
            {
                Symbol equals = Previous();
                Ausdruck value = Zuweisung();

                if (expr is Ausdruck.Variable variable)
                {
                    Symbol name = variable.name;
                    return new Ausdruck.Zuweisung(name, value);
                }

                if (expr is Ausdruck.Binär bin && bin.op.typ == STELLE && bin.links is Ausdruck.Variable _variable)
                {
                    return new Ausdruck.Zuweisung(_variable.name, bin.rechts, value);
                }

                Error(equals, Fehlermeldungen.varInvalidAssignment);
            }

            return expr;
        }

        private Ausdruck Oder()
        {
            Ausdruck expr = Und();

            // "x oder y"
            while (Match(ODER))
            {
                Symbol op = Previous();
                Ausdruck right = Und();
                expr = new Ausdruck.Logisch(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Und()
        {
            Ausdruck expr = Gleichheit();

            // "x und y"
            while (Match(UND))
            {
                Symbol op = Previous();
                Ausdruck right = Gleichheit();
                expr = new Ausdruck.Logisch(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Gleichheit()
        {
            Ausdruck expr = Vergleich();

            // "x un-/gleich y"
            while (Match(UNGLEICH, GLEICH))
            {
                Symbol op = Previous();
                Ausdruck right = Vergleich();
                expr = new Ausdruck.Binär(expr, op, right);

                Consume(IST, Fehlermeldungen.tokenMissing("einem Vergleich", "'ist'"));
            }

            return expr;
        }

        private Ausdruck Vergleich()
        {
            Ausdruck expr = Bitweise();

            // "x größer/kleiner als(, oder) y ist"
            while (Match(GRÖßER, KLEINER))
            {
                // Schaue ob ein "als" vorhanden ist.
                if (!Check(ALS))
                {
                    Error(Previous(), Fehlermeldungen.tokenMissing("einem größer/kleiner operator", "'als'"));
                    continue;
                }

                Symbol op = Previous();
                Advance(); // ALS

                // "x größer/kleiner als, oder y ist"
                if (Match(KOMMA) && Match(ODER))
                {
                    if (op.typ == GRÖßER)
                        op.typ = GRÖßER_GLEICH;
                    else if (op.typ == KLEINER)
                        op.typ = KLEINER_GLEICH;
                }

                Ausdruck right = Bitweise();

                Consume(IST, Fehlermeldungen.tokenMissing("einem Vergleich", "'ist'"));
                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Bitweise()
        {
            // "x logisch oder/und y"
            while (Match(LOGISCH))
            {
                Ausdruck _expr = Term();
                Symbol op = Advance();
                Ausdruck right = Term();
                return new Ausdruck.Binär(_expr, op, right);
            }

            Ausdruck expr = Term();

            // "x um y bit nach links/rechts verschoben"
            while (Match(UM))
            {
                Ausdruck right = Term();
                Consume(BIT, Fehlermeldungen.tokenMissing("dem verschiebungswert", "'bit'"));
                Consume(NACH, Fehlermeldungen.tokenMissing("einer bit Anweisung", "ein 'nach'"));
                Symbol op = Advance();
                Consume(VERSCHOBEN, Fehlermeldungen.tokenMissingAtEnd("einer Bitverschiebungsanweisung", "ein 'verschoben'"));

                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Term()
        {
            Ausdruck expr = Trigo();

            // "x plus/minus y"
            while (Match(MINUS, PLUS))
            {
                Symbol op = Previous();
                Ausdruck right = Trigo();
                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Trigo()
        {
            // "Sinus/Kosinus/Tangens/Arkussinus/Arkuskosinus/Arkustangens/Hyperbeksinus/Hyperbelkosinus/Hyperbeltangens von x"
            while (Match(SINUS, KOSINUS, TANGENS, ARKUSSINUS, ARKUSKOSINUS, ARKUSTANGENS, HYPERBELSINUS, HYPERBELKOSINUS, HYPERBELTANGENS))
            {
                Symbol op = Previous();
                Consume(VON, "fehlt von");
                Ausdruck right = Faktor();
                return new Ausdruck.Unär(op, right);
            }

            return Faktor();
        }

        private Ausdruck Faktor()
        {
            Ausdruck expr = Wurzel();

            // "x durch/mal/modulo y"
            while (Match(DURCH, MAL, MODULO))
            {
                Symbol op = Previous();
                Ausdruck right = Wurzel();
                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Wurzel()
        {
            Ausdruck expr = Potenz();

            // "x. wurzel von y"
            if (Match(PUNKT))
            {
                if (Match(WURZEL))
                {
                    Symbol op = Previous();
                    Consume(VON, Fehlermeldungen.tokenMissing("dem Wurzel operator", "'von'"));
                    Ausdruck right = Potenz();
                    expr = new Ausdruck.Binär(right, op, expr);
                }
                else
                {
                    current--; // eine anweisung wie: "print 2,0." Matched PUNKT, aber nicht Wurzel wird aber trotzdem Consumed.
                }
            }

            return expr;
        }

        private Ausdruck Potenz()
        {
            Ausdruck expr = Unär();

            // "x hoch y"
            while (Match(HOCH))
            {
                Symbol op = Previous();
                Ausdruck right = Unär();
                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Unär()
        {
            // "logisch nicht x"
            if (Match(LOGISCH))
            {
                if (Match(NICHT))
                {
                    Symbol op = Previous();
                    Ausdruck right = Unär();
                    return new Ausdruck.Unär(op, right);
                }
                else
                {
                    current--;
                }
            }

            // "ln x"
            if (Match(LOG))
            {
                Symbol op = Previous();
                Ausdruck right = Unär();
                return new Ausdruck.Unär(op, right);
            }

            // "der Betrag von x"
            if (Match(DER))
            {
                if (Match(BETRAG))
                {
                    Symbol op = Previous();
                    Consume(VON, Fehlermeldungen.tokenMissing("dem Betrag operator", "'von'"));
                    Ausdruck right = Unär();
                    return new Ausdruck.Unär(op, right);
                }
                else
                {
                    current--;
                }
            }

            // "nicht/- x"
            if (Match(NICHT, BANG_MINUS))
            {
                Symbol op = Previous();
                Ausdruck right = Unär();
                return new Ausdruck.Unär(op, right);
            }

            return Aufruf();
        }

        private Ausdruck Aufruf()
        {
            Ausdruck expr = Indexierung();

            // "x(y, z)"
            if (Match(L_KLAMMER))
            {
                List<Ausdruck> arguments = new();
                if (!Check(R_KLAMMER))
                {
                    do
                    {
                        if (arguments.Count >= 255)
                        {
                            Error(Peek(), Fehlermeldungen.tooManyArguments);
                        }
                        arguments.Add(Ausdruck());
                    } while (Match(KOMMA));
                }

                Symbol paren = Consume(R_KLAMMER, Fehlermeldungen.parameterParenMissing);

                expr = new Ausdruck.Aufruf(expr, paren, arguments);
            }

            return expr;
        }

        private Ausdruck Indexierung()
        {
            Ausdruck expr = Primär();

            if (Match(AN))
            {
                Consume(DER, "fehlt an");
                Consume(STELLE, "fehlt an");

                Symbol op = Previous();
                Ausdruck right = Primär();

                expr = new Ausdruck.Binär(expr, op, right);
            }

            return expr;
        }

        private Ausdruck Primär()
        {
            if (Match(FALSCH)) return new Ausdruck.Wert(false);
            if (Match(WAHR)) return new Ausdruck.Wert(true);
            if (Match(PI)) return new Ausdruck.Wert(System.Math.PI);
            if (Match(TAU)) return new Ausdruck.Wert(System.Math.Tau);
            if (Match(E)) return new Ausdruck.Wert(System.Math.E);
            if (Match(PHI)) return new Ausdruck.Wert(1.6180339887);

            if (Match(INT, FLOAT, STRING, CHAR))
            {
                return new Ausdruck.Wert(Previous().wert);
            }

            if (Match(INTARR, FLOATARR, STRINGARR, CHARARR, BOOLEANARR))
            {
                return new Ausdruck.Wert(Previous().wert);
            }

            if (Match(IDENTIFIER))
            {
                return new Ausdruck.Variable(Previous());
            }

            if (Match(L_KLAMMER))
            {
                Ausdruck expr = Ausdruck();
                Consume(R_KLAMMER, Fehlermeldungen.groupingParenMissing);
                return new Ausdruck.Gruppierung(expr);
            }

            throw Error(Peek(), Fehlermeldungen.expressionMissing);
        }

        /// <summary>
        /// Advances if one of the given TokenTypes is the current Token
        /// </summary>
        /// <param name="types">The TokenTypes to check for</param>
        /// <returns>true there was a match, false if otherwise</returns>
        private bool Match(params SymbolTyp[] types)
        {
            foreach (SymbolTyp type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// <see cref="Advance">Advances</see> if one of the given TokenTypes is the current Token.
        /// </summary>
        /// <param name="matched">The Token, which has been matched. null if no token was matched</param>
        /// <param name="types">The TokenTypes to check for</param>
        /// <returns>true there was a match, false if otherwise</returns>
        private bool Match(out Symbol matched, params SymbolTyp[] types)
        {
            foreach (SymbolTyp type in types)
            {
                if (Check(type))
                {
                    matched = Advance();
                    return true;
                }
            }
            matched = null;
            return false;
        }

        /// <summary>
        /// Advances if the the current Token is the given <paramref name="type" /> and throws an error if it's not
        /// </summary>
        /// <param name="type">The TokenType to check for</param>
        /// <param name="message">The error message to show if the check failed</param>
        private Symbol Consume(SymbolTyp type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        /// <summary>
        /// Checks if the <see cref="current">current</see> Token is the given TokenType
        /// </summary>
        /// <param name="type">The TokenType to check for</param>
        /// <returns>true if the current Token is the given TokenType. Returns false if the current Token is <see cref="EOF"/></returns>
        private bool Check(SymbolTyp type)
        {
            if (IsAtEnd) return false;
            return Peek().typ == type;
        }

        /// <summary>
        /// Increments <see cref="current">current</see> if the current Token is not <see cref="EOF"/>
        /// </summary>
        /// <returns>the now previous Token</returns>
        private Symbol Advance()
        {
            if (!IsAtEnd) current++;
            return Previous();
        }

        /// <returns>the <see cref="current">current</see> Token</returns>
        private Symbol Peek()
        {
            return symbole[current];
        }

        /// <returns>the previous Token</returns>
        private Symbol Previous()
        {
            return symbole[current - 1];
        }

        private static ParseFehler Error(Symbol token, string message)
        {
            DDP.Fehler(token, message);
            return new ParseFehler(token, message);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().typ == PUNKT) return;

                switch (Peek().typ)
                {
                    case FUNKTION:
                    case FÜR:
                    case WENN:
                    case SOLANGE:
                    case GIB:
                        return;
                }

                Advance();
            }
        }
    }
}
