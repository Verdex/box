
langTest : 
	dmcs Utils.cs AST/Nodes.cs Parsing/ParserUtil.cs Parsing/LangParser.cs Main.cs -out:Test.exe

clean : 
	rm -rf *.exe
