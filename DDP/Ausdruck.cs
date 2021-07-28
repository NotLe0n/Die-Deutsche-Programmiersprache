using System.Collections.Generic;

namespace DDP
{
    public abstract class Ausdruck
    {
        public abstract R Accept<R>(IVisitor<R> visitor);

        public interface IVisitor<R>
        {
            R VisitAssignExpr(Zuweisung expr);
            R VisitBinaryExpr(Binär expr);
            R VisitCallExpr(Aufruf expr);
            //R visitGetExpr(Get expr);
            R VisitGroupingExpr(Gruppierung expr);
            R VisitLiteralExpr(Wert expr);
            R VisitLogicalExpr(Logisch expr);
            //R visitSetExpr(Set expr);
            //R visitSuperExpr(Super expr);
            //R visitThisExpr(This expr);
            R VisitUnaryExpr(Unär expr);
            R VisitVariableExpr(Variable expr);
        }

        public class Zuweisung : Ausdruck
        {
            public Zuweisung(Symbol name, Ausdruck wert)
            {
                this.name = name;
                this.wert = wert;
            }

            public Zuweisung(Symbol name, Ausdruck stelle, Ausdruck wert)
            {
                this.name = name;
                this.stelle = stelle;
                this.wert = wert;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public readonly Symbol name;
            public readonly Ausdruck stelle;
            public readonly Ausdruck wert;
        }

        public class Binär : Ausdruck
        {
            public Binär(Ausdruck links, Symbol op, Ausdruck rechts)
            {
                this.links = links;
                this.op = op;
                this.rechts = rechts;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public readonly Ausdruck links;
            public readonly Symbol op;
            public readonly Ausdruck rechts;
        }

        public class Aufruf : Ausdruck
        {
            public Aufruf(Ausdruck aufrufer, Symbol klammer, List<Ausdruck> argumente)
            {
                this.aufrufer = aufrufer;
                this.klammer = klammer;
                this.argumente = argumente;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public readonly Ausdruck aufrufer;
            public readonly Symbol klammer;
            public readonly List<Ausdruck> argumente;
        }

        /*public class Get : Expr
        {
            Get(Expr obj, Token name)
            {
                this.obj = obj;
                this.name = name;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitGetExpr(this);
            }

            public readonly Expr obj;
            public readonly Token name;
        }*/

        public class Gruppierung : Ausdruck
        {
            public Gruppierung(Ausdruck ausdruck)
            {
                this.ausdruck = ausdruck;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public readonly Ausdruck ausdruck;
        }

        public class Wert : Ausdruck
        {
            public Wert(object wert)
            {
                this.wert = wert;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public readonly object wert;
        }

        public class Logisch : Ausdruck
        {
            public Logisch(Ausdruck links, Symbol op, Ausdruck rechts)
            {
                this.links = links;
                this.op = op;
                this.rechts = rechts;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public readonly Ausdruck links;
            public readonly Symbol op;
            public readonly Ausdruck rechts;
        }

        /*public class Set : Expr
        {
            Set(Expr obj, Token name, Expr value)
            {
                this.obj = obj;
                this.name = name;
                this.value = value;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitSetExpr(this);
            }

            public readonly Expr obj;
            public readonly Token name;
            public readonly Expr value;
        }

        public class Super : Expr
        {
            Super(Token keyword, Token method)
            {
                this.keyword = keyword;
                this.method = method;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitSuperExpr(this);
            }

            public readonly Token keyword;
            public readonly Token method;
        }

        public class This : Expr
        {
            This(Token keyword)
            {
                this.keyword = keyword;
            }

            public override R accept<R>(Visitor<R> visitor)
            {
                return visitor.visitThisExpr(this);
            }

            public readonly Token keyword;
        }*/

        public class Unär : Ausdruck
        {
            public Unär(Symbol op, Ausdruck rechts)
            {
                this.op = op;
                this.rechts = rechts;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public readonly Symbol op;
            public readonly Ausdruck rechts;
        }

        public class Variable : Ausdruck
        {
            public Variable(Symbol name)
            {
                this.name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public readonly Symbol name;
        }
    }
}
