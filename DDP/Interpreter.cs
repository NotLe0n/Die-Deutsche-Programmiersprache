using DDP.Eingebaute_Funktionen;
using System;
using System.Collections.Generic;
using System.Linq;
using static DDP.SymbolTyp;

namespace DDP
{
    internal class Interpreter : Ausdruck.IVisitor<object>, Anweisung.IVisitor<object>
    {
        public readonly Environment globals = new();
        private Environment environment;

        private readonly Dictionary<Ausdruck, int> locals = new();

        public Interpreter()
        {
            environment = globals;

            globals.Define("clock", new Clock());
            globals.Define("schreibe", new Schreibe());
            globals.Define("schreibeZeile", new SchreibeZeile());
            globals.Define("lese", new Lese());
            globals.Define("leseZeile", new LeseZeile());
            globals.Define("zuZahl", new Umwandeln.Zahl());
            globals.Define("zuKommazahl", new Umwandeln.Kommazahl());
            globals.Define("zuBuchstabe", new Umwandeln.Buchstabe());
            globals.Define("zuText", new Umwandeln.Text());
            globals.Define("zuBoolean", new Umwandeln.Boolean());
            globals.Define("Min", new Mathematik.Min());
            globals.Define("Max", new Mathematik.Max());
            globals.Define("Clamp", new Mathematik.Clamp());
            globals.Define("Trunkiert", new Mathematik.Trunkiert());
            globals.Define("Rund", new Mathematik.Rund());
            globals.Define("Decke", new Mathematik.Decke());
            globals.Define("Boden", new Mathematik.Boden());
            globals.Define("Beschneiden", new Stringmanipulation.Beschneiden());
            globals.Define("Einfügen", new Stringmanipulation.Einfügen());
            globals.Define("Entfernen", new Stringmanipulation.Entfernen());
            globals.Define("Enthält", new Stringmanipulation.Enthält());
            globals.Define("Ersetzten", new Stringmanipulation.Ersetzten());
            globals.Define("Länge", new Arrays.Länge());
            globals.Define("Spalten", new Stringmanipulation.Spalten());
            globals.Define("Zuschneiden", new Stringmanipulation.Zuschneiden());
            globals.Define("warte", new Warte());
            globals.Define("existiertDatei", new IO.ExistiertDatei());
            globals.Define("leseDatei", new IO.LeseDatei());
            globals.Define("leseBytes", new IO.LeseBytes());
            globals.Define("schreibeDatei", new IO.SchreibeDatei());
            globals.Define("schreibeBytes", new IO.SchreibeBytes());
            globals.Define("bearbeiteDatei", new IO.BearbeiteDatei());
            globals.Define("bearbeiteBytes", new IO.BearbeiteBytes());
        }

