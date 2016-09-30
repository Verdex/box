
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
    }
}
