
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Box.Tests
{
    public static class TestRunner
    {
        public static void RunTests<T>()
            where T : new()
        {
            var results = new List<string>();
            var target = new T();
            var methods = typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance );
            foreach ( var method in methods )
            {
                if ( method.GetCustomAttributes( typeof( Test ), true ).Length != 0 )
                {
                    var pass = false;
                    try
                    {
                        method.Invoke( target, null );
                        pass = true;   
                    }
                    catch ( TargetInvocationException e )
                    {
                        var ae = e.InnerException as AssertException;
                        if ( ae != null )
                        {
                            results.Add( method.Name +  ": Failed: " + ae.Message );
                        }
                        else
                        {
                            results.Add( method.Name + ": Failed due to unexpected exception: " + e.InnerException );
                        }
                    }
                    if ( pass )
                    {
                        results.Add( method.Name + ": pass" );
                    }
                }
            }

            // TODO should probably return this
            foreach( var result in results )
            {
                Console.WriteLine( result );
            }
        }
    }

    public static class Assert
    {   
        public static void IsTrue( bool value )
        {
            if ( !value )
            {
                throw new AssertException("Expected true got false.");
            }
        }   
    }

    public class Test : Attribute
    {

    }

    public class AssertException : Exception
    {
        public AssertException(string message)
            : base( message )
        {
        }
    }
}
