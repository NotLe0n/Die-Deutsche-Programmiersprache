using System.Collections.Generic;

namespace DDP
{
    internal class Resolver : Ausdruck.IVisitor<object>, Anweisung.IVisitor<object>
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

        public void Resolve(List<Anweisung> statements)
        {
            foreach (Anweisung statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Anweisung stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Ausdruck expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Anweisung.Funktion function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach ((Symbol typ, Symbol arg) param in function.argumente)
            {
                Declare(param.arg);
                Define(param.arg);
            }
            Resolve(function.körper);
            EndScope();

            currentFunction = enclosingFunction;
        }

        private void ResolveLocal(Ausdruck expr, Symbol name)
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

        public object VisitBlockStmt(Anweisung.Block stmt)
        {
            BeginScope();
            Resolve(stmt.anweisungen);
            EndScope();
            return null;
        }

        public object VisitExpressionStmt(Anweisung.Ausdruck stmt)
        {
            Resolve(stmt.ausdruck);
            return null;
        }

        public object VisitFunctionStmt(Anweisung.Funktion stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitIfStmt(Anweisung.Wenn stmt)
        {
            Resolve(stmt.bedingung);
            Resolve(stmt.dannZweig);
            if (stmt.sonstZweig != null) Resolve(stmt.sonstZweig);
            return null;
        }

        public object VisitReturnStmt(Anweisung.Rückgabe stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                DDP.Fehler(stmt.schlüsselwort, Fehlermeldungen.returnNotInFunc);
            }

            if (stmt.wert != null)
            {
                Resolve(stmt.wert);
            }

            return null;
        }

        public object VisitVarStmt(Anweisung.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializierer != null)
            {
                Resolve(stmt.initializierer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Anweisung.Solange stmt)
        {
            Resolve(stmt.bedingung);
            Resolve(stmt.körper);
            return null;
        }

        public object VisitDoWhileStmt(Anweisung.MacheSolange stmt)
        {
            Resolve(stmt.körper);
            Resolve(stmt.bedingung);
            return null;
        }

        public object VisitForStmt(Anweisung.Für stmt)
        {
            Resolve(stmt.min);
            Resolve(stmt.max);
            Resolve(stmt.körper);
            return null;
        }

        public object VisitAssignExpr(Ausdruck.Zuweisung expr)
        {
            Resolve(expr.wert);
            if (expr.stelle != null) Resolve(expr.stelle);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Ausdruck.Binär expr)
        {
            Resolve(expr.links);
            Resolve(expr.rechts);
            return null;
        }

        public object VisitCallExpr(Ausdruck.Aufruf expr)
        {
            Resolve(expr.aufrufer);

            foreach (Ausdruck argument in expr.argumente)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Ausdruck.Gruppierung expr)
        {
            Resolve(expr.ausdruck);
            return null;
        }

        public object VisitLiteralExpr(Ausdruck.Wert expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Ausdruck.Logisch expr)
        {
            Resolve(expr.links);
            Resolve(expr.rechts);
            return null;
        }

        public object VisitUnaryExpr(Ausdruck.Unär expr)
        {
            Resolve(expr.rechts);
            return null;
        }

        public object VisitVariableExpr(Ausdruck.Variable expr)
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

        private void Declare(Symbol name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();

            if (scope.ContainsKey(name.lexeme))
            {
                DDP.Fehler(name, Fehlermeldungen.varAlreadyExists);
            }

            scope[name.lexeme] = false;
        }

        private void Define(Symbol name)
        {
            if (scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }
    }
}