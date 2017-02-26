using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;
using Ullikummi.CodeGeneration.Objective.Roslyn.Extensions;

namespace Ullikummi.CodeGeneration
{
    internal class RoslynCodeGenerator
    {
        private const string TransitionsClassNameSufix = "Transitions";

        public CodeFile TranslateGraphToCodeFile(Graph graph)
        {
            if(graph == null)
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

            foreach(var edge in graph.Edges)
            {
                if(edge.Start.IsStart)
                {
                    continue;
                }

                if(edge.End.IsEnd)
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

                foreach(var parameterPair in parameterPairs)
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

            foreach(var @interface in interfaces.Values)
            {
                transitionsClass.InternalTypes.Add(@interface);
            }

            codeFile.Types.Add(transitionsClass);

            return codeFile;
        }

        public string GenerateCode(Graph graph, string language)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }
            if (String.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException("Value cannot be null nor whitespace.", nameof(language));
            }

            var codeFile = TranslateGraphToCodeFile(graph);

            // Get a workspace
            var workspace = new AdhocWorkspace();

            // Get the SyntaxGenerator for the specified language
            var generator = SyntaxGenerator.GetGenerator(workspace, language);

            var syntaxNodes = new List<SyntaxNode>();


            // Create using/Imports directives
            foreach (var @namespace in codeFile.Usings)
            {
                syntaxNodes.Add(generator.NamespaceImportDeclaration(@namespace));
            }

            var typesSyntaxNodes = codeFile.Types.Select(type => type.ToSyntaxNode(generator));

            var namespaceDeclaration = generator.NamespaceDeclaration(codeFile.Namespace, typesSyntaxNodes);
            syntaxNodes.Add(namespaceDeclaration);

            var compilationUnit = generator.CompilationUnit(syntaxNodes).NormalizeWhitespace();

            return compilationUnit.ToFullString();
        }
    }
}
