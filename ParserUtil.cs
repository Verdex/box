
using System;

namespace Parsing 
{
    public struct Empty
    {
    }

    public struct ParseBuffer
    {
        public int Index;
        public string Text;

        public ParseBuffer( string text, int index )
        {
            Text = text;
            Index = index;
        }
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
                    return gen( result.Result )( result.Buffer );
                }
                else
                {
                    return new ParseResult<B>( null ); // TODO put some fail data in here
                }
            };
        }

        public static Parser<A> Unit<A>( A value )
        {
            return buffer => new ParseResult<A>( value, buffer );
        } 

        public static Parser<Empty> End = buffer => 
        {
            if( buffer.Index == buffer.Text.Length )
            {
                return new ParseResult<Empty>( new Empty(), buffer );
            }
            else
            {
                return new ParseResult<Empty>( null ); // TODO fail data
            }
        };

        public static Parser<char> EatChar = buffer => 
        {
            if ( buffer.Index < buffer.Text.Length )
            {
                var index = buffer.Index;
                buffer.Index++;
                return new ParseResult<char>( buffer.Text[index], buffer );
            }
            else
            {
                return new ParseResult<char>( null ); // TODO fail data
            }
        };
    }
}
