using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
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

        public override Graph VisitEdge([NotNull] DataFileGrammarParser.EdgeContext context)
        {
            Visit(context.state_identifier(0));
            Visit(context.state_identifier(1));
            var startNodeIdentifier = context.state_identifier(0).StateIdentifier;
            var endNodeIdentifier = context.state_identifier(1).StateIdentifier;

            AddNodeIfNotExist(startNodeIdentifier);
            AddNodeIfNotExist(endNodeIdentifier);

            var edge = new Edge(null, graph.Nodes[startNodeIdentifier], graph.Nodes[endNodeIdentifier]);
            graph.Edges.Add(edge);

            return base.VisitEdge(context);
        }

        public override Graph VisitState_identifier([NotNull] DataFileGrammarParser.State_identifierContext context)
        {
            context.StateIdentifier = String.Concat(context.START_STATE_PREFIX()?.GetText() ?? String.Empty,
                context.END_STATE_PREFIX()?.GetText() ?? String.Empty,
                context.IDENTIFIER().GetText()
                );
            return base.VisitState_identifier(context);
        }

        private void AddNodeIfNotExist(string nodeIdentifier)
        {
            if (!graph.Nodes.ContainsKey(nodeIdentifier))
            {
                var node = new Node(nodeIdentifier);
                graph.Nodes.Add(nodeIdentifier, node);
            }
        }
    }
}
