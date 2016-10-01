
using System.Linq;
using Box.AST;

namespace Box.Parsing
{
    public static class LangParser
    {
        public static Parser<Boolean> Boolean = 
            ParserUtil.Alternate( 
                ParserUtil.Match( "true" ), 
                ParserUtil.Match( "false" ) )
            .Map( value => new Boolean( value == "true" ) );

        private static Parser<int> Digits =
            ParserUtil.Alternate(
                ParserUtil.Match( "0" ),
                ParserUtil.Match( "1" ),
                ParserUtil.Match( "2" ),
                ParserUtil.Match( "3" ),
                ParserUtil.Match( "4" ),
                ParserUtil.Match( "5" ),
                ParserUtil.Match( "6" ),
                ParserUtil.Match( "7" ),
                ParserUtil.Match( "8" ),
                ParserUtil.Match( "9" ) )
            .OneOrMore()
            .Map( value => value.Aggregate( "", (a, b) => a + b  ) )
            .Map( value => int.Parse( value ) );

        private static Parser<Number> WholeNumber =
            Digits
            .Map( value => new Number( value, 0, 0 ) );

        private static Parser<Number> DecimalNumber = 
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( ParserUtil.Match( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Unit( new Number( whole, 0, deci ) ) ) ) );

        private static Parser<Number> ExponentNumber =
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( ParserUtil.Match( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Bind( 
                ParserUtil.Alternate( 
                    ParserUtil.Match( "E" ),
                    ParserUtil.Match( "e" ) ), () =>
            ParserUtil.Bind( Digits, exponent => 
            ParserUtil.Unit( new Number( whole, exponent, deci ) ) ) ) ) ) );

        // TODO make this work with negative numbers (also negative exponents)
        // TODO make this work with fractional exponents
        public static Parser<Number> Number =
            ParserUtil.Alternate(
                // The order here matters.  If we start with WholeNumber, then
                // we will miss parse an exponent and/or decimal.
                ExponentNumber,
                DecimalNumber,
                WholeNumber );
    }
}
