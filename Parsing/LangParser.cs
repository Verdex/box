
using System.Linq;
using Box.AST;

namespace Box.Parsing
{
    public static class LangParser
    {
        private static Parser<string> Lit( string value )
        {
            return ParserUtil.Match( value );
        }

        public static Parser<Empty> EndLine = 
            ParserUtil.Alternate(
                Lit( "\r\n" ),
                Lit( "\n" ),
                Lit( "\r" ) ).Map( v => new Empty() );

        public static Parser<Boolean> Boolean = 
            ParserUtil.Alternate( 
                Lit( "true" ), 
                Lit( "false" ) )
            .Map( value => new Boolean( value == "true" ) );

        private static Parser<int> Digits =
            ParserUtil.Alternate(
                Lit( "0" ),
                Lit( "1" ),
                Lit( "2" ),
                Lit( "3" ),
                Lit( "4" ),
                Lit( "5" ),
                Lit( "6" ),
                Lit( "7" ),
                Lit( "8" ),
                Lit( "9" ) )
            .OneOrMore()
            .Map( value => value.Aggregate( "", (a, b) => a + b  ) )
            .Map( value => int.Parse( value ) );

        private static Parser<bool> Negative =
            Lit( "-" )
            .OneOrNone()
            .Map( value => value.HasValue );

        private static Parser<Number> WholeNumber =
            ParserUtil.Bind( Negative, neg => 
            ParserUtil.Bind( Digits, digits =>
            ParserUtil.Unit( new Number( digits, neg, 0, false, 0 ) ) ) );

        private static Parser<Number> DecimalNumber = 
            ParserUtil.Bind( Negative, neg => 
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( Lit( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Unit( new Number( whole, neg, 0, false, deci ) ) ) ) ) );

        private static Parser<Number> ExponentNumber =
            ParserUtil.Bind( Negative, negWhole => 
            ParserUtil.Bind( Digits, whole =>
            ParserUtil.Bind( Lit( "." ), () =>
            ParserUtil.Bind( Digits, deci => 
            ParserUtil.Bind( 
                ParserUtil.Alternate( 
                    Lit( "E" ),
                    Lit( "e" ) ), () =>
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
            ParserUtil.Bind( Lit( "\"" ), () =>
            ParserUtil.Bind( ParserUtil.ParseUntil( Lit( "\"" ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) );

        private static Parser<NString> RawString = 
            ParserUtil.Bind( Lit( "[" ), () => 
            ParserUtil.Bind( Lit( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            ParserUtil.Bind( Lit( "[" ), () => 
            ParserUtil.Bind( ParserUtil.ParseUntil( Lit( "]" + equals + "]" ) ), str => 
            ParserUtil.Unit( new NString( str ) ) ) ) ) );

        public static Parser<NString> NString = 
            ParserUtil.Alternate(
                NormalString,
                RawString );

        private static Parser<Empty> LineComment =
            ParserUtil.Bind( Lit( "--" ), () =>
            ParserUtil.Bind( ParserUtil.ParseUntil( ParserUtil.Alternate( EndLine, ParserUtil.End ) ), () =>
            ParserUtil.Unit( new Empty() ) ) );

        private static Parser<Empty> BlockComment = 
            ParserUtil.Bind( Lit( "--[" ), () => 
            ParserUtil.Bind( Lit( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            ParserUtil.Bind( Lit( "[" ), () => 
            ParserUtil.Bind( ParserUtil.ParseUntil( Lit( "--]" + equals + "]" ) ), () => 
            ParserUtil.Unit( new Empty() ) ) ) ) );

        public static Parser<Empty> Comment = 
            ParserUtil.Alternate(
                // TODO test the toggle block sometime
                // Line Comment needs to happen first in order to pull off the toggle block trick
                LineComment,
                BlockComment );

            // TODO test
        public static Parser<Empty> Semi =
            ParserUtil.Bind( ParserUtil.Whitespace, () =>
            ParserUtil.Bind( Lit( ";" ), () => 
            ParserUtil.Bind( ParserUtil.Whitespace, () =>
            ParserUtil.Unit( new Empty() ) ) ) );

            // TODO test
        public static Parser<Return> Return =
            ParserUtil.Bind( Lit( "return" ), () =>
            ParserUtil.Bind( ParserUtil.Whitespace, () => 
            ParserUtil.Bind( Expr, expr =>
            ParserUtil.Bind( Semi, () =>
            ParserUtil.Unit( new Return( expr ) ) ) ) ) ); 

        public static Parser<Expr> ParenExpr = 
            ParserUtil.Bind( ParserUtil.Whitespace, () =>
            ParserUtil.Bind( Lit( "(" ), () =>
            ParserUtil.Bind( ParserUtil.Whitespace, () => 
            ParserUtil.Bind( Expr, e => 
            ParserUtil.Bind( ParserUtil.Whitespace, () => 
            ParserUtil.Bind( Lit( ")" ), () =>
            ParserUtil.Bind( ParserUtil.Whitespace, () => 
            ParserUtil.Unit( e ) ) ) ) ) ) ) );

        private static Parser<Expr> Cast<T>( Parser<T> p )
            where T : Expr 
        {
            return p.Map( v => v as Expr );
        }

             // TODO test when finished
        public static Parser<Expr> Expr =
            ParserUtil.Alternate( 
                Cast( Boolean ),
                Cast( Number ),
                Cast( NString ),
                ParenExpr );
    }
}
