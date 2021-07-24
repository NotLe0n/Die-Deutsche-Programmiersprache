using System.Collections.Generic;

namespace DDP
{
    public abstract class Anweisung
    {
        public abstract R Accept<R>(IVisitor<R> visitor);

        public interface IVisitor<R>
        {
            public R VisitBlockStmt(Block stmt);
            //public R visitClassStmt(Class stmt);
            public R VisitExpressionStmt(Ausdruck stmt);
            public R VisitFunctionStmt(Funktion stmt);
            public R VisitIfStmt(Wenn stmt);
            public R VisitReturnStmt(Rückgabe stmt);
            public R VisitVarStmt(Var stmt);
            public R VisitWhileStmt(Solange stmt);
            public R VisitForStmt(Für stmt);
            public R VisitDoWhileStmt(MacheSolange stmt);
        }


        public class Block : Anweisung
        {
            public Block(List<Anweisung> anweisungen)
            {
                this.anweisungen = anweisungen;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public readonly List<Anweisung> anweisungen;
        }

        /*public class Class : Stmt
        {
            private Class(Token name,
                  Expr.Variable superclass,
                  List<Function> methods)
            {
                this.name = name;
                this.superclass = superclass;
                this.methods = methods;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitClassStmt(this);
            }

            private readonly Token name;
            private readonly Expr.Variable superclass;
            private readonly List<Function> methods;
        }*/

        public class Ausdruck : Anweisung
        {
            public Ausdruck(global::DDP.Ausdruck ausdruck)
            {
                this.ausdruck = ausdruck;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public readonly global::DDP.Ausdruck ausdruck;
        }

        public class Funktion : Anweisung
        {
            public Funktion(Symbol name, List<Symbol> argumente, Symbol typ, List<Anweisung> körper)
            {
                this.name = name;
                this.argumente = argumente;
                this.typ = typ;
                this.körper = körper;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public readonly Symbol name;
            public readonly List<Symbol> argumente;
            public readonly Symbol typ;
            public readonly List<Anweisung> körper;
        }

        public class Wenn : Anweisung
        {
            public Wenn(global::DDP.Ausdruck bedingung, Anweisung dannZweig, Anweisung sonstZweig)
            {
                this.bedingung = bedingung;
                this.dannZweig = dannZweig;
                this.sonstZweig = sonstZweig;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public readonly global::DDP.Ausdruck bedingung;
            public readonly Anweisung dannZweig;
            public readonly Anweisung sonstZweig;
            public Symbol token;
        }

        public class Rückgabe : Anweisung
        {
            public Rückgabe(Symbol schlüsselwort, global::DDP.Ausdruck wert)
            {
                this.schlüsselwort = schlüsselwort;
                this.wert = wert;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public readonly Symbol schlüsselwort;
            public readonly global::DDP.Ausdruck wert;
        }

        public class Var : Anweisung
        {
            public Var(SymbolTyp artikel, Symbol typ, Symbol name, global::DDP.Ausdruck initializierer)
            {
                this.typ = typ;
                this.name = name;
                this.initializierer = initializierer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public readonly Symbol name;
            public readonly global::DDP.Ausdruck initializierer;
            public readonly Symbol typ;
            public readonly SymbolTyp artikel;
        }

        public class Solange : Anweisung
        {
            public Solange(global::DDP.Ausdruck bedingung, Anweisung körper)
            {
                this.bedingung = bedingung;
                this.körper = körper;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public readonly global::DDP.Ausdruck bedingung;
            public readonly Anweisung körper;
            public Symbol symbol;
        }

        public class Für : Anweisung
        {
            public Für(Var initializierer, global::DDP.Ausdruck min, global::DDP.Ausdruck max, global::DDP.Ausdruck inc, Anweisung körper)
            {
                this.initializierer = initializierer;
                this.min = min;
                this.max = max;
                this.körper = körper;
                this.inc = inc;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }

            public readonly Var initializierer;
            public readonly global::DDP.Ausdruck min;
            public readonly global::DDP.Ausdruck max;
            public readonly Anweisung körper;
            public readonly global::DDP.Ausdruck inc;
        }

        public class MacheSolange : Anweisung
        {
            public MacheSolange(global::DDP.Ausdruck bedingung, Anweisung körper)
            {
                this.bedingung = bedingung;
                this.körper = körper;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitDoWhileStmt(this);
            }

            public readonly global::DDP.Ausdruck bedingung;
            public readonly Anweisung körper;
            public Symbol symbol;
        }
    }
}
