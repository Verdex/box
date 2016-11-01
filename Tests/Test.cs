
using System;

namespace Box.Tests
{
    public static class TestRunner
    {
        public static void RunTests<T>()
            where T : new()
        {
            var tests = new T();

        }
    }

    public static class Assert
    {
        
    }

    public class Test : Attribute
    {

    }
}
