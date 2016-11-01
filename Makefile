
Utils = Utils/Maybe.cs Utils/DisplayArray.cs Utils/Extend.cs
Ast = AST/Nodes.cs
Parsing = Parsing/ParserUtil.cs Parsing/LangParser.cs

TestFramework = Tests/Test.cs
ParsingTests = Tests/Parsing/LangParserTests.cs

langTest : 
	dmcs $(Utils) $(Ast) $(Parsing) Main.cs -out:Test.exe

test :
	rm -rf *.exe
	dmcs $(Utils) $(Ast) $(Parsing) $(TestFramework) $(ParsingTests) Test.cs -out:Test.exe
	mono Test.exe

clean : 
	rm -rf *.exe
