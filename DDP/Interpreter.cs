using System;
using System.Collections.Generic;

using static DDP.TokenType;

namespace DDP
{
    class Interpreter : Expression.IVisitor<object>, Statement.IVisitor<object>
    {
        public readonly Environment globals = new();
        private Environment environment;

        private readonly Dictionary<Expression, int> locals = new();


        public Interpreter()
        {
            environment = globals;

            globals.Define("clock", new Clock());
        }


        public void Interpret(List<Statement> statements)
        {
            try
            {
                foreach (Statement statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                DDP.RuntimeError(error);
            }
        }

        private object Evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Statement stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expression expr, int depth)
        {
            locals[expr] = depth;
        }

        public object VisitBlockStmt(Statement.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public void ExecuteBlock(List<Statement> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Statement statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitExpressionStmt(Statement.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Statement.Function stmt)
        {
            Function function = new Function(stmt, environment);

            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitIfStmt(Statement.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Statement.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Statement.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        public object VisitVarStmt(Statement.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            switch (stmt.type.type) 
            {
                case ZAHL:
                    if (value is not int)
                        throw new RuntimeError(stmt.name, "THIS IS NOT A INT EWWWWWW");
                    break;
                case FLIEßKOMMAZAHL:
                    if (value is not double)
                        throw new RuntimeError(stmt.name, "THIS IS NOT A DOUBLE EWWWWWW");
                    break;
                case ZEICHENKETTE:
                    if (value is not string)
                        throw new RuntimeError(stmt.name, "THIS IS NOT A STRING EWWWWWW");
                    break;
                case ZEICHEN:
                    if (value is not char)
                        throw new RuntimeError(stmt.name, "THIS IS NOT A CHAR EWWWWWW");
                    break;
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Statement.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitDoWhileStmt(Statement.DoWhile stmt)
        {
            do
            {
                Execute(stmt.body);
            }
            while (IsTruthy(Evaluate(stmt.condition)));
            return null;
        }

        public object VisitForStmt(Statement.For stmt)
        {
            object _min = Evaluate(stmt.min);
            object _max = Evaluate(stmt.max);
            object _inc = Evaluate(stmt.inc);

            if (_min is int min && _max is int max && _inc is int inc)
            {
                if (min < max)
                {
                    for (int i = min; i <= max; i += inc)
                    {
                        Execute(stmt.body);
                    }
                }
                else
                {
                    for (int i = min; i >= max; i += inc)
                    {
                        Execute(stmt.body);
                    }
                }
            }
            return null;
        }

        public object VisitAssignExpr(Expression.Assign expr)
        {
            var value = Evaluate(expr.value);

            if (locals.TryGetValue(expr, out var distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }

            return value;
        }

        public object VisitBinaryExpr(Expression.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case UNGLEICH: return !IsEqual(left, right);
                case GLEICH: return IsEqual(left, right);

                case GRÖßER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case GRÖßER_GLEICH:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case KLEINER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case KLEINER_GLEICH:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case MODULO:
                    CheckNumberOperands(expr.op, left, right);
                    return (int)left % (int)right;
                case MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    if (left is double && right is double)
                        return (double)left - (double)right;

                    if (left is int && right is int)
                        return (int)left - (int)right;
                    break;
                case PLUS:
                    if (left is double dlval && right is double drval)  
                        return dlval + drval;
                    
                    if (left is int ilval && right is int irval)
                        return ilval + irval;
                    
                    if (left is string lstr && right is string rstr)
                    {
                        return lstr + rstr;
                    }

                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
                case DURCH:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;
                case MAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case HOCH:
                    CheckNumberOperands(expr.op, left, right);
                    return Math.Pow((double)left, (double)right);
                case WURZEL:
                    CheckNumberOperands(expr.op, left, right);
                    return Math.Pow((double)left, 1 / (double)right);
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Expression.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new();
            foreach (Expression argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ICallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            ICallable function = (ICallable)callee;
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGroupingExpr(Expression.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expression.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expression.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == ODER)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitUnaryExpr(Expression.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case NICHT:
                    return !IsTruthy(right);
                case BANG_MINUS:
                    CheckNumberOperand(expr.op, right);

                    if (right is double)
                        return -(double)right;
                    if (right is int)
                        return -(int)right;
                    break;
                case STRICH:
                    CheckNumberOperand(expr.op, right);
                    if (right is double)
                        return Math.Abs((double)right);
                    if (right is int)
                        return Math.Abs((int)right);
                    break;
                case LOG:
                    CheckNumberOperand(expr.op, right);
                    if (right is double)
                        return Math.Log((double)right);
                    if (right is int)
                        return Math.Log((int)right);
                    break;
            }

            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expression.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expression expr)
        {
            if (locals.TryGetValue(expr, out var distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double || operand is int) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if ((left is double && right is double) || (left is int && right is int)) return;

            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private static bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool boolean) return boolean;
            return true;
        }

        private static bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private static string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - (text.Length - 2));
                }
                return text;
            }

            if (obj is bool boolean)
            {
                return boolean ? "wahr" : "falsch";
            }

            return obj.ToString();
        }
    }
}
