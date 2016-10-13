
using System.Linq;
using Box.AST;

namespace Box.Parsing
{
    public static class LangParser
    {
        public static Parser<Empty> EndLine = 
            ParserUtil.Alternate(
                ParserUtil.Match( "\r\n" ),
                ParserUtil.Match( "\n" ),
                ParserUtil.Match( "\r" ) ).Map( v => new Empty() );

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

        private static Parser<NString> NormalString = 
            ParserUtil.Bind( ParserUtil.Match( "\"" ), () =>
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Match( "\"" ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) );

        private static Parser<NString> RawString = 
            ParserUtil.Bind( ParserUtil.Match( "[" ), () => 
            ParserUtil.Bind( ParserUtil.Match( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            ParserUtil.Bind( ParserUtil.Match( "[" ), () => 
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Match( "]" + equals + "]" ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) ) ) );

        public static Parser<NString> NString = 
            ParserUtil.Alternate(
                NormalString,
                RawString );

        private static Parser<Empty> LineComment =
            ParserUtil.Bind( ParserUtil.Match( "--" ), () =>
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Alternate( EndLine, ParserUtil.End ) ), () =>
            ParserUtil.Unit( new Empty() ) ) );

        private static Parser<Empty> BlockComment = 
            ParserUtil.Bind( ParserUtil.Match( "--[" ), () => 
            ParserUtil.Bind( ParserUtil.Match( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            ParserUtil.Bind( ParserUtil.Match( "[" ), () => 
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Match( "--]" + equals + "]" ) ), () => 
            ParserUtil.Unit( new Empty() ) ) ) ) );

        public static Parser<Empty> Comment = 
            ParserUtil.Alternate(
                // TODO test the toggle block sometime
                // Line Comment needs to happen first in order to pull off the toggle block trick
                LineComment,
                BlockComment );
    }
}
