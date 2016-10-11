
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

        private static Parser<bool> Negative =
            ParserUtil.Match( "-" )
            .OneOrNone()
            .Map( value => value.HasValue );

        private static Parser<Number> WholeNumber =
            ParserUtil.Bind( Negative, neg => 
            ParserUtil.Bind( Digits, digits =>
            ParserUtil.Unit( new Number( digits, neg, 0, false, 0 ) ) ) );

        private static Parser<Number> DecimalNumber = 
            ParserUtil.Bind( Negative, neg => 
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( ParserUtil.Match( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Unit( new Number( whole, neg, 0, false, deci ) ) ) ) ) );

        private static Parser<Number> ExponentNumber =
            ParserUtil.Bind( Negative, negWhole => 
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( ParserUtil.Match( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Bind( 
                ParserUtil.Alternate( 
                    ParserUtil.Match( "E" ),
                    ParserUtil.Match( "e" ) ), () =>
            ParserUtil.Bind( Negative, negExp =>
            ParserUtil.Bind( Digits, exponent => 
            ParserUtil.Unit( new Number( whole, negWhole, exponent, negExp, deci ) ) ) ) ) ) ) ) );
            
        public static Parser<Number> Number =
            ParserUtil.Alternate(
                // The order here matters.  If we start with WholeNumber, then
                // we will miss parse an exponent and/or decimal.
                ExponentNumber,
                DecimalNumber,
                WholeNumber );

        private static Parser<NString> SingleLineString = 
            ParserUtil.Bind( ParserUtil.Match( "\"" ), () =>
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Match( "\"" ).Map( v => new Empty() ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) );

        public static Parser<NString> MultiLineString = 
            ParserUtil.Bind( ParserUtil.Match( "[" ), () => 
            ParserUtil.Bind( ParserUtil.Match( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals =>
            ParserUtil.Bind( ParserUtil.Match( "[" ), () => 
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Match( "]" + equals + "]" ).Map( v => new Empty() ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) ) ) );

        /*private static Parser<NString> NString = 
            ParserUtil.Alternate(
                SingleLineString,
                MultiLineString );*/
    }
}
