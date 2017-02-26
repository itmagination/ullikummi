using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;
using Microsoft.CodeAnalysis.CSharp;
using Ullikummi.Data.Connections;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;

namespace Ullikummi.CodeGeneration
{
    public static class GraphConst
    {
        public class Metadata
        {
            public const string Name = "name";
            public const string Namespace = "namespace";
            public const string Using = "using";
            public const string Accessibility = "accessibility";

            public const string UsingNamespaceSeparator = "&";
        }

        public class ConnectionProperties
        {
            public const string Name = "name";
            public const string Parameters = "params";

            public const string ValuesSeparator = ";";
            public const string ComplexValueSeparator = "#";
        }
    }

    public static class GraphExtensions
    {
        public const string DefaultName = "Ullikummi";
        public const string DefaultNamespace = "Ullikummi";
        public const Accessibility DefaultAccessibility = Accessibility.Public;

        public static string GetName(this Graph graph)
        {
            if(!graph.Metadata.ContainsKey(GraphConst.Metadata.Name))
            {
                return DefaultName;
            }

            return graph.Metadata[GraphConst.Metadata.Name];
        }

        public static IList<string> GetUsings(this Graph graph)
        {
            if(!graph.Metadata.ContainsKey(GraphConst.Metadata.Using))
            {
                return new List<string>();
            }

            var namespaces = graph.Metadata[GraphConst.Metadata.Using];

            return namespaces.Split(new[] { GraphConst.Metadata.UsingNamespaceSeparator }, StringSplitOptions.None);
        }

        public static string GetNamespace(this Graph grap)
        {
            if(!grap.Metadata.ContainsKey(GraphConst.Metadata.Namespace))
            {
                return DefaultNamespace;
            }

            return grap.Metadata[GraphConst.Metadata.Namespace];
        }

        public static Accessibility GetAccessibility(this Graph graph)
        {
            if(!graph.Metadata.ContainsKey(GraphConst.Metadata.Accessibility))
            {
                return DefaultAccessibility;
            }

            var accessibilityString = graph.Metadata[GraphConst.Metadata.Accessibility];

            Accessibility accessibility;
            if(!Enum.TryParse(accessibilityString, true, out accessibility))
            {
                throw new ApplicationException($"Unknown accessibility value {accessibilityString}!");
            }

            return accessibility;
        }
    }

    internal static class ConnectionExtensions
    {
        public static string GetName(this Connection connection)
        {
            if (!connection.Properties.ContainsKey(GraphConst.ConnectionProperties.Name))
            {
                throw new ApplicationException($"Name was not defined for connection '{connection.Identifier}'.");
            }

            return connection.Properties[GraphConst.ConnectionProperties.Name];
        }

        public class ParameterPair
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }

        public static IList<ParameterPair> GetParameters(this Connection connection)
        {
            if(!connection.Properties.ContainsKey(GraphConst.ConnectionProperties.Parameters))
            {
                return new List<ParameterPair>();
            }

            var result = new List<ParameterPair>();

            var joinedParams = connection.Properties[GraphConst.ConnectionProperties.Parameters];

            return joinedParams.Split(new[] { GraphConst.ConnectionProperties.ValuesSeparator }, StringSplitOptions.None)
                .Select(complexValue =>
                {
                    var simpleValues = complexValue.Split(new[] { GraphConst.ConnectionProperties.ComplexValueSeparator }, StringSplitOptions.None);
                    if (simpleValues.Count() < 2)
                    {
                        throw new ApplicationException($"Parameters list in connection '{connection.Identifier}' is malformed!");
                    }

                    var parameterPair = new ParameterPair()
                    {
                        Type = simpleValues[0],
                        Name = simpleValues[1]
                    };

                    return parameterPair;
                })
                .ToList();
        }
    }

    public partial class RoslynCodeGenerator
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
