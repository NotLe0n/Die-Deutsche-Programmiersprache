using System.Collections.Generic;

namespace DDP
{
    public abstract class Statement
    {
        public abstract R Accept<R>(IVisitor<R> visitor);

        public interface IVisitor<R>
        {
            public R VisitBlockStmt(Block stmt);
            //public R visitClassStmt(Class stmt);
            public R VisitExpressionStmt(Expression stmt);
            public R VisitFunctionStmt(Function stmt);
            public R VisitIfStmt(If stmt);
            public R VisitReturnStmt(Return stmt);
            public R VisitVarStmt(Var stmt);
            public R VisitWhileStmt(While stmt);
            public R VisitForStmt(For stmt);
            public R VisitDoWhileStmt(DoWhile stmt);
        }


        public class Block : Statement
        {
            public Block(List<Statement> statements)
            {
                this.statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public readonly List<Statement> statements;
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

        public class Expression : Statement
        {
            public Expression(global::DDP.Expression expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public readonly global::DDP.Expression expression;
        }

        public class Function : Statement
        {
            public Function(Token name, List<Token> param, Token type, List<Statement> body)
            {
                this.name = name;
                this.param = param;
                this.type = type;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitFunctionStmt(this);
            }

            public readonly Token name;
            public readonly List<Token> param;
            public readonly Token type;
            public readonly List<Statement> body;
        }

        public class If : Statement
        {
            public If(global::DDP.Expression condition, Statement thenBranch, Statement elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public readonly global::DDP.Expression condition;
            public readonly Statement thenBranch;
            public readonly Statement elseBranch;
            public Token token;
        }

        public class Return : Statement
        {
            public Return(Token keyword, global::DDP.Expression value)
            {
                this.keyword = keyword;
                this.value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitReturnStmt(this);
            }

            public readonly Token keyword;
            public readonly global::DDP.Expression value;
        }

        public class Var : Statement
        {
            public Var(Token type, Token name, global::DDP.Expression initializer)
            {
                this.type = type;
                this.name = name;
                this.initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public readonly Token name;
            public readonly global::DDP.Expression initializer;
            public readonly Token type;
        }

        public class While : Statement
        {
            public While(global::DDP.Expression condition, Statement body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public readonly global::DDP.Expression condition;
            public readonly Statement body;
            public Token token;
        }

        public class For : Statement
        {
            public For(Var initializer, global::DDP.Expression min, global::DDP.Expression max, global::DDP.Expression inc, Statement body)
            {
                this.initializer = initializer;
                this.min = min;
                this.max = max;
                this.body = body;
                this.inc = inc;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }

            public readonly Var initializer;
            public readonly global::DDP.Expression min;
            public readonly global::DDP.Expression max;
            public readonly Statement body;
            public readonly global::DDP.Expression inc;
        }

        public class DoWhile : Statement
        {
            public DoWhile(global::DDP.Expression condition, Statement body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitDoWhileStmt(this);
            }

            public readonly global::DDP.Expression condition;
            public readonly Statement body;
            public Token token;
        }
    }
}
