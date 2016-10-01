
using Box;

namespace Box.AST
{
    public class Boolean
    {
        public readonly bool Value;

        public Boolean( bool value )
        {
            Value = value;
        }
    }

    public class Number
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
            Exponent = exponent;
            Decimal = deci;
        }
    }
}
