
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
        public readonly ParseBuffer Buffer;
        public readonly FailData Failure;

        public ParseResult( T result, ParseBuffer buffer )
        {
            IsSuccessful = true;
            Result = result;
            Buffer = buffer;
        }
        public ParseResult( FailData f )
        {
            IsSuccessful = false;
            Failure = f;
        }
    }

    public delegate ParseResult<T> Parser<T>( ParseBuffer buffer );

    public static class ParserUtil
    {
        public static Parser<B> Bind<A, B>( Parser<A> parser, Func<A, Parser<B>> gen )
        {
            return buffer => 
            {
                var result = parser( buffer );
                if ( result.IsSuccessful )
                {
                    return gen( result )( result.Buffer );
                }
                else
                {
                    return result;
                }
            };
        }
    }
}
