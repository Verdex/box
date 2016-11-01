
using Box.Tests;
using Box.Tests.Parsing;

namespace Box 
{
    public static class Program
    {
        public static void Main()
        {
            TestRunner.RunTests<LangParserTests>();
        }
    }   
}