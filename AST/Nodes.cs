
using Box;

namespace Box.AST
{
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

    // :(
    public interface Expr { }
    public interface Statement { }
}
