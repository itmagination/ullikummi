using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Ullikummi.Data;
using Ullikummi.Data.Edges;
using Ullikummi.Data.Nodes;

namespace Ullikummi.DataReader
{
    internal class DataFileGrammarVisitor : DataFileGrammarBaseVisitor<Graph>
    {
        private Graph graph;

        public DataFileGrammarVisitor()
        {
            graph = new Graph();
        }

        public override Graph Visit(IParseTree tree)
        {
            base.Visit(tree);
            return graph;
        }

        public override Graph VisitEdge_start([NotNull] DataFileGrammarParser.Edge_startContext context)
        {
            var startNodeIdentifier = context.START_STATE()?.GetText();
            var nodeIdentifier = context.IDENTIFIER()?.GetText();

            if(startNodeIdentifier != null)
            {
                AddNodeIfNotExist(startNodeIdentifier);
                context.Node = graph.Nodes[startNodeIdentifier];
            }
            if(nodeIdentifier != null)
            {
                AddNodeIfNotExist(nodeIdentifier);
                context.Node = graph.Nodes[nodeIdentifier];
            }

            return base.VisitEdge_start(context);
        }

        public override Graph VisitEdge_end([NotNull] DataFileGrammarParser.Edge_endContext context)
        {
            var endNodeIdentifier = context.END_STATE()?.GetText();
            var nodeIdentifier = context.IDENTIFIER()?.GetText();

            if (endNodeIdentifier != null)
            {
                AddNodeIfNotExist(endNodeIdentifier);
                context.Node = graph.Nodes[endNodeIdentifier];
            }
            if (nodeIdentifier != null)
            {
                AddNodeIfNotExist(nodeIdentifier);
                context.Node = graph.Nodes[nodeIdentifier];
            }

            return base.VisitEdge_end(context);
        }

        public override Graph VisitEdge([NotNull] DataFileGrammarParser.EdgeContext context)
        {
            Visit(context.edge_start());
            Visit(context.edge_end());

            var edge = new Edge(null, context.edge_start().Node, context.edge_start().Node);
            graph.Edges.Add(edge);

            return base.VisitEdge(context);
        }

        private void AddNodeIfNotExist(string nodeIdentifier)
        {
            if (!graph.Nodes.ContainsKey(nodeIdentifier))
            {
                var node = new Node(nodeIdentifier);
                graph.Nodes.Add(nodeIdentifier, node);
            }
        }

        public override Graph VisitEdge_identifier([NotNull] DataFileGrammarParser.Edge_identifierContext context)
        {
            var edgeIdentifier = context.IDENTIFIER().GetText();
            //TODO Pass edgeIdentifier up to edge.
            return base.VisitEdge_identifier(context);
        }
    }
}
