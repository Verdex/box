
using System;
using System.Reflection;

namespace Box.Tests
{
    public static class TestRunner
    {
        public static void RunTests<T>()
        {
            var x = typeof( T ).GetMethods( BindingFlags.Public );
            foreach ( var xlet in x )
            {
                foreach( var z in xlet.GetCustomAttributes( typeof( Test ), true ) )
                {
                    Console.WriteLine( "Blarg" + z );
                }
            }

        }
    }

    public static class Assert
    {
        
    }

    public class Test : Attribute
    {

    }
}
