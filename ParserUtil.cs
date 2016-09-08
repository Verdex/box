
namespace Parsing 
{
    public class ParseBuffer
    {

    }

    public class FailData
    {
        
    }

    public class ParseResult<T>
    {
        public readonly bool IsSuccessful;
        public readonly T Result;
        public readonly FailData Failure;

        public ParseResult( T result )
        {
            IsSuccessful = true;
            Result = result;
        }
        public ParseResult( FailData f )
        {
            IsSuccessful = false;
            Failure = f;
        }
    }

    public delegate ParseResult<T> Parser<T>( ParseBuffer buffer );

    // TODO need parsing result
    // TODO need parse source
    public static class ParserUtil
    {
        public static Parser<B> Bind<A, B>( Parser<A> parser, Func<A, Parser<B>> gen )
        {
            
        }
    }
}
