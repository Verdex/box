
UTILS = Utils/Maybe.cs Utils/DisplayArray.cs Utils/Extend.cs
AST = AST/Nodes.cs
PARSING = Parsing/ParserUtil.cs Parsing/LangParser.cs

langTest : 
	dmcs $(UTILS) $(AST) $(PARSING) Main.cs -out:Test.exe

clean : 
	rm -rf *.exe
