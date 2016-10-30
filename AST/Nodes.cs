
using System.Collections.Generic;
using Box;

namespace Box.AST
{
    public class NamespaceDesignator
    {
        public readonly IEnumerable<string> Designator;

        public NamespaceDesignator( IEnumerable<string> d )
        {
            Designator = d;
        }
    }

    public class NBoolean : Expr
    {
        public readonly bool Value;

        public NBoolean( bool value )
        {
            Value = value;
        }
    }

    public class Number : Expr
    {
        public readonly bool NegativeWhole;
        public readonly int WholeNumber;
        public readonly bool NegativeExponent;
        public readonly int Exponent;
        public readonly int Decimal;

        public Number(
            int whole, 
            bool negWhole, 
            int exponent, 
            bool negExp, 
            int deci)
        {
            WholeNumber = whole;
            NegativeWhole = negWhole;
            Exponent = exponent;
            NegativeExponent = negExp;
            Decimal = deci;
        }
    }

    public class NString : Expr
    {
        public readonly string Value;

        public NString(string value)
        {
            Value = value;
        }
    }

    public class Return : Statement
    {
        public readonly Expr Expression;
        
        public Return( Expr e )
        {
            Expression = e;
        }
    }

    public class YieldReturn : Statement
    {
        public readonly Expr Expression;
        public YieldReturn( Expr e )
        {
            Expression = e;
        }
    }

    public class YieldBreak : Statement { }

    public class Break : Statement { }

    public class Continue : Statement { }

    public class While : Statement
    {
        public readonly Expr Test;
        public readonly IEnumerable<Statement> Statements;
        public While( Expr test, IEnumerable<Statement> stm )
        {
            Test = test;
            Statements = stm;
        }
    }

    public class UsingStatement : Statement
    {
        public readonly NamespaceDesignator Designator;
        public UsingStatement( NamespaceDesignator d )
        {
            Designator = d;
        }
    }

    // :(
    public interface Expr { }
    public interface Statement { }
}
