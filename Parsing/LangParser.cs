
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

        public static Parser<int> Digit =
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
            .Map( value => int.Parse( value ) );

        //public static Parser<?> Decimal = 

        /*public static Parser<Number> Number =
            ParserUtil.Bind( Digit.OneOrMore(), wholeDigits => 
            */
    }
}
