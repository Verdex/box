
using System;
using System.Linq;
using System.Collections.Generic;

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

        public static Parser<B> Map<A, B>( this Parser<A> parser, Func<A, B> f )
        {
            return buffer => 
            {
                var result = parser( buffer );
                if ( result.IsSuccessful )
                {
                    return new ParseResult<B>( f( result.Result ), result.Buffer );
                }
                else
                {
                    return new ParseResult<B>( null ); // TODO fail data
                }
            };
        }

        public static Parser<IEnumerable<A>> OneOrMore<A>( this Parser<A> parser )
        {
            return Bind( parser,              v  =>
                   Bind( parser.ZeroOrMore(), vs => 
                   Unit( new A[] { v }.Concat( vs ) ) ) );
        }

        public static Parser<IEnumerable<A>> ZeroOrMore<A>( this Parser<A> parser )
        {
            return buffer =>
            {
                var a = new List<A>();
                var result = parser( buffer );
                ParseBuffer? temp = null;
                while ( result.IsSuccessful )
                {
                    a.Add( result.Result );
                    result = parser( result.Buffer );
                    if ( result.IsSuccessful )
                    {
                        temp = result.Buffer;
                    }
                }
                if ( temp.HasValue )
                {
                    return new ParseResult<IEnumerable<A>>( a, temp.Value );
                }
                else
                {
                    return new ParseResult<IEnumerable<A>>( a, buffer ); 
                }
            };
        }

        public static Parser<A> Alternate<A>( params Parser<A>[] parsers )
        {
            return buffer => 
            {
                foreach( var p in parsers )
                {
                    var result = p( buffer );
                    if ( result.IsSuccessful )
                    {
                        return new ParseResult<A>( result.Result, result.Buffer );
                    }
                }
                return new ParseResult<A>( null ); // TODO fail data
            };
        }

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
