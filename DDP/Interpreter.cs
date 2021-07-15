﻿using DDP.Eingebaute_Funktionen;
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
            globals.Define("schreibe", new Schreibe());
            globals.Define("schreibeZeile", new SchreibeZeile());
            globals.Define("lese", new Lese());
            globals.Define("leseZeile", new LeseZeile());
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
            object value = Evaluate(stmt.condition);
            if (value is bool)
            {
                if (value.Equals(true))
                {
                    Execute(stmt.thenBranch);
                }
                else if (stmt.elseBranch != null)
                {
                    Execute(stmt.elseBranch);
                }
            }
            else
            {
                throw new RuntimeError(stmt.token, "wenn anweisung braucht boolean");
            }
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
                        throw new RuntimeError(stmt.name, $"Die variable {stmt.name.lexeme} kann nur einer Zahl zugewiesen werden.");
                    break;
                case FLIEßKOMMAZAHL:
                    if (value is not double)
                        throw new RuntimeError(stmt.name, $"Die variable {stmt.name.lexeme} kann nur einer Fließkommazahl zugewiesen werden.");
                    break;
                case ZEICHENKETTE:
                    if (value is not string)
                        throw new RuntimeError(stmt.name, $"Die variable {stmt.name.lexeme} kann nur einer Zeichenkette zugewiesen werden.");
                    break;
                case ZEICHEN:
                    if (value is not char)
                        throw new RuntimeError(stmt.name, $"Die variable {stmt.name.lexeme} kann nur einem Zeichen zugewiesen werden.");
                    break;
                case BOOLEAN:
                    if (value is not bool)
                        throw new RuntimeError(stmt.name, $"Die variable {stmt.name.lexeme} kann nur einem Boolean zugewiesen werden.");
                    break;
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Statement.While stmt)
        {
            object value = Evaluate(stmt.condition);
            if (value is bool)
            {
                while (value.Equals(true))
                {
                    Execute(stmt.body);
                }
            }
            else
            {
                throw new RuntimeError(stmt.token, "solange anweisung braucht boolean");
            }
            return null;
        }

        public object VisitDoWhileStmt(Statement.DoWhile stmt)
        {
            object value = Evaluate(stmt.condition);
            if (value is bool)
            {
                do
                {
                    Execute(stmt.body);
                }
                while (value.Equals(true));
            }
            else
            {
                throw new RuntimeError(stmt.token, "solange anweisung braucht boolean");
            }
            return null;
        }

        public object VisitForStmt(Statement.For stmt)
        {
            dynamic min = Evaluate(stmt.min);
            dynamic max = Evaluate(stmt.max);
            dynamic inc = Evaluate(stmt.inc);

            if (min is not int && max is not int && inc is not int && min is not double &&  max is not double && inc is not double)
                throw new RuntimeError(stmt.initializer.name, "wrong type in for loop");

            if (min < max)
            {
                for (dynamic i = min; i <= max; i += inc)
                {
                    Execute(stmt.body);
                }
            }
            else
            {
                for (dynamic i = min; i >= max; i += inc)
                {
                    Execute(stmt.body);
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

            Type type = CheckOperandTypes(expr.op, left, right);

            left = Convert.ChangeType(left, type);
            right = Convert.ChangeType(right, type);

            switch (expr.op.type)
            {
                case UNGLEICH:
                    if (type == typeof(bool)) return !left.Equals(right);
                    throw new RuntimeError(expr.op, "Operanden können nur Booleans sein.");
                case GLEICH:
                    if (type == typeof(bool)) return left.Equals(right);
                    throw new RuntimeError(expr.op, "Operanden können nur Booleans sein.");
                case GRÖßER:
                    if (type == typeof(double)) return (double)left > (double)right;
                    if (type == typeof(int)) return (int)left > (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case GRÖßER_GLEICH:
                    if (type == typeof(double)) return (double)left >= (double)right;
                    if (type == typeof(int)) return (int)left >= (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case KLEINER:
                    if (type == typeof(double)) return (double)left < (double)right;
                    if (type == typeof(int)) return (int)left < (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case KLEINER_GLEICH:
                    if (type == typeof(double)) return (double)left <= (double)right;
                    if (type == typeof(int)) return (int)left <= (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case MODULO:
                    if (type == typeof(int)) return (int)left % (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur integrale zahlen sein.");
                case MINUS:
                    if (type == typeof(double)) return (double)left - (double)right;
                    if (type == typeof(int)) return (int)left - (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case PLUS:
                    if (type == typeof(double)) return (double)left + (double)right;
                    if (type == typeof(int)) return (int)left + (int)right;
                    if (left is string || right is string) return Stringify(left) + Stringify(right);
                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
                case DURCH:
                    if (type == typeof(double)) return (double)left / (double)right;
                    if (type == typeof(int)) return (int)left / (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case MAL:
                    if (type == typeof(double)) return (double)left * (double)right;
                    if (type == typeof(int)) return (int)left * (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case HOCH:
                    if (type == typeof(double)) return Math.Pow((double)left, (double)right);
                    if (type == typeof(int)) return Math.Pow((int)left, (int)right);
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case WURZEL:
                    if (type == typeof(double)) return Math.Pow((double)left, 1 / (double)right);
                    if (type == typeof(int)) return Math.Pow((int)left, 1.0 / (int)right);
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case UND:
                    if (type == typeof(int)) return (int)left & (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case ODER:
                    if (type == typeof(int)) return (int)left | (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case KONTRA:
                    if (type == typeof(int)) return (int)left ^ (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case LINKS:
                    if (type == typeof(int)) return (int)left << (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
                case RECHTS:
                    if (type == typeof(int)) return (int)left >> (int)right;
                    throw new RuntimeError(expr.op, "Operanden können nur zahlen sein.");
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
            if (arguments.Count != function.Arity)
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
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
                if (left.Equals(true)) return left;
            }
            else
            {
                if (left.Equals(false)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitUnaryExpr(Expression.Unary expr)
        {
            object right = Evaluate(expr.right);

            Type type = CheckOperandTypes(expr.op, expr.right);
            right = Convert.ChangeType(right, type);

            switch (expr.op.type)
            {
                case NICHT:
                    if (type == typeof(bool)) return right.Equals(false);
                    if (type == typeof(int)) return ~(int)right;
                    throw new RuntimeError(expr.op, "nicht operator nimmt nur Boolean oder Zahl.");
                case BANG_MINUS:
                    if (type == typeof(double)) return -(double)right;
                    if (type == typeof(int)) return -(int)right;
                    throw new RuntimeError(expr.op, "- operator nimmt Zahlen.");
                case BETRAG:
                    if (type == typeof(double)) return Math.Abs((double)right);
                    if (type == typeof(int)) return Math.Abs((int)right);
                    throw new RuntimeError(expr.op, "betrag operator nimmt Zahlen.");
                case LOG:
                    if (type == typeof(double)) return Math.Log((double)right);
                    if (type == typeof(int)) return Math.Log((int)right);
                    throw new RuntimeError(expr.op, "ln operator nimmt Zahlen.");
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

        private Type CheckOperandTypes(Token op, object operand)
        {
            if (operand is double) return typeof(double);
            if (operand is int) return typeof(int);
            if (operand is bool) return typeof(bool);
            if (operand is string) return typeof(string);
            if (operand is char) return typeof(char);

            throw new RuntimeError(op, "Ungültiger Operanden Typ");
        }

        private Type CheckOperandTypes(Token op, object left, object right)
        {

            if ((left is double && right is int) || (left is int && right is double) || (left is double && right is double))
                return typeof(double);

            if (left is int && right is int)
                return typeof(int);

            if (left is bool && right is bool)
                return typeof(bool);

            if (left is string && right is string)
                return typeof(string);

            if (left is char && right is char)
                return typeof(char);

            throw new RuntimeError(op, "Ungültiger Operanden Typ");
        }

        private static string Stringify(object obj)
        {
            if (obj == null) return "nix";

            if (obj is bool boolean)
            {
                return boolean ? "wahr" : "falsch";
            }

            return obj.ToString();
        }
    }
}
