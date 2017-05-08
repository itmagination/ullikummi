using System;
using Antlr4.Runtime;
using Ullikummi.Data;

namespace Ullikummi.DataReader
{
    public class DataReader
    {
        public Graph ReadData(string data)
        {
            var input = new AntlrInputStream(data);
            var lexer = new DataFileGrammarLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new DataFileGrammarParser(tokens);
            var tree = parser.data();
            //Console.WriteLine(tree.ToStringTree(parser));
            var visitor = new DataFileGrammarVisitor();

            return visitor.Visit(tree);
        }
    }
}
