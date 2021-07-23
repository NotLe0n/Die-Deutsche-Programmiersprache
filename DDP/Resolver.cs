using System.Collections.Generic;

namespace DDP
{
    class Resolver : Expression.IVisitor<object>, Statement.IVisitor<object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new();

        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION
        }

        public void Resolve(List<Statement> statements)
        {
            foreach (Statement statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Statement stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expression expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Statement.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (Token param in function.param)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        public object VisitBlockStmt(Statement.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitExpressionStmt(Statement.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Statement.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitIfStmt(Statement.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitReturnStmt(Statement.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                DDP.Fehler(stmt.keyword, Fehlermeldungen.returnNotInFunc);
            }

            if (stmt.value != null)
            {
                Resolve(stmt.value);
            }

            return null;
        }

        public object VisitVarStmt(Statement.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Statement.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitDoWhileStmt(Statement.DoWhile stmt)
        {
            Resolve(stmt.body);
            Resolve(stmt.condition);
            return null;
        }

        public object VisitForStmt(Statement.For stmt)
        {
            Resolve(stmt.min);
            Resolve(stmt.max);
            Resolve(stmt.body);
            return null;
        }

        public object VisitAssignExpr(Expression.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expression.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expression.Call expr)
        {
            Resolve(expr.callee);

            foreach (Expression argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Expression.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expression.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expression.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitUnaryExpr(Expression.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expression.Variable expr)
        {
            if (IsDeclaredExact(expr.name.lexeme, false) == true)
            {
                DDP.Fehler(expr.name, Fehlermeldungen.varDefineInInit);
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private bool? IsDeclaredExact(string name, bool value)
        {
            if (scopes.TryPeek(out var scope))
            {
                if (scope.TryGetValue(name, out var v))
                {
                    return v == value;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();

            if (scope.ContainsKey(name.lexeme))
            {
                DDP.Fehler(name, Fehlermeldungen.varAlreadyExists);
            }

            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expression expr, Token name)
        {
            // Stack.ToArray returns a reversed array. The first pushed item will be the last in the array.
            var _scopes = scopes.ToArray();

            for (var i = 0; i < _scopes.Length; i++)
            {
                if (_scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, i);
                    return;
                }
            }

            // Not found. Assume it is global.
        }
    }
}