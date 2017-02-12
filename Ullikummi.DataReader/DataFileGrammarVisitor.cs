﻿using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using Ullikummi.Data;
using Ullikummi.Data.Connections;
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
            if(context.connection_identity() != null)
            {
                Visit(context.connection_identity());
            }

            var startNodeIdentifier = context.state_identifier(0).StateIdentifier;
            var endNodeIdentifier = context.state_identifier(1).StateIdentifier;

            var connectionIdentity = context.connection_identity()?.ConnectionIdentity;

            AddNodeIfNotExist(startNodeIdentifier);
            AddNodeIfNotExist(endNodeIdentifier);

            Connection connection = null;

            if(connectionIdentity != null)
            {
                AddConnectionIfNotExist(connectionIdentity);
                connection = graph.Connections[connectionIdentity];
            }

            var edge = new Edge(connection, graph.Nodes[startNodeIdentifier], graph.Nodes[endNodeIdentifier]);
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

                if(node.IsStart && graph.Nodes.Values.Any(n => n.IsStart))
                {
                    throw new ApplicationException("Only single start state is allowed!");
                }

                graph.Nodes.Add(nodeIdentifier, node);
            }
        }

        private void AddConnectionIfNotExist(string connectionIdentifier)
        {
            if(!graph.Connections.ContainsKey(connectionIdentifier))
            {
                var connection = new Connection(connectionIdentifier);
                graph.Connections.Add(connectionIdentifier, connection);
            }
        }

        public override Graph VisitConnection_identity([NotNull] DataFileGrammarParser.Connection_identityContext context)
        {
            context.ConnectionIdentity = context.IDENTIFIER().GetText();
            return base.VisitConnection_identity(context);
        }

        public override Graph VisitObject_parameter([NotNull] DataFileGrammarParser.Object_parameterContext context)
        {
            context.Parameter = context.IDENTIFIER(0).GetText();
            context.Value = context.IDENTIFIER(1).GetText();
            return base.VisitObject_parameter(context);
        }

        public override Graph VisitObject_parameters([NotNull] DataFileGrammarParser.Object_parametersContext context)
        {
            context.Parameters = new Dictionary<string, string>();
            foreach(var object_parameterContext in context.object_parameter())
            {
                Visit(object_parameterContext);
                if(context.Parameters.ContainsKey(object_parameterContext.Parameter))
                {
                    throw new ApplicationException($"Parameter '{object_parameterContext.Parameter}' was already defined!");
                }
                context.Parameters.Add(object_parameterContext.Parameter, object_parameterContext.Value);
            }
            return base.VisitObject_parameters(context);
        }

        public override Graph VisitNode_metadata([NotNull] DataFileGrammarParser.Node_metadataContext context)
        {
            Visit(context.state_identifier());

            var nodeIdentifier = context.state_identifier().StateIdentifier;
            AddNodeIfNotExist(nodeIdentifier);

            Visit(context.object_parameters());

            var nodeParameters = context.object_parameters().Parameters;

            if(nodeParameters != null)
            {
                foreach(var nodeParameter in nodeParameters)
                {
                    graph.Nodes[nodeIdentifier].Properties.Add(nodeParameter.Key, nodeParameter.Value);
                }
            }

            return base.VisitNode_metadata(context);
        }

        public override Graph VisitConnection_metadata([NotNull] DataFileGrammarParser.Connection_metadataContext context)
        {
            var connectionIdentity = context.IDENTIFIER().GetText();
            AddConnectionIfNotExist(connectionIdentity);

            Visit(context.object_parameters());

            var connectionParameters = context.object_parameters().Parameters;

            if (connectionParameters != null)
            {
                foreach (var nodeParameter in connectionParameters)
                {
                    graph.Connections[connectionIdentity].Properties.Add(nodeParameter.Key, nodeParameter.Value);
                }
            }

            return base.VisitConnection_metadata(context);
        }
    }
}
