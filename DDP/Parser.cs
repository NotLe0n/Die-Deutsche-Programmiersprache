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
                statements.Add(Declaration());
            }

            return statements;
        }

        private Statement Declaration()
        {
            try
            {
                if (Match(DER)) return VarDeclaration(DER);
                if (Match(DIE)) return Match(FUNKTION) ? Function() : VarDeclaration(DIE);

                if (Match(DAS)) return VarDeclaration(DAS);

                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                _ = error; // get rid of annyoing warning
                return null;
            }
        }

        private Statement Statement()
        {
            if (Match(FÜR)) return ForStatement();
            if (Match(WENN)) return IfStatement();
            if (Match(GIB)) return ReturnStatement();
            if (Match(SOLANGE)) return WhileStatement();
            if (Match(MACHE)) return DoWhileStatement();
            if (Match(DOPPELPUNKT)) return new Statement.Block(Block());

            return ExpressionStatement();
        }

        private Statement ExpressionStatement()
        {
            Expression expr = Expression();
            Consume(PUNKT, ErrorMessages.dotAfterExpression);
            return new Statement.Expression(expr);
        }

        private Expression Expression()
        {
            return Assignment();
        }

        private Statement ForStatement()
        {
            Consume(JEDE, ErrorMessages.tokenMissing("einer für anweisung", "'jede'"));

            Statement.Var initializer;
            Expression min;
            if (Match(out Token matched, ZAHL, KOMMAZAHL, ZEICHENKETTE, ZEICHEN))
            {
                Token name = Consume(IDENTIFIER, ErrorMessages.varNameExpected);
                Consume(VON, ErrorMessages.tokenMissing("der Variablendeklaration in einer für anweisung", "'von'"));
                min = Expression();

                initializer = new Statement.Var(matched, name, min);
            }
            else throw Error(Previous(), ErrorMessages.forNoVar);

            Consume(BIS, ErrorMessages.tokenMissing("dem minimum in einer für anweisung", "'bis'"));
            Expression max = Expression();

            Expression schrittgröße = new Expression.Literal(1);
            if (Match(MIT))
            {
                Consume(SCHRITTGRÖßE, ErrorMessages.tokenMissing("'mit' in einer für anweisung", "'schrittgröße'"));

                schrittgröße = Expression();
            }

            Consume(KOMMA, "Es wird ein komma erwartet!");
            Consume(MACHE, ErrorMessages.tokenMissingAtEnd("einer für Anweisung", "ein 'mache'"));

            Statement body = Statement();

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

        private Statement IfStatement()
        {
            var token = Previous();

            Expression condition = Expression();
            Consume(KOMMA, ErrorMessages.ifKommaMissing);
            Consume(DANN, ErrorMessages.ifDannMissing);

            Statement thenBranch = Statement();
            Match(depth, TAB); // consume all tabs (workaround to block issue)
            Statement elseBranch = null;
            if (Match(WENN))
            {
                if (Match(ABER))
                {
                    elseBranch = IfStatement();
                }
                else current--;
            }

            if (Match(SONST))
            {
                elseBranch = Statement();
            }

            var stmt = new Statement.If(condition, thenBranch, elseBranch);
            stmt.token = token;
            return stmt;
        }

        private Statement VarDeclaration(TokenType artikel)
        {
            Token type = CheckArtikel(artikel);
            Token name = Consume(IDENTIFIER, ErrorMessages.varNameExpected);

            Expression initializer = null;
            if (Match(IST))
            {
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
                initializer = Expression();

                if (negate)
                {
                    initializer = new Expression.Unary(new Token(NICHT, "nicht", null, name.line, name.position), initializer);
                }
            }

            Consume(PUNKT, ErrorMessages.dotAfterVarDeclaration);
            return new Statement.Var(type, name, initializer);
        }

        private Token CheckArtikel(TokenType artikel)
        {
            Token type;
            switch (artikel)
            {
                case DER:
                    type = Consume(BOOLEAN, ErrorMessages.wrongArtikel("'der'", "zum Typ Boolean"));
                    break;
                case DIE:
                    if (Match(out var matched, ZAHL, KOMMAZAHL, ZEICHENKETTE))
                    {
                        type = matched;
                    }
                    else
                    {
                        throw Error(Previous(), ErrorMessages.wrongArtikel("'die'", "zu den Typen Zahl, Kommazahl, oder Zeichenkette"));
                    }
                    break;
                case DAS:
                    type = Consume(ZEICHEN, ErrorMessages.wrongArtikel("'das'", "zum Typ Zeichen"));
                    break;
                default:
                    throw Error(Previous(), ErrorMessages.noArtikel);
            }

            return type;
        }

        private Statement WhileStatement()
        {
            var token = Previous();

            Expression condition = Expression();
            Consume(KOMMA, ErrorMessages.tokenMissing("der Bedingung einer solange-Anweisung", "ein komma"));
            Consume(MACHE, ErrorMessages.tokenMissingAtEnd("einer solange Anweisung", "ein 'mache'"));
            Statement body = Statement();

            var stmt = new Statement.While(condition, body);
            stmt.token = token;
            return stmt;
        }

        private Statement DoWhileStatement()
        {
            var token = Previous();

            Statement body = Statement();
            Consume(SOLANGE, ErrorMessages.tokenMissing("einem mache-block", "'solange'"));
            Expression condition = Expression();
            Consume(PUNKT, ErrorMessages.dotAfterExpression);

            var stmt = new Statement.DoWhile(condition, body);
            stmt.token = token;
            return stmt;
        }

        private Statement.Function Function()
        {
            Token name = Consume(IDENTIFIER, ErrorMessages.funcNameExpected);
            Token typ = null;
            List<Token> parameters = new();

            // handle parameters
            Consume(L_KLAMMER, ErrorMessages.tokenMissing("dem Funktionsname", "eine Klammer auf"));

            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), ErrorMessages.tooManyParameters);
                }

                if (Match(ZAHL, KOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
                {
                    parameters.Add(Consume(IDENTIFIER, ErrorMessages.parameterNameExpected));
                }
            } while (Match(KOMMA));

            Consume(R_KLAMMER, ErrorMessages.tokenMissing("den Argumenten", "eine Klammer zu"));

            // return type
            if (Match(VOM))
            {
                Consume(TYP, ErrorMessages.tokenMissing("einer vom Anweisung", "ein Typ"));
                if (Match(out Token match, ZAHL, KOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
                {
                    typ = match;
                }
                else
                {
                    Error(Peek(), ErrorMessages.returnTypeInvalid);
                }
            }

            Consume(MACHT, ErrorMessages.tokenMissingAtEnd("variablen deklaration", "ein 'macht'"));
            Consume(DOPPELPUNKT, ErrorMessages.tokenMissing("einer macht anweisung", "ein doppelpunkt"));

            List<Statement> body = Block();

            // require return statement if return type is present
            if (typ != null)
            {
                foreach (var stmt in body)
                {
                    if (stmt is Statement.Return)
                        return new Statement.Function(name, parameters, typ, body);
                }
                Error(name, ErrorMessages.returnMissing);
            }

            return new Statement.Function(name, parameters, typ, body);
        }

        private Statement ReturnStatement()
        {
            Token keyword = Previous();
            Expression value = null;
            if (!Match(PUNKT))
            {
                value = Expression();
            }

            Consume(ZURÜCK, ErrorMessages.tokenMissing("einem Rückgabe-Wert", "'zurück'"));
            Consume(PUNKT, ErrorMessages.tokenMissing("einer Rückgabe-Anweisung", "ein punkt"));
            return new Statement.Return(keyword, value);
        }

        private List<Statement> Block()
        {
            depth++;
            List<Statement> statements = new();

            while (Match(depth, TAB) && !IsAtEnd)
            {
                statements.Add(Declaration());
            }

            depth--;

            return statements;
        }

        private Expression Assignment()
        {
            Expression expr = Or();

            if (Match(IST))
            {
                Token equals = Previous();
                Expression value = Assignment();

                if (expr is Expression.Variable variable)
                {
                    Token name = variable.name;
                    return new Expression.Assign(name, value);
                }

                Error(equals, ErrorMessages.varInvalidAssignment);
            }

            return expr;
        }

        private Expression Or()
        {
            Expression expr = And();

            while (Match(ODER))
            {
                Token op = Previous();
                Expression right = And();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression And()
        {
            Expression expr = Equality();

            while (Match(UND))
            {
                Token op = Previous();
                Expression right = Equality();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression Equality()
        {
            Expression expr = Comparison();

            while (Match(UNGLEICH, GLEICH))
            {
                Token op = Previous();
                Expression right = Comparison();
                expr = new Expression.Binary(expr, op, right);

                Consume(IST, ErrorMessages.tokenMissing("einem Vergleich", "'ist'"));
            }

            return expr;
        }

        private Expression Comparison()
        {
            Expression expr = Bitweise();

            while (Match(GRÖßER, KLEINER))
            {
                // Check if ALS is present
                if (!Check(ALS))
                {
                    Error(Previous(), ErrorMessages.tokenMissing("einem größer/kleiner operator", "'als'"));
                    continue;
                }

                Token op = Previous();
                Advance(); // current: ALS

                // handle "größer/kleiner als, oder gleich"
                if (Match(KOMMA) && Match(ODER))
                {
                    if (op.type == GRÖßER)
                        op.type = GRÖßER_GLEICH;
                    else if (op.type == KLEINER)
                        op.type = KLEINER_GLEICH;
                }

                Expression right = Bitweise();

                Consume(IST, ErrorMessages.tokenMissing("einem Vergleich", "'ist'"));
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Bitweise()
        {
            while (Match(LOGISCH))
            {
                Expression _expr = Term();
                Token op = Advance();
                Expression right = Term();
                return new Expression.Binary(_expr, op, right);
            }

            Expression expr = Term();

            while (Match(UM))
            {
                Expression right = Term();
                Consume(BIT, ErrorMessages.tokenMissing("dem verschiebungswert", "'bit'"));
                Consume(NACH, ErrorMessages.tokenMissing("einer bit Anweisung", "ein 'nach'"));
                Token op = Advance();
                Consume(VERSCHOBEN, ErrorMessages.tokenMissingAtEnd("einer Bitverschiebungsanweisung", "ein 'verschoben'"));

                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Term()
        {
            Expression expr = Factor();

            while (Match(MINUS, PLUS))
            {
                Token op = Previous();
                Expression right = Factor();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Factor()
        {
            Expression expr = Wurzel();

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

            if (Match(PUNKT))
            {
                if (Match(WURZEL))
                {
                    Token op = Previous();
                    Consume(VON, ErrorMessages.tokenMissing("dem Wurzel operator", "'von'"));
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
            Expression expr = Unary();

            while (Match(HOCH))
            {
                Token op = Previous();
                Expression right = Unary();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression Unary()
        {
            if (Match(LOGISCH))
            {
                if (Match(NICHT))
                {
                    Token op = Previous();
                    Expression right = Unary();
                    return new Expression.Unary(op, right);
                }
                else
                {
                    current--;
                }
            }

            if (Match(LOG))
            {
                Token op = Previous();
                Expression right = Unary();
                return new Expression.Unary(op, right);
            }

            if (Match(DER))
            {
                if (Match(BETRAG))
                {
                    Token op = Previous();
                    Consume(VON, ErrorMessages.tokenMissing("dem Betrag operator", "'von'"));
                    Expression right = Unary();
                    return new Expression.Unary(op, right);
                }
                else
                {
                    current--;
                }
            }

            if (Match(NICHT, BANG_MINUS))
            {
                Token op = Previous();
                Expression right = Unary();
                return new Expression.Unary(op, right);
            }

            return Call();
        }

        private Expression Call()
        {
            Expression expr = Primary();

            if (Match(L_KLAMMER))
            {
                List<Expression> arguments = new();
                if (!Check(R_KLAMMER))
                {
                    do
                    {
                        if (arguments.Count >= 255)
                        {
                            Error(Peek(), ErrorMessages.tooManyParameters);
                        }
                        arguments.Add(Expression());
                    } while (Match(KOMMA));
                }

                Token paren = Consume(R_KLAMMER, ErrorMessages.parameterParenMissing);

                expr = new Expression.Call(expr, paren, arguments);
            }

            return expr;
        }

        private Expression Primary()
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
                Expression expr = Expression();
                Consume(R_KLAMMER, ErrorMessages.groupingParenMissing);
                return new Expression.Grouping(expr);
            }

            throw Error(Peek(), ErrorMessages.expressionMissing);
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
            DDP.Error(token, message);
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
