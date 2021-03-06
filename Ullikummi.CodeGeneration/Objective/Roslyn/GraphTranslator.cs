﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;
using Ullikummi.Data.Connections;
using Ullikummi.Data.Edges;
using Ullikummi.Data.Nodes;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;
using Ullikummi.CodeGeneration.Objective.Roslyn.Extensions;

namespace Ullikummi.CodeGeneration.Objective.Roslyn
{
    internal class GraphTranslator
    {
        private const string TransitionsClassNameSufix = "Transitions";
        private const string StartConnectionTransitionInternalMethodNameSufix = "Internal";

        public static CodeFile TranslateGraphToCodeFile(Graph graph)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var codeFile = new CodeFile();

            codeFile.Usings = graph.GetUsings();
            codeFile.Namespace = graph.GetNamespace();

            var transitionsClass = CreateTransitionsClass(graph);
            codeFile.Types.Add(transitionsClass);

            var builderClass = CreateBuilderClass(graph);
            codeFile.Types.Add(builderClass);

            var returnTypes = GetReturnTypes(graph);
            foreach(var returnType in returnTypes)
            {
                codeFile.Types.Add(returnType);
            }

            return codeFile;
        }

        private static IList<Code.Type> GetReturnTypes(Graph graph)
        {
            var returnTypes = new Dictionary<string, Code.Type>();

            foreach (var edge in graph.Edges)
            {
                if (edge.End.IsEnd)
                {
                    if(!returnTypes.ContainsKey(edge.End.GetReturn()))
                    {
                        var returnType = new Code.Type()
                        {
                            Name = edge.End.GetReturn(),
                            Accessibility = graph.GetAccessibility(),
                            IsPartial = true
                        };

                        returnTypes.Add(returnType.Name, returnType);
                    }
                }

            }

            return returnTypes.Values.ToList();
        }

        private static IList<MethodDescription> TranslateStartEdgeToVirtualTransitionMethodDescriptions(Graph graph, Edge edge)
        {
            var virtualTransitionMethodDescriptions = new List<MethodDescription>();

            var internalMethodName = String.Concat(edge.Connection.GetName(), StartConnectionTransitionInternalMethodNameSufix);

            var parameterPairs = edge.Connection.GetParameters();

            var internalMethod = new MethodDescription()
            {
                Name = internalMethodName,
                ReturnType = TypeName.Void(),
                Accessibility = Accessibility.Protected,
                DeclarationModifiers = DeclarationModifiers.Virtual
            };
            internalMethod.Parameters = parameterPairs.Select(ConvertToParameter).ToList();
            virtualTransitionMethodDescriptions.Add(internalMethod);

            var method = new MethodDescription()
            {
                Name = edge.Connection.GetName(),
                ReturnType = TypeName.CreateTypeName(String.Concat(GetTransitionClassName(graph), ".", String.Concat("I", edge.End.Identifier))),
                Accessibility = Accessibility.Public,
                Parameters = internalMethod.Parameters
            };

            method.StatementsBuilders.Add(syntaxGenerator => GenerateInvokeMethodSyntaxNode(syntaxGenerator, internalMethodName, internalMethod.Parameters.Select(parameter => parameter.Name)));
            method.StatementsBuilders.Add(syntaxGenerator => syntaxGenerator.ReturnStatement(syntaxGenerator.ThisExpression()));

            virtualTransitionMethodDescriptions.Add(method);

            return virtualTransitionMethodDescriptions;
        }

        private static SyntaxNode GenerateInvokeMethodSyntaxNode(SyntaxGenerator syntaxGenerator, string methodName, IEnumerable<string> arguments)
        {
            return syntaxGenerator.InvocationExpression(syntaxGenerator.IdentifierName(methodName),
                arguments.Select(argument => syntaxGenerator.IdentifierName(argument)));
        }

        private static MethodDescription TranslateEndStateToVirtualTransitionMethodDescription(Node end)
        {
            var method = new MethodDescription()
            {
                Name = end.GetName(),
                ReturnType = TypeName.CreateTypeName(end.GetReturn()),
                Accessibility = Accessibility.Protected,
                DeclarationModifiers = DeclarationModifiers.Virtual,
                IsEndStateTransition = true
            };

            var endParameterPairs = end.GetParameters();
            method.Parameters = endParameterPairs.Select(ConvertToParameter).ToList();

            method.StatementsBuilders.Add(
                (syntaxGenerator) => syntaxGenerator.ReturnStatement(syntaxGenerator.DefaultExpression(method.ReturnType.ToSyntaxNode(syntaxGenerator))));

            return method;
        }

        private static MethodDescription TranslateTransitionConnectionToVirtualTransitionMethodDescription(Connection connection)
        {
            var method = new MethodDescription()
            {
                Name = connection.GetName(),
                ReturnType = TypeName.Void(),
                Accessibility = Accessibility.Protected,
                DeclarationModifiers = DeclarationModifiers.Virtual
            };

            var parameterPairs = connection.GetParameters();
            method.Parameters = parameterPairs.Select(ConvertToParameter).ToList();

            return method;
        }

        private static IList<MethodDescription> GetVirtualTransitionMethodsDescription(Graph graph)
        {
            var startConnections = new Dictionary<Connection, Edge>();
            var endStates = new HashSet<Node>();
            var transitionConnections = new HashSet<Connection>();

            foreach (var edge in graph.Edges)
            {
                if (edge.Start.IsStart)
                {
                    if(!startConnections.ContainsKey(edge.Connection))
                    {
                        startConnections.Add(edge.Connection, edge);
                    }
                }
                else if(edge.End.IsEnd)
                {
                    endStates.Add(edge.End);
                }
                else
                {
                    transitionConnections.Add(edge.Connection);
                }
            }

            var methodDescriptions = new List<MethodDescription>();

            methodDescriptions.AddRange(startConnections.Values.SelectMany(startEdge => TranslateStartEdgeToVirtualTransitionMethodDescriptions(graph, startEdge)));
            methodDescriptions.AddRange(endStates.Select(TranslateEndStateToVirtualTransitionMethodDescription));
            methodDescriptions.AddRange(transitionConnections.Select(TranslateTransitionConnectionToVirtualTransitionMethodDescription));

            return methodDescriptions;
        }

