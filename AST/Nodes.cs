
using Box;

namespace Box.AST
{
    public class Boolean : Expr
    {
        public readonly bool Value;

        public Boolean( bool value )
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

    // :(
    public interface Expr { }
}
