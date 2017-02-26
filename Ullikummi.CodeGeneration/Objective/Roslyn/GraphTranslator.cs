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

            return codeFile;
        }

        private static Code.Type CreateTransitionsClass(Graph graph)
        {
            var name = graph.GetName();
            var accessibility = graph.GetAccessibility();

            var transitionsClass = new Code.Type()
            {
                Name = String.Concat(name, TransitionsClassNameSufix),
                Accessibility = accessibility
            };

            var interfaces = new Dictionary<string, Interface>();

            foreach (var edge in graph.Edges)
            {
                if (edge.Start.IsStart)
                {
                    continue;
                }

                var @interface = SafeGetStartNodeTypeInterface(interfaces, edge);

                if (edge.End.IsEnd)
                {
                    //TODO
                    continue;
                }

                var method = GetInternalTransitionMethodDescription(edge);
                @interface.Methods.Add(method);
            }

            foreach (var @interface in interfaces.Values)
            {
                transitionsClass.InternalTypes.Add(@interface);
            }

            return transitionsClass;
        }

        private static MethodDescription GetInternalTransitionMethodDescription(Edge edge)
        {
            var method = new MethodDescription()
            {
                Name = edge.Connection.GetName(),
                ReturnType = new TypeName() { Name = String.Concat("I", edge.End.Identifier) }
            };

            var parameterPairs = edge.Connection.GetParameters();
            method.Parameters = parameterPairs.Select(ConvertToParameter).ToList();

            return method;
        }

        private static Interface SafeGetStartNodeTypeInterface(IDictionary<string, Interface> interfaces, Edge edge)
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
                Type = new TypeName() { Name = parameterPair.Type },
                Name = parameterPair.Name
            };
        }
    }
}
