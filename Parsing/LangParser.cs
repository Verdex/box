
using System;
using System.Collections.Generic;
using System.Linq;
using Box.AST;

namespace Box.Parsing
{
    public static class LangParser
    {
        // NOTE:  Referencing a field in the definition of a field will yield null.
        // For recursive definitions use properties.  Also make sure fields that
        // are defined in terms of other fields occur last.

        // TODO going to need to find a way to add in comments basically anywhere

        private static Parser<T> Alt<T>( params Parser<T>[] parsers )
        {
            return ParserUtil.Alternate( parsers );
        }

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
            Alt(
                Lit( "\r\n" ),
                Lit( "\n" ),
                Lit( "\r" ) ).Map( v => new Empty() );

                // TODO probably need end of file in here too
        public static Parser<Empty> Ws =
            Alt(
                EndLine,
                CastEmpty( Lit( " " ) ),
                CastEmpty( Lit( "\t" ) ),
                CastEmpty( Lit( "\f" ) ),
                CastEmpty( Lit( "\v" ) ) ).ZeroOrMore().Map( v => new Empty() );

        private static Parser<Empty> LineComment =
            Bind( Lit( "--" ), () =>
            Bind( ParserUtil.ParseUntil( Alt( EndLine, ParserUtil.End ) ), () =>
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
            Alt(
                // TODO test the toggle block sometime
                // Line Comment needs to happen first in order to pull off the toggle block trick
                LineComment,
                BlockComment );

        public static Parser<Empty> Junk = 
            Bind( Ws, () =>
            Bind( Comment, () =>
            Bind( Ws, () =>
            Unit( new Empty() ) ) ) );

        public static Parser<NBoolean> Boolean =
            Alt( 
                Lit( "true" ), 
                Lit( "false" ) )
            .Map( value => new NBoolean( value == "true" ) );

        private static Parser<int> Digits = 
            Alt( // TODO can use EatCharIf
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

        private static Parser<string> NonNumSymbolChar =
            Alt(
                ParserUtil.EatCharIf( Char.IsLetter ).Map( c => new String( c, 1 ) ),
                Lit( "_" ),
                Lit( "~" ),
                Lit( "@" ),
                Lit( "$" ) );

        public static Parser<string> Symbol =
            Bind( NonNumSymbolChar,            first => 
            Bind( Alt( 
                      NonNumSymbolChar,
                      Digits.Map( d => d.ToString() ) )
                            .ZeroOrMore()
                            .Map( value => value.Aggregate( "", (a, b) => a + b  ) ),   rest => 
            Unit( first + rest ) ) );

        private static Parser<IEnumerable<String>> NamespaceList =
            Bind( Lit( "." ), () =>
            Bind( Symbol, s => 
            Unit( s ) ) ).ZeroOrMore();

        public static Parser<NamespaceDesignator> NamespaceDesignator =
            Bind( Symbol, s =>
            Bind( NamespaceList, l => 
            Unit( new NamespaceDesignator( new [] { s }.Concat( l ) ) ) ) );

        public static Parser<UsingStatement> UsingStatement =
            Bind( Ws, () => 
            Bind( Lit( "using" ), () =>
            Bind( Ws, () =>
            Bind( NamespaceDesignator, nsd =>
            Bind( Semi, () =>
            Unit( new UsingStatement( nsd ) ) ) ) ) ) );

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
                Alt( 
                    Lit( "E" ),
                    Lit( "e" ) ), () =>
            Bind( Negative, negExp =>
            Bind( Digits, exponent => 
            Unit( new Number( whole, negWhole, exponent, negExp, deci ) ) ) ) ) ) ) ) );
            
        public static Parser<Number> Number =
            Alt(
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
            Alt(
                NormalString,
                RawString );

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

        public static Parser<YieldReturn> YieldReturn = 
            Bind( Ws, () => 
            Bind( Lit( "yield" ), () =>
            Bind( Ws, () => 
            Bind( Lit( "return" ), () =>
            Bind( Ws, () => 
            Bind( Expr, expr =>
            Bind( Semi, () =>
            Unit( new YieldReturn( expr ) ) ) ) ) ) ) ) ); 

        public static Parser<YieldBreak> YieldBreak = 
            Bind( Ws, () => 
            Bind( Lit( "yield" ), () =>
            Bind( Ws, () => 
            Bind( Lit( "break" ), () =>
            Bind( Semi, () =>
            Unit( new YieldBreak() ) ) ) ) ) ); 

        public static Parser<Break> Break =
            Bind( Ws, () => 
            Bind( Lit( "break" ), () =>
            Bind( Semi, () =>
            Unit( new Break() ) ) ) ); 

        public static Parser<Continue> Continue =
            Bind( Ws, () => 
            Bind( Lit( "continue" ), () =>
            Bind( Semi, () =>
            Unit( new Continue() ) ) ) ); 

            // TODO test
        public static Parser<While> While =
            Bind( Ws, () =>
            Bind( Lit( "while" ), () => 
            Bind( Ws, () => 
            Bind( Expr, test => 
            Bind( Ws, () =>
            Bind( Lit( "{" ), () =>
            Bind( Ws, () => 
            Bind( Statement.ZeroOrMore(), statements => // TODO whitespace between statements?
            Bind( Ws, () => 
            Bind( Lit( "}" ), () =>
            Unit( new While( test, statements ) ) ) ) ) ) ) ) ) ) ) );

        public static Parser<Expr> ParenExpr = 
            Bind( Ws, () =>
            Bind( Lit( "(" ), () =>
            Bind( Ws, () => 
            Bind( Expr, e => 
            Bind( Ws, () => 
            Bind( Lit( ")" ), () =>
            Bind( Ws, () => 
            Unit( e ) ) ) ) ) ) ) );

        private static Parser<Statement> CastStatement<T>( Parser<T> p )
            where T : Statement
        {
            return p.Map( v => v as Statement );
        }

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
        private static Parser<Statement> _stm =
            Alt(
                CastStatement( Return ),
                CastStatement( YieldReturn ),
                CastStatement( YieldBreak ),
                CastStatement( UsingStatement ) );
        public static Parser<Statement> Statement 
        {
            get { return _stm; }
        }
            

             // TODO test when finished
        private static Parser<Expr> _expr = 
            Alt( 
                CastExpr( Boolean ),
                CastExpr( Number ),
                CastExpr( NString ),
                ParenExpr );
        public static Parser<Expr> Expr
        {
            get { return _expr; }
        }
    }
}
