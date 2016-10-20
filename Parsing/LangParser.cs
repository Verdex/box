
using System;
using System.Linq;
using Box.AST;

namespace Box.Parsing
{
    public static class LangParser
    {
        // NOTE:  Referencing a field in the definition of a field will yield null.
        // For recursive definitions use properties.  Also make sure fields that
        // are defined in terms of other fields occur last.

        private static Parser<string> Lit( string value )
        {
            return ParserUtil.Match( value );
        }

        private static Parser<B> Bind<A, B>( Parser<A> parser, Func<A, Parser<B>> gen )
        {
            return ParserUtil.Bind( parser, gen );
        }

        private static Parser<B> Bind<A, B>( Parser<A> parser, Func<Parser<B>> gen )
        {
            return ParserUtil.Bind( parser, gen );
        }

        private static Parser<A> Unit<A>( A value )
        {
            return ParserUtil.Unit( value );
        } 

        public static Parser<Empty> EndLine =
            ParserUtil.Alternate(
                Lit( "\r\n" ),
                Lit( "\n" ),
                Lit( "\r" ) ).Map( v => new Empty() );

                // TODO probably need end of file in here too
        public static Parser<Empty> Ws =
            ParserUtil.Alternate(
                EndLine,
                CastEmpty( Lit( " " ) ),
                CastEmpty( Lit( "\t" ) ),
                CastEmpty( Lit( "\f" ) ),
                CastEmpty( Lit( "\v" ) ) ).ZeroOrMore().Map( v => new Empty() );

        public static Parser<NBoolean> Boolean =
            ParserUtil.Alternate( 
                Lit( "true" ), 
                Lit( "false" ) )
            .Map( value => new NBoolean( value == "true" ) );

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
            Bind( Negative, neg => 
            Bind( Digits, digits =>
            Unit( new Number( digits, neg, 0, false, 0 ) ) ) );

        private static Parser<Number> DecimalNumber = 
            Bind( Negative, neg => 
            Bind( Digits, whole =>
            Bind( Lit( "." ), () =>
            Bind( Digits, deci => 
            Unit( new Number( whole, neg, 0, false, deci ) ) ) ) ) );

        private static Parser<Number> ExponentNumber =
            Bind( Negative, negWhole => 
            Bind( Digits, whole =>
            Bind( Lit( "." ), () =>
            Bind( Digits, deci => 
            Bind( 
                ParserUtil.Alternate( 
                    Lit( "E" ),
                    Lit( "e" ) ), () =>
            Bind( Negative, negExp =>
            Bind( Digits, exponent => 
            Unit( new Number( whole, negWhole, exponent, negExp, deci ) ) ) ) ) ) ) ) );
            
        public static Parser<Number> Number =
            ParserUtil.Alternate(
                // The order here matters.  If we start with WholeNumber, then
                // we will miss parse an exponent and/or decimal.
                ExponentNumber,
                DecimalNumber,
                WholeNumber );

        private static Parser<NString> NormalString = 
            Bind( Lit( "\"" ), () =>
            Bind( ParserUtil.ParseUntil( Lit( "\"" ) ), str => 
            Unit( new NString( str ) ) ) );

        private static Parser<NString> RawString = 
            Bind( Lit( "[" ), () => 
            Bind( Lit( "=" )
                        .ZeroOrMore()
                        .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            Bind( Lit( "[" ), () => 
            Bind( ParserUtil.ParseUntil( Lit( "]" + equals + "]" ) ), str => 
            Unit( new NString( str ) ) ) ) ) );

        public static Parser<NString> NString = 
            ParserUtil.Alternate(
                NormalString,
                RawString );

        private static Parser<Empty> LineComment =
            Bind( Lit( "--" ), () =>
            Bind( ParserUtil.ParseUntil( ParserUtil.Alternate( EndLine, ParserUtil.End ) ), () =>
            Unit( new Empty() ) ) );

        private static Parser<Empty> BlockComment = 
            Bind( Lit( "--[" ), () => 
            Bind( Lit( "=" )
                                .ZeroOrMore()
                                .Map( value => value.Aggregate( "", (a, b) => a + b  ) ), equals => 
            Bind( Lit( "[" ), () => 
            Bind( ParserUtil.ParseUntil( Lit( "--]" + equals + "]" ) ), () => 
            Unit( new Empty() ) ) ) ) );

        public static Parser<Empty> Comment = 
            ParserUtil.Alternate(
                // TODO test the toggle block sometime
                // Line Comment needs to happen first in order to pull off the toggle block trick
                LineComment,
                BlockComment );

        public static Parser<Empty> Semi =
            Bind( Ws, () =>
            Bind( Lit( ";" ), () => 
            Bind( Ws, () =>
            Unit( new Empty() ) ) ) );

        public static Parser<Return> Return =
            Bind( Ws, () => 
            Bind( Lit( "return" ), () =>
            Bind( Ws, () => 
            Bind( Expr, expr =>
            Bind( Semi, () =>
            Unit( new Return( expr ) ) ) ) ) ) ); 

        public static Parser<Expr> ParenExpr = 
            Bind( Ws, () =>
            Bind( Lit( "(" ), () =>
            Bind( Ws, () => 
            Bind( Expr, e => 
            Bind( Ws, () => 
            Bind( Lit( ")" ), () =>
            Bind( Ws, () => 
            Unit( e ) ) ) ) ) ) ) );

        private static Parser<Expr> CastExpr<T>( Parser<T> p )
            where T : Expr 
        {
            return p.Map( v => v as Expr );
        }

        private static Parser<Empty> CastEmpty<T>( Parser<T> p )
        {
            return p.Map( v => new Empty() );
        }
             // TODO test when finished
        public static Parser<Expr> Expr
        {
            get
            {
                return ParserUtil.Alternate( 
                            CastExpr( Boolean ),
                            CastExpr( Number ),
                            CastExpr( NString ),
                            ParenExpr );
            }
        }
    }
}