        private static Code.Type CreateBuilderClass(Graph graph)
        {
            var name = graph.GetName();
            var accessibility = graph.GetAccessibility();

            var builderClass = new Code.Type()
            {
                Name = name,
                Accessibility = accessibility,
            };

            var interfaces = GetInterfacesDictionary(graph);

            var virtualTransitionMethodsDescription = GetVirtualTransitionMethodsDescription(graph);
            
            foreach(var transitionMethod in virtualTransitionMethodsDescription)
            {
                builderClass.Methods.Add(transitionMethod);
            }

            foreach (var @interface in interfaces.Values)
            {
                builderClass.BaseTypes.Add(TypeName.CreateTypeName(String.Concat(GetTransitionClassName(graph), ".", @interface.Name)));

                foreach(var method in @interface.Methods)
                {
                    MethodDescription methodDescription = null;
                    if (method.IsEndStateTransition)
                    {
                        methodDescription = new MethodDescription()
                        {
                            Name = String.Concat(GetTransitionClassName(graph), ".", @interface.Name, ".", method.Name),
                            ReturnType = TypeName.CreateTypeName(method.ReturnType.Name),
                            Parameters = method.Parameters.ToList()
                        };

                        methodDescription.StatementsBuilders.Add(syntaxGenerator =>
                            syntaxGenerator.ReturnStatement(GenerateInvokeMethodSyntaxNode(syntaxGenerator, method.Name, method.Parameters.Select(parameter => parameter.Name))));
                    }
                    else
                    {
                        methodDescription = new MethodDescription()
                        {
                            Name = String.Concat(GetTransitionClassName(graph), ".", @interface.Name, ".", method.Name),
                            ReturnType = TypeName.CreateTypeName(String.Concat(GetTransitionClassName(graph), ".", method.ReturnType.Name)),
                            Parameters = method.Parameters.ToList()
                        };

                        methodDescription.StatementsBuilders.Add(syntaxGenerator => GenerateInvokeMethodSyntaxNode(syntaxGenerator, method.Name, method.Parameters.Select(parameter => parameter.Name)));
                        methodDescription.StatementsBuilders.Add(syntaxGenerator => syntaxGenerator.ReturnStatement(syntaxGenerator.ThisExpression()));
                    }

                    builderClass.Methods.Add(methodDescription);
                }
            }

            return builderClass;
        }

        private static string GetTransitionClassName(Graph graph)
        {
            var name = graph.GetName();
            return String.Concat(name, TransitionsClassNameSufix);
        }

        private static Code.Type CreateTransitionsClass(Graph graph)
        {
            var accessibility = graph.GetAccessibility();

            var transitionsClass = new Code.Type()
            {
                Name = GetTransitionClassName(graph),
                Accessibility = accessibility
            };

            var interfaces = GetInterfacesDictionary(graph);

            foreach (var @interface in interfaces.Values)
            {
                transitionsClass.InternalTypes.Add(@interface);
            }

            return transitionsClass;
        }

        private static IDictionary<string, Interface> GetInterfacesDictionary(Graph graph)
        {
            var interfaces = new Dictionary<string, Interface>();

            foreach (var edge in graph.Edges)
            {
                if (edge.Start.IsStart)
                {
                    continue;
                }

                var @interface = SafeGetOrAddStartNodeTypeInterface(interfaces, edge);

                var method = edge.End.IsEnd ? GetEndStateTransitionMethodDescription(edge)
                    : GetInternalTransitionMethodDescription(edge);

                @interface.Methods.Add(method);
            }

            return interfaces;
        }

        private static MethodDescription GetEndStateTransitionMethodDescription(Edge edge)
        {
            var method = new MethodDescription()
            {
                Name = edge.End.GetName(),
                ReturnType = TypeName.CreateTypeName(edge.End.GetReturn()),
                IsEndStateTransition = true
            };

            var parameterPairs = edge.End.GetParameters();
            method.Parameters = parameterPairs.Select(ConvertToParameter).ToList();

            return method;
        }

        private static MethodDescription GetInternalTransitionMethodDescription(Edge edge)
        {
            var method = new MethodDescription()
            {
                Name = edge.Connection.GetName(),
                ReturnType = TypeName.CreateTypeName(String.Concat("I", edge.End.Identifier))
            };

            var parameterPairs = edge.Connection.GetParameters();
            method.Parameters = parameterPairs.Select(ConvertToParameter).ToList();

            return method;
        }

        private static Interface SafeGetOrAddStartNodeTypeInterface(IDictionary<string, Interface> interfaces, Edge edge)
        {
            if (!interfaces.ContainsKey(edge.Start.Identifier))
            {
                interfaces.Add(edge.Start.Identifier, new Interface()
                {
                    Name = edge.Start.Identifier,
                    Accessibility = Accessibility.Public
                });
            }

            var @interface = interfaces[edge.Start.Identifier];

            return @interface;
        }

        private static Parameter ConvertToParameter(ParameterPair parameterPair)
        {
            return new Parameter()
            {
                Type = TypeName.CreateTypeName(parameterPair.Type),
                Name = parameterPair.Name
            };
        }
    }
}