        public void Interpret(List<Anweisung> statements)
        {
            try
            {
                foreach (Anweisung statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (Laufzeitfehler error)
            {
                DDP.Laufzeitfehler(error);
            }
        }

        private object Evaluate(Ausdruck expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Anweisung stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Ausdruck expr, int depth)
        {
            locals[expr] = depth;
        }

        public object VisitBlockStmt(Anweisung.Block stmt)
        {
            ExecuteBlock(stmt.anweisungen, new Environment(environment));
            return null;
        }

        public void ExecuteBlock(List<Anweisung> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Anweisung statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitExpressionStmt(Anweisung.Ausdruck stmt)
        {
            Evaluate(stmt.ausdruck);
            return null;
        }

        public object VisitFunctionStmt(Anweisung.Funktion stmt)
        {
            var funktion = new Funktion(stmt, environment);

            environment.Define(stmt.name.lexeme, funktion);
            return null;
        }

        public object VisitIfStmt(Anweisung.Wenn stmt)
        {
            object value = Evaluate(stmt.bedingung);
            if (value is bool)
            {
                if (value.Equals(true))
                {
                    Execute(stmt.dannZweig);
                }
                else if (stmt.sonstZweig != null)
                {
                    Execute(stmt.sonstZweig);
                }
            }
            else
            {
                throw new Laufzeitfehler(stmt.token, Fehlermeldungen.ifConditionNotBool);
            }
            return null;
        }

        public object VisitReturnStmt(Anweisung.Rückgabe stmt)
        {
            object wert = null;
            if (stmt.wert != null) wert = Evaluate(stmt.wert);

            throw new Rückgabe(wert);
        }

        public object VisitVarStmt(Anweisung.Var stmt)
        {
            object wert = null;
            if (stmt.initializierer != null)
            {
                wert = Evaluate(stmt.initializierer);
            }

            switch (stmt.typ.typ)
            {
                case ZAHL:
                    if (wert is not int)
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Zahl"));
                    break;
                case KOMMAZAHL:
                    if (wert is not double)
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Kommazahl"));
                    break;
                case TEXT:
                    if (wert is not string)
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einem Text"));
                    break;
                case BUCHSTABEN:
                    if (wert is not char[])
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Buchstaben liste"));
                    break;
                case BUCHSTABE:
                    if (wert is not char)
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einem Buchstabe"));
                    break;
                case BOOLEANS:
                    if (wert is not bool[])
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Boolean liste"));
                    break;
                case BOOLEAN:
                    if (wert is not bool)
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einem Boolean"));
                    break;
                case ZAHLEN:
                    if (wert is not int[])
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Zahlen liste"));
                    break;
                case KOMMAZAHLEN:
                    if (wert is not double[])
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Kommazahlen liste"));
                    break;
                case TEXTE:
                    if (wert is not string[])
                        throw new Laufzeitfehler(stmt.name, Fehlermeldungen.varWrongType(stmt.name.lexeme, "einer Text liste"));
                    break;
            }

            environment.Define(stmt.name.lexeme, wert);
            return null;
        }

        public object VisitWhileStmt(Anweisung.Solange stmt)
        {
            object wert = Evaluate(stmt.bedingung);

            if (wert is bool)
            {
                while (wert.Equals(true))
                {
                    Execute(stmt.körper);
                    wert = Evaluate(stmt.bedingung);
                }
            }
            else
            {
                throw new Laufzeitfehler(stmt.symbol, Fehlermeldungen.whileConditionNotBool);
            }
            return null;
        }

        public object VisitDoWhileStmt(Anweisung.MacheSolange stmt)
        {
            object wert = Evaluate(stmt.bedingung);

            if (wert is bool)
            {
                do
                {
                    Execute(stmt.körper);
                    wert = Evaluate(stmt.bedingung);
                }
                while (wert.Equals(true));
            }
            else
            {
                throw new Laufzeitfehler(stmt.symbol, Fehlermeldungen.whileConditionNotBool);
            }
            return null;
        }

        public object VisitForStmt(Anweisung.Für stmt)
        {
            dynamic min = Evaluate(stmt.min);
            dynamic max = Evaluate(stmt.max);
            dynamic inc = Evaluate(stmt.inc);

            if (min is not int && max is not int && inc is not int && min is not double && max is not double && inc is not double)
                throw new Laufzeitfehler(stmt.initializierer.name, Fehlermeldungen.forWrongType);

            if (min < max)
            {
                for (dynamic i = min; i <= max; i += inc)
                {
                    Execute(stmt.körper);
                }
            }
            else if (min > max)
            {
                for (dynamic i = min; i >= max; i += inc)
                {
                    Execute(stmt.körper);
                }
            }
            else
            {
                Execute(stmt.körper);
            }

            return null;
        }

        public object VisitAssignExpr(Ausdruck.Zuweisung expr)
        {
            var wert = Evaluate(expr.wert);
            object stelle;
            if (expr.stelle != null)
            {
                stelle = Evaluate(expr.stelle);

                if (locals.TryGetValue(expr, out var _distance))
                {
                    environment.AssignArrayAt(_distance, expr.name, (int)stelle, wert);
                }
                else
                {
                    globals.AssignArray(expr.name, (int)stelle, wert);
                }

                return wert;
            }

            if (locals.TryGetValue(expr, out var distance))
            {
                environment.AssignAt(distance, expr.name, wert);
            }
            else
            {
                globals.Assign(expr.name, wert);
            }

            return wert;
        }

        public object VisitStandartArrayExpr(Ausdruck.StandartArray expr)
        {
            int anzahl = (int)Evaluate(expr.anzahl);

            return expr.typ.typ switch
            {
                ZAHLEN => new int[anzahl],
                KOMMAZAHLEN => new double[anzahl],
                BOOLEANS => new bool[anzahl],
                BUCHSTABEN => new char[anzahl],
                TEXTE => Enumerable.Repeat(string.Empty, anzahl).ToArray(),
                _ => throw new Laufzeitfehler(expr.typ, "igendwas ist bei standart array schief gelaufen"),
            };
        }

        public object VisitBinaryExpr(Ausdruck.Binär expr)
        {
            object _left = Evaluate(expr.links);
            object _right = Evaluate(expr.rechts);

            Type typ;
            object left, right;
            if (_left is not Array)
            {
                typ = CheckOperandTypes(expr.op, _left, _right);

                left = Convert.ChangeType(_left, typ);
                right = Convert.ChangeType(_right, typ);
            }
            else
            {
                typ = typeof(object[]);
                left = _left;
                right = Convert.ChangeType(_right, typeof(int));
            }

            switch (expr.op.typ)
            {
                case UNGLEICH:
                    if (_left.GetType() == _right.GetType()) return !left.Equals(right);
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opSameType);
                case GLEICH:
                    if (_left.GetType() == _right.GetType()) return left.Equals(right);
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opSameType);
                case GRÖßER:
                    if (typ == typeof(double)) return (double)left > (double)right;
                    if (typ == typeof(int)) return (int)left > (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case GRÖßER_GLEICH:
                    if (typ == typeof(double)) return (double)left >= (double)right;
                    if (typ == typeof(int)) return (int)left >= (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case KLEINER:
                    if (typ == typeof(double)) return (double)left < (double)right;
                    if (typ == typeof(int)) return (int)left < (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case KLEINER_GLEICH:
                    if (typ == typeof(double)) return (double)left <= (double)right;
                    if (typ == typeof(int)) return (int)left <= (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case MODULO:
                    if (typ == typeof(int)) return (int)left % (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyInt);
                case MINUS:
                    if (typ == typeof(double)) return (double)left - (double)right;
                    if (typ == typeof(int)) return (int)left - (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case PLUS:
                    if (typ == typeof(double)) return (double)left + (double)right;
                    if (typ == typeof(int)) return (int)left + (int)right;
                    if (left is string || right is string) return Erweiterungen.Stringify(left) + Erweiterungen.Stringify(right);
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNumOrString);
                case DURCH:
                    if (typ == typeof(double)) return (double)left / (double)right;
                    if (typ == typeof(int)) return (int)left / (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case MAL:
                    if (typ == typeof(double)) return (double)left * (double)right;
                    if (typ == typeof(int)) return (int)left * (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case HOCH:
                    if (typ == typeof(double)) return Math.Pow((double)left, (double)right);
                    if (typ == typeof(int)) return Math.Pow((int)left, (int)right);
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case WURZEL:
                    if (typ == typeof(double)) return Math.Pow((double)left, 1 / (double)right);
                    if (typ == typeof(int)) return Math.Pow((int)left, 1.0 / (int)right);
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case UND:
                    if (typ == typeof(int)) return (int)left & (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case ODER:
                    if (typ == typeof(int)) return (int)left | (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case KONTRA:
                    if (typ == typeof(int)) return (int)left ^ (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case LINKS:
                    if (typ == typeof(int)) return (int)left << (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case RECHTS:
                    if (typ == typeof(int)) return (int)left >> (int)right;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.opOnlyNum);
                case STELLE:
                    if ((left as Array).Length > (int)right) return (left as Array).GetValue((int)right);
                    throw new Laufzeitfehler(expr.op, "Index außerhalb des Arrays");
            }

            // Unreachable.
            return null;
        }

        public object VisitCallExpr(Ausdruck.Aufruf expr)
        {
            object callee = Evaluate(expr.aufrufer);

            List<object> arguments = new();

            foreach (Ausdruck argument in expr.argumente)
            {
                arguments.Add(Evaluate(argument));
            }
            if (!(callee is IAufrufbar))
            {
                throw new Laufzeitfehler(expr.klammer, Fehlermeldungen.onlyCallFunc);
            }

            IAufrufbar function = (IAufrufbar)callee;
            if (arguments.Count != function.Arity)
            {
                throw new Laufzeitfehler(expr.klammer, Fehlermeldungen.wrongParamCount(function.Arity, arguments.Count));
            }

            return function.Aufrufen(this, arguments);

        }

        public object VisitGroupingExpr(Ausdruck.Gruppierung expr)
        {
            return Evaluate(expr.ausdruck);
        }

        public object VisitLiteralExpr(Ausdruck.Wert expr)
        {
            return expr.wert;
        }

        public object VisitLogicalExpr(Ausdruck.Logisch expr)
        {
            object left = Evaluate(expr.links);

            if (expr.op.typ == ODER)
            {
                if (left.Equals(true)) return left;
            }
            else
            {
                if (left.Equals(false)) return left;
            }

            return Evaluate(expr.rechts);
        }

        public object VisitUnaryExpr(Ausdruck.Unär expr)
        {
            object rechts = Evaluate(expr.rechts);

            Type typ = CheckOperandTypes(expr.op, rechts);
            rechts = Convert.ChangeType(rechts, typ);

            switch (expr.op.typ)
            {
                case NICHT:
                    if (typ == typeof(bool)) return rechts.Equals(false);
                    if (typ == typeof(int)) return ~(int)rechts;
                    throw new Laufzeitfehler(expr.op, Fehlermeldungen.unaryOpWrongType("nicht", "Boolean oder Zahlen"));
                case BANG_MINUS:
                    return NumberUnary(x => -x);
                case BETRAG:
                    return NumberUnary(x => Math.Abs(x));
                case LOG:
                    return NumberUnary(x => Math.Log(x));
                case SINUS:
                    return NumberUnary(x => Math.Sin(x));
                case KOSINUS:
                    return NumberUnary(x => Math.Cos(x));
                case TANGENS:
                    return NumberUnary(x => Math.Tan(x));
                case ARKUSSINUS:
                    return NumberUnary(x => Math.Asin(x));
                case ARKUSKOSINUS:
                    return NumberUnary(x => Math.Acos(x));
                case ARKUSTANGENS:
                    return NumberUnary(x => Math.Atan(x));
                case HYPERBELSINUS:
                    return NumberUnary(x => Math.Sinh(x));
                case HYPERBELKOSINUS:
                    return NumberUnary(x => Math.Cosh(x));
                case HYPERBELTANGENS:
                    return NumberUnary(x => Math.Tanh(x));
                case STÜCK:
                    if (rechts is not int)
                        throw new Laufzeitfehler(expr.op, Fehlermeldungen.unaryOpWrongType("stück", "Zahlen"));

                    return new int[(int)rechts];
            }

            // Unreachable.
            return null;

            object NumberUnary(Func<dynamic, object> func)
            {
                if (typ == typeof(double)) return func((double)rechts);
                if (typ == typeof(int)) return func((int)rechts);
                throw new Laufzeitfehler(expr.op, Fehlermeldungen.unaryOpWrongType("ln", "Zahlen"));
            }
        }

        public object VisitVariableExpr(Ausdruck.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Symbol name, Ausdruck expr)
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

        private Type CheckOperandTypes(Symbol op, object operand)
        {
            if (operand is double) return typeof(double);
            if (operand is int) return typeof(int);
            if (operand is bool) return typeof(bool);
            if (operand is string) return typeof(string);
            if (operand is char) return typeof(char);

            throw new Laufzeitfehler(op, Fehlermeldungen.opInvalid);
        }

        private Type CheckOperandTypes(Symbol op, object left, object right)
        {

            if ((left is double && right is int) || (left is int && right is double) || (left is double && right is double))
                return typeof(double);
            if (left is int && right is int)
                return typeof(int);
            if (left is bool && right is bool)
                return typeof(bool);
            if (left is string || right is string)
                return typeof(string);
            if (left is char && right is char)
                return typeof(char);

            throw new Laufzeitfehler(op, Fehlermeldungen.opInvalid);
        }
    }
}
