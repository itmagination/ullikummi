using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Ullikummi.Data;
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

            var name = graph.GetName();
            var accessibility = graph.GetAccessibility();

            var transitionsClass = new Objective.Roslyn.Code.Type()
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

                if (edge.End.IsEnd)
                {
                    //TODO
                    continue;
                }

                if (!interfaces.ContainsKey(edge.Start.Identifier))
                {
                    interfaces.Add(edge.Start.Identifier, new Interface()
                    {
                        Name = edge.Start.Identifier,
                        Accessibility = Accessibility.Public
                    });
                }

                var @interface = interfaces[edge.Start.Identifier];

                var method = new MethodDescription()
                {
                    Name = edge.Connection.GetName(),
                    ReturnType = new TypeName() { Name = String.Concat("I", edge.End.Identifier) }
                };

                var parameterPairs = edge.Connection.GetParameters();

                foreach (var parameterPair in parameterPairs)
                {
                    var parameter = new Parameter
                    {
                        Type = new TypeName() { Name = parameterPair.Type },
                        Name = parameterPair.Name
                    };
                    method.Parameters.Add(parameter);
                }

                @interface.Methods.Add(method);
            }

            foreach (var @interface in interfaces.Values)
            {
                transitionsClass.InternalTypes.Add(@interface);
            }

            codeFile.Types.Add(transitionsClass);

            return codeFile;
        }
    }
}
