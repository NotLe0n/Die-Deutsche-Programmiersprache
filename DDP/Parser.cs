using System.Collections.Generic;

using static DDP.TokenType;

namespace DDP
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;
        private int depth = 0;

        private bool IsAtEnd => Peek().type == EOF;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Statement> Parse()
        {
            List<Statement> statements = new();
            while (!IsAtEnd)
            {
                statements.Add(Deklaration());
            }

            return statements;
        }

        private Statement Deklaration()
        {
            try
            {
                if (Match(DER)) return VarDeclaration(DER);
                if (Match(DIE)) return Match(FUNKTION) ? Funktion() : VarDeclaration(DIE);

                if (Match(DAS)) return VarDeclaration(DAS);

                return Anweisung();
            }
            catch (ParseError error)
            {
                Synchronize();
                _ = error; // get rid of annyoing warning
                return null;
            }
        }

        private Statement Anweisung()
        {
            if (Match(FÜR)) return FürAnweisung();
            if (Match(WENN)) return WennAnweisung();
            if (Match(GIB)) return RückgabeAnweisung();
            if (Match(SOLANGE)) return SolangeAnweisung();
            if (Match(MACHE)) return MacheSolangeAnweisung();
            if (Match(DOPPELPUNKT)) return new Statement.Block(Block());

            return AusdruckAnweisung();
        }

        private Statement AusdruckAnweisung()
        {
            Expression expr = Ausdruck();
            Consume(PUNKT, Fehlermeldungen.dotAfterExpression);
            return new Statement.Expression(expr);
        }

        private Expression Ausdruck()
        {
            return Zuweisung();
        }

        private Statement FürAnweisung()
        {
            Consume(JEDE, Fehlermeldungen.tokenMissing("einer für anweisung", "'jede'"));

            // "Typ von n bis n" syntax
            Statement.Var initializer;
            Expression min;
            if (Match(out Token matched, ZAHL, KOMMAZAHL, ZEICHENKETTE, ZEICHEN))
            {
                Token name = Consume(IDENTIFIER, Fehlermeldungen.varNameExpected);
                Consume(VON, Fehlermeldungen.tokenMissing("der Variablendeklaration in einer für anweisung", "'von'"));
                min = Ausdruck();

                initializer = new Statement.Var(matched, name, min);
            }
            else throw Error(Previous(), Fehlermeldungen.forNoVar);

            Consume(BIS, Fehlermeldungen.tokenMissing("dem minimum in einer für anweisung", "'bis'"));
            Expression max = Ausdruck();

            // Schrittgröße syntax
            Expression schrittgröße = new Expression.Literal(1);
            if (Match(MIT))
            {
                Consume(SCHRITTGRÖßE, Fehlermeldungen.tokenMissing("'mit' in einer für anweisung", "'schrittgröße'"));

                schrittgröße = Ausdruck();
            }

            Consume(KOMMA, "Es wird ein komma erwartet!");
            Consume(MACHE, Fehlermeldungen.tokenMissingAtEnd("einer für Anweisung", "ein 'mache'"));

            Statement body = Anweisung();

            // Schrittgröße hinzufügen
            body = new Statement.Block(new List<Statement>
            {
                body,
                new Statement.Expression(new Expression.Assign(initializer.name, new Expression.Binary(
                    new Expression.Variable(initializer.name),
                    new Token(PLUS, "plus", null, initializer.name.line, initializer.name.position),
                    schrittgröße)))
            });

            body = new Statement.For(initializer, min, max, schrittgröße, body);

            body = new Statement.Block(new List<Statement>
            {
                initializer,
                body
            });

            return body;
        }

        private Statement WennAnweisung()
        {
            var token = Previous();

            // Bedingung
            Expression condition = Ausdruck();
            Consume(KOMMA, Fehlermeldungen.ifKommaMissing);
            Consume(DANN, Fehlermeldungen.ifDannMissing);

            Statement thenBranch = Anweisung();
            Match(depth, TAB); // consume all tabs (workaround to block issue)

            // wenn aber
            Statement elseBranch = null;
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

            var stmt = new Statement.If(condition, thenBranch, elseBranch);
            stmt.token = token;
            return stmt;
        }

        private Statement VarDeclaration(TokenType artikel)
        {
            // der Boolean/die Zahl/die Kommazahl/die Zeichenkette/das Zeichen x ist (wahr wenn) y.

            Token type = CheckArtikel(artikel);
            Token name = Consume(IDENTIFIER, Fehlermeldungen.varNameExpected);

            Expression initializer = null;
            if (Match(IST))
            {
                // falls zu einem Boolean zugewiesen wird, braucht der Syntax: "wahr/falsch wenn"
                bool negate = false;
                if (type.type == BOOLEAN)
                {
                    if (Match(out Token matched, WAHR, FALSCH))
                    {
                        Consume(WENN, "fehlt wenn");
                        if(matched.type == FALSCH)
                        {
                            negate = true;
                        }
                    }
                    else Error(Peek(), "fehlt wahr/falsch wenn");
                }

                initializer = Ausdruck();

                // falls "der Boolean x ist falsch wenn y" wird y negiert
                if (negate)
                {
                    initializer = new Expression.Unary(new Token(NICHT, "nicht", null, name.line, name.position), initializer);
                }
            }

            Consume(PUNKT, Fehlermeldungen.dotAfterVarDeclaration);
            return new Statement.Var(type, name, initializer);
        }

        private Token CheckArtikel(TokenType artikel)
        {
            Token type;
            switch (artikel)
            {
                case DER:
                    type = Consume(BOOLEAN, Fehlermeldungen.wrongArtikel("'der'", "zum Typ Boolean"));
                    break;
                case DIE:
                    if (Match(out var matched, ZAHL, KOMMAZAHL, ZEICHENKETTE))
                    {
                        type = matched;
                    }
                    else
                    {
                        throw Error(Previous(), Fehlermeldungen.wrongArtikel("'die'", "zu den Typen Zahl, Kommazahl, oder Zeichenkette"));
                    }
                    break;
                case DAS:
                    type = Consume(ZEICHEN, Fehlermeldungen.wrongArtikel("'das'", "zum Typ Zeichen"));
                    break;
                default:
                    throw Error(Previous(), Fehlermeldungen.noArtikel);
            }

            return type;
        }

        private Statement SolangeAnweisung()
        {
            var token = Previous();

            // solange x:{\n\t}y
            Expression condition = Ausdruck();
            Consume(KOMMA, Fehlermeldungen.tokenMissing("der Bedingung einer solange-Anweisung", "ein komma"));
            Consume(MACHE, Fehlermeldungen.tokenMissingAtEnd("einer solange Anweisung", "ein 'mache'"));
            Statement body = Anweisung();

            var stmt = new Statement.While(condition, body);
            stmt.token = token;
            return stmt;
        }

        private Statement MacheSolangeAnweisung()
        {
            var token = Previous();

            // mache:{\n\t}x{\n}solange y.
            Statement body = Anweisung();
            Consume(SOLANGE, Fehlermeldungen.tokenMissing("einem mache-block", "'solange'"));
            Expression condition = Ausdruck();
            Consume(PUNKT, Fehlermeldungen.dotAfterExpression);

            var stmt = new Statement.DoWhile(condition, body);
            stmt.token = token;
            return stmt;
        }

        private Statement.Function Funktion()
        {
            Token name = Consume(IDENTIFIER, Fehlermeldungen.funcNameExpected);
            Token typ = null;
            List<Token> parameters = new();

            // Argumente
            Consume(L_KLAMMER, Fehlermeldungen.tokenMissing("dem Funktionsname", "eine Klammer auf"));

            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), Fehlermeldungen.tooManyArguments);
                }

                if (Match(ZAHL, KOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
                {
                    parameters.Add(Consume(IDENTIFIER, Fehlermeldungen.argumentNameExpected));
                }
            } while (Match(KOMMA));

            Consume(R_KLAMMER, Fehlermeldungen.tokenMissing("den Argumenten", "eine Klammer zu"));

            // rückgabe Typ
            if (Match(VOM))
            {
                Consume(TYP, Fehlermeldungen.tokenMissing("einer vom Anweisung", "ein Typ"));
                if (Match(out Token match, ZAHL, KOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
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

            List<Statement> body = Block();

            // Funktion braucht eine Rückgabe-Anweisung wenn es einen Rückgabe-Typ besitzt.
            if (typ != null)
            {
                foreach (var stmt in body)
                {
                    if (stmt is Statement.Return)
                        return new Statement.Function(name, parameters, typ, body);
                }
                Error(name, Fehlermeldungen.returnMissing);
            }

            return new Statement.Function(name, parameters, typ, body);
        }

        private Statement RückgabeAnweisung()
        {
            // "gib 2 zurück."

            Token keyword = Previous();
            Expression value = null;
            if (!Match(PUNKT))
            {
                value = Ausdruck();
            }

            Consume(ZURÜCK, Fehlermeldungen.tokenMissing("einem Rückgabe-Wert", "'zurück'"));
            Consume(PUNKT, Fehlermeldungen.tokenMissing("einer Rückgabe-Anweisung", "ein punkt"));
            return new Statement.Return(keyword, value);
        }

        private List<Statement> Block()
        {
            depth++;
            List<Statement> statements = new();

            while (Match(depth, TAB) && !IsAtEnd)
            {
                statements.Add(Deklaration());
            }

            depth--;

            return statements;
        }

        private Expression Zuweisung()
        {
            Expression expr = Oder();

            // "x ist y"
            if (Match(IST))
            {
                Token equals = Previous();
                Expression value = Zuweisung();

                if (expr is Expression.Variable variable)
                {
                    Token name = variable.name;
                    return new Expression.Assign(name, value);
                }

                Error(equals, Fehlermeldungen.varInvalidAssignment);
            }

            return expr;
        }

        private Expression Oder()
        {
            Expression expr = Und();

            // "x oder y"
            while (Match(ODER))
            {
                Token op = Previous();
                Expression right = Und();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression Und()
        {
            Expression expr = Gleichheit();

            // "x und y"
            while (Match(UND))
            {
                Token op = Previous();
                Expression right = Gleichheit();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression Gleichheit()
        {
            Expression expr = Vergleich();

            // "x un-/gleich y"
            while (Match(UNGLEICH, GLEICH))
            {
                Token op = Previous();
                Expression right = Vergleich();
                expr = new Expression.Binary(expr, op, right);

                Consume(IST, Fehlermeldungen.tokenMissing("einem Vergleich", "'ist'"));
            }

            return expr;
        }

        private Expression Vergleich()
        {
            Expression expr = Bitweise();

            // "x größer/kleiner als(, oder) y ist"
            while (Match(GRÖßER, KLEINER))
            {
                // Schaue ob ein "als" vorhanden ist.
                if (!Check(ALS))
                {
                    Error(Previous(), Fehlermeldungen.tokenMissing("einem größer/kleiner operator", "'als'"));
                    continue;
                }

                Token op = Previous();
                Advance(); // ALS

                // "x größer/kleiner als, oder y ist"
                if (Match(KOMMA) && Match(ODER))
                {
                    if (op.type == GRÖßER)
                        op.type = GRÖßER_GLEICH;
                    else if (op.type == KLEINER)
                        op.type = KLEINER_GLEICH;
                }

                Expression right = Bitweise();

                Consume(IST, Fehlermeldungen.tokenMissing("einem Vergleich", "'ist'"));
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Bitweise()
        {
            // "x logisch oder/und y"
            while (Match(LOGISCH))
            {
                Expression _expr = Term();
                Token op = Advance();
                Expression right = Term();
                return new Expression.Binary(_expr, op, right);
            }

            Expression expr = Term();

            // "x um y bit nach links/rechts verschoben"
            while (Match(UM))
            {
                Expression right = Term();
                Consume(BIT, Fehlermeldungen.tokenMissing("dem verschiebungswert", "'bit'"));
                Consume(NACH, Fehlermeldungen.tokenMissing("einer bit Anweisung", "ein 'nach'"));
                Token op = Advance();
                Consume(VERSCHOBEN, Fehlermeldungen.tokenMissingAtEnd("einer Bitverschiebungsanweisung", "ein 'verschoben'"));

                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Term()
        {
            Expression expr = Trigo();

            // "x plus/minus y"
            while (Match(MINUS, PLUS))
            {
                Token op = Previous();
                Expression right = Trigo();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Trigo()
        {
            // "Sinus/Kosinus/Tangens/Arkussinus/Arkuskosinus/Arkustangens/Hyperbeksinus/Hyperbelkosinus/Hyperbeltangens von x"
            while (Match(SINUS, KOSINUS, TANGENS, ARKUSSINUS, ARKUSKOSINUS, ARKUSTANGENS, HYPERBELSINUS, HYPERBELKOSINUS, HYPERBELTANGENS))
            {
                Token op = Previous();
                Consume(VON, "fehlt von");
                Expression right = Faktor();
                return new Expression.Unary(op, right);
            }

            return Faktor();
        }

        private Expression Faktor()
        {
            Expression expr = Wurzel();

            // "x durch/mal/modulo y"
            while (Match(DURCH, MAL, MODULO))
            {
                Token op = Previous();
                Expression right = Wurzel();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Wurzel()
        {
            Expression expr = Potenz();

            // "x. wurzel von y"
            if (Match(PUNKT))
            {
                if (Match(WURZEL))
                {
                    Token op = Previous();
                    Consume(VON, Fehlermeldungen.tokenMissing("dem Wurzel operator", "'von'"));
                    Expression right = Potenz();
                    expr = new Expression.Binary(right, op, expr);
                }
                else
                {
                    current--; // eine anweisung wie: "print 2,0." Matched PUNKT, aber nicht Wurzel wird aber trotzdem Consumed.
                }
            }

            return expr;
        }

        private Expression Potenz()
        {
            Expression expr = Unär();

            // "x hoch y"
            while (Match(HOCH))
            {
                Token op = Previous();
                Expression right = Unär();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Unär()
        {
            // "logisch nicht x"
            if (Match(LOGISCH))
            {
                if (Match(NICHT))
                {
                    Token op = Previous();
                    Expression right = Unär();
                    return new Expression.Unary(op, right);
                }
                else
                {
                    current--;
                }
            }

            // "ln x"
            if (Match(LOG))
            {
                Token op = Previous();
                Expression right = Unär();
                return new Expression.Unary(op, right);
            }

            // "der Betrag von x"
            if (Match(DER))
            {
                if (Match(BETRAG))
                {
                    Token op = Previous();
                    Consume(VON, Fehlermeldungen.tokenMissing("dem Betrag operator", "'von'"));
                    Expression right = Unär();
                    return new Expression.Unary(op, right);
                }
                else
                {
                    current--;
                }
            }

            // "nicht/- x"
            if (Match(NICHT, BANG_MINUS))
            {
                Token op = Previous();
                Expression right = Unär();
                return new Expression.Unary(op, right);
            }

            return Aufruf();
        }

        private Expression Aufruf()
        {
            Expression expr = Primär();

            // "x(y, z)"
            if (Match(L_KLAMMER))
            {
                List<Expression> arguments = new();
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

                Token paren = Consume(R_KLAMMER, Fehlermeldungen.parameterParenMissing);

                expr = new Expression.Call(expr, paren, arguments);
            }

            return expr;
        }

        private Expression Primär()
        {
            if (Match(FALSCH)) return new Expression.Literal(false);
            if (Match(WAHR)) return new Expression.Literal(true);
            if (Match(PI)) return new Expression.Literal(System.Math.PI);
            if (Match(TAU)) return new Expression.Literal(System.Math.Tau);
            if (Match(E)) return new Expression.Literal(System.Math.E);

            if (Match(INT, FLOAT, STRING, CHAR))
            {
                return new Expression.Literal(Previous().literal);
            }

            if (Match(IDENTIFIER))
            {
                return new Expression.Variable(Previous());
            }

            if (Match(L_KLAMMER))
            {
                Expression expr = Ausdruck();
                Consume(R_KLAMMER, Fehlermeldungen.groupingParenMissing);
                return new Expression.Grouping(expr);
            }

            throw Error(Peek(), Fehlermeldungen.expressionMissing);
        }

        /// <summary>
        /// Advances if one of the given TokenTypes is the current Token
        /// </summary>
        /// <param name="types">The TokenTypes to check for</param>
        /// <returns>true there was a match, false if otherwise</returns>
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
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
        private bool Match(out Token matched, params TokenType[] types)
        {
            foreach (TokenType type in types)
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
        /// <see cref="Match(TokenType[])">Matches</see> the given TokenTypes <paramref name="repeat"/> times
        /// </summary>
        /// <param name="repeat">how many times Match should be repeated</param>
        /// <param name="types">what TokenTypes to match</param>
        /// <returns>true if one of the TokenTypes have been matched <paramref name="repeat"/> times</returns>
        private bool Match(int repeat, params TokenType[] types)
        {
            if (repeat == 0) return false;

            for (int i = 0; i < repeat; i++)
            {
                if (!Match(types))
                {
                    while (i > 0)
                    {
                        i--;
                        current--;
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Advances if the the current Token is the given <paramref name="type" /> and throws an error if it's not
        /// </summary>
        /// <param name="type">The TokenType to check for</param>
        /// <param name="message">The error message to show if the check failed</param>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        /// <summary>
        /// Checks if the <see cref="current">current</see> Token is the given TokenType
        /// </summary>
        /// <param name="type">The TokenType to check for</param>
        /// <returns>true if the current Token is the given TokenType. Returns false if the current Token is <see cref="EOF"/></returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd) return false;
            return Peek().type == type;
        }

        /// <summary>
        /// Increments <see cref="current">current</see> if the current Token is not <see cref="EOF"/>
        /// </summary>
        /// <returns>the now previous Token</returns>
        private Token Advance()
        {
            if (!IsAtEnd) current++;
            return Previous();
        }

        /// <returns>the <see cref="current">current</see> Token</returns>
        private Token Peek()
        {
            return tokens[current];
        }

        /// <returns>the previous Token</returns>
        private Token Previous()
        {
            return tokens[current - 1];
        }

        private static ParseError Error(Token token, string message)
        {
            DDP.Fehler(token, message);
            return new ParseError(token, message);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().type == PUNKT) return;

                switch (Peek().type)
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
