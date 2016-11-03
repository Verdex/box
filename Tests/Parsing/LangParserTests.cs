
using Box.Tests;

using Box.Parsing;

namespace Box.Tests.Parsing
{
    public class LangParserTests
    {
        [Test]
        public void BoolParsesTrue()
        {
            var buffer = new ParseBuffer( @"true", 0 );
            var ret = LangParser.Boolean( buffer );

            Assert.IsTrue( ret.IsSuccessful );
        
            Assert.IsTrue( ret.Result != null ); // TODO replace with not null assert method
            Assert.IsTrue( ret.Result.Value );
        }
    }
}