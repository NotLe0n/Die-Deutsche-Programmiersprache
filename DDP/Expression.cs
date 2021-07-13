using System.Collections.Generic;

namespace DDP
{
    public abstract class Expression
    {
        public abstract R Accept<R>(IVisitor<R> visitor);

        public interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitCallExpr(Call expr);
            //R visitGetExpr(Get expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitLogicalExpr(Logical expr);
            //R visitSetExpr(Set expr);
            //R visitSuperExpr(Super expr);
            //R visitThisExpr(This expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
        }

        public class Assign : Expression
        {
            public Assign(Token name, Expression value)
            {
                this.name = name;
                this.value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public readonly Token name;
            public readonly Expression value;
        }

        public class Binary : Expression
        {
            public Binary(Expression left, Token op, Expression right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public readonly Expression left;
            public readonly Token op;
            public readonly Expression right;
        }

        public class Call : Expression
        {
            public Call(Expression callee, Token paren, List<Expression> arguments)
            {
                this.callee = callee;
                this.paren = paren;
                this.arguments = arguments;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public readonly Expression callee;
            public readonly Token paren;
            public readonly List<Expression> arguments;
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

        public class Grouping : Expression
        {
            public Grouping(Expression expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public readonly Expression expression;
        }

        public class Literal : Expression
        {
            public Literal(object value)
            {
                this.value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public readonly object value;
        }

        public class Logical : Expression
        {
            public Logical(Expression left, Token op, Expression right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public readonly Expression left;
            public readonly Token op;
            public readonly Expression right;
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

        public class Unary : Expression
        {
            public Unary(Token op, Expression right)
            {
                this.op = op;
                this.right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public readonly Token op;
            public readonly Expression right;
        }

        public class Variable : Expression
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public readonly Token name;
        }
    }
}
