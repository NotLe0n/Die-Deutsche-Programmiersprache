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
            Consume(PUNKT, "Satzzeichen beachten! Ein Punkt muss nach einer Anweisung folgen!");
            return new Statement.Expression(expr);
        }

        private Expression Expression()
        {
            return Assignment();
        }

        private Statement ForStatement()
        {
            Consume(JEDE, "da fehlt jede");

            Statement.Var initializer;
            Expression min;
            if (Match(out Token matched, ZAHL, FLIEßKOMMAZAHL, ZEICHENKETTE, ZEICHEN))
            {
                Token name = Consume(IDENTIFIER, "Expect variable name.");
                Consume(VON, "da fehlt von");
                min = Expression();

                initializer = new Statement.Var(matched, name, min);
            }
            else throw Error(Previous(), "keine variablen deklaration");

            Consume(BIS, "da fehlt bis");
            Expression max = Expression();

            Expression schrittgröße = new Expression.Literal(1);
            if (Match(MIT))
            {
                Consume(SCHRITTGRÖßE, "da fehlt schrittgröße");

                schrittgröße = Expression();
            }

            Consume(KOMMA, "da fehlt komma");
            Consume(MACHE, "da fehlt mache");

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
            Expression condition = Expression();
            Consume(KOMMA, "Expect , after if condition.");
            Consume(DANN, "Expect dann after if condition.");

            Statement thenBranch = Statement();
            Statement elseBranch = null;
            if (Match(WENN) && Match(ABER))
            {
                elseBranch = IfStatement();
            }

            if (Match(SONST))
            {
                elseBranch = Statement();
            }

            return new Statement.If(condition, thenBranch, elseBranch);
        }

        private Statement VarDeclaration(TokenType artikel)
        {
            Token type = CheckArtikel(artikel);
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expression initializer = null;
            if (Match(IST))
            {
                initializer = Expression();
            }

            Consume(PUNKT, "Satzzeichen beachten! Ein Punkt muss nach einer Variablen Deklaration folgen!");
            return new Statement.Var(type, name, initializer);
        }

        private Token CheckArtikel(TokenType artikel)
        {
            Token type;
            switch (artikel)
            {
                case DER:
                    type = Consume(BOOLEAN, "der artikel passt nicht.");
                    break;
                case DIE:
                    if (Match(out var matched, ZAHL, FLIEßKOMMAZAHL, ZEICHENKETTE))
                    {
                        type = matched;
                    }
                    else
                    {
                        throw Error(Previous(), "der artikel passt nicht.");
                    }
                    break;
                case DAS:
                    type = Consume(ZEICHEN, "der artikel passt nicht.");
                    break;
                default:
                    throw Error(Previous(), "da fehlt ein artikel");
            }

            return type;
        }

        private Statement WhileStatement()
        {
            Expression condition = Expression();
            Consume(IST, "Da fehlt \"ist\"");
            Consume(KOMMA, "Da fehlt \",\"");
            Consume(MACHE, "Da fehlt \"mache\"");
            Statement body = Statement();

            return new Statement.While(condition, body);
        }

        private Statement DoWhileStatement()
        {
            Statement body = Statement();

            Consume(SOLANGE, "Da fehlt \"While\"");
            Expression condition = Expression();
            Consume(IST, "Da fehlt \"ist\"");
            Consume(PUNKT, "Da fehlt \".\"");

            return new Statement.DoWhile(condition, body);
        }

        private Statement.Function Function()
        {
            Token name = Consume(IDENTIFIER, "funktion braucht name bruh");
            Token typ = null;
            List<Token> parameters = new();

            // handle parameters
            Consume(L_KLAMMER, "da fehlt linke klammer");

            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                if (Match(ZAHL, FLIEßKOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
                {
                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                }
            } while (Match(KOMMA));

            Consume(R_KLAMMER, "fehlt rechte klammer");

            // return type
            if (Match(VOM))
            {
                Consume(TYP, "da fehlt typ");
                if (Match(out Token match, ZAHL, FLIEßKOMMAZAHL, BOOLEAN, CHAR, ZEICHENKETTE))
                {
                    typ = match;
                }
                else
                {
                    Error(Peek(), "da fehlt ein typ");
                }
            }

            Consume(MACHT, "fehlt macht");
            Consume(DOPPELPUNKT, "fehlt doppelpunkt");

            List<Statement> body = Block();

            // require return statement if return type is present
            if (typ != null)
            {
                foreach (var stmt in body)
                {
                    if (stmt is Statement.Return)
                    {
                        return new Statement.Function(name, parameters, body);
                    }
                }
                Error(name, "da fehlt ein return");
            }

            return new Statement.Function(name, parameters, body);
        }

        private Statement ReturnStatement()
        {
            Token keyword = Previous();
            Expression value = null;
            if (!Match(PUNKT))
            {
                value = Expression();
            }

            Consume(ZURÜCK, "fehlt zurück");
            Consume(PUNKT, "Expect '.' after return value.");
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

                Error(equals, "Invalid assignment target.");
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

                Consume(IST, "da fehlt ist");
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
                    Error(Previous(), "Da fehlt ein \"als\"");
                    continue;
                }

                Token op = Previous();
                Advance(); // current: ALS

                // handle "größer/kleiner als, oder gleich"
                if (Match(KOMMA) && Match(ODER) && Match(GLEICH))
                {
                    if (op.type == GRÖßER)
                        op.type = GRÖßER_GLEICH;
                    else if (op.type == KLEINER)
                        op.type = GRÖßER_GLEICH;
                }

                Expression right = Bitweise();
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
                Consume(BIT, "fehlt bit");
                Consume(NACH, "fehlt nach");
                Token op = Advance();
                Consume(VERSCHOBEN, "fehlt verschoben");

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
                    Consume(VON, "fehlt von");
                    Expression right = Potenz();
                    expr = new Expression.Binary(right, op, expr);
                }
                else
                {
                    current--; // eine anweisung wie: "print 2,0." Matched PUNKT, aber nicht Wurzel wird, aber trotzdem Consumed.
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
                    Consume(VON, "da fehlt von");
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
                            Error(Peek(), "Can't have more than 255 arguments.");
                        }
                        arguments.Add(Expression());
                    } while (Match(KOMMA));
                }

                Token paren = Consume(R_KLAMMER, "Expect ')' after arguments.");

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
                Consume(R_KLAMMER, "Expect ')' after expression.");
                return new Expression.Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
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
            for (int i = 0; i < repeat; i++)
            {
                if (!Match(types))
                {
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
