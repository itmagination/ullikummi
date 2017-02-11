using Antlr4.Runtime.Misc;

namespace Ullikummi.DataReader
{
    class DataFileGrammarVisitor : DataFileGrammarBaseVisitor<string>
    {
        public override string VisitEdge([NotNull] DataFileGrammarParser.EdgeContext context)
        {
            return base.VisitEdge(context);
        }
    }
}
