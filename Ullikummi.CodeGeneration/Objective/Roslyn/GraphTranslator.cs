using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Ullikummi.Data;
using Ullikummi.Data.Edges;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;
using Ullikummi.CodeGeneration.Objective.Roslyn.Extensions;

namespace Ullikummi.CodeGeneration.Objective.Roslyn
{
    internal class GraphTranslator
    {
        private const string TransitionsClassNameSufix = "Transitions";

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

            return codeFile;
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

            foreach (var @interface in interfaces.Values)
            {
                builderClass.BaseTypes.Add(TypeName.CreateTypeName(String.Concat(GetTransitionClassName(graph), ".", @interface.Name)));

                foreach(var method in @interface.Methods)
                {
                    var methodDescription = new MethodDescription()
                    {
                        Name = String.Concat(GetTransitionClassName(graph), ".", @interface.Name, ".", method.Name),
                        ReturnType = TypeName.CreateTypeName(
                            method.IsEndStateTransition ? method.ReturnType.Name 
                            : String.Concat(GetTransitionClassName(graph), ".", method.ReturnType.Name)),
                        Parameters = method.Parameters.ToList()
                    };

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
