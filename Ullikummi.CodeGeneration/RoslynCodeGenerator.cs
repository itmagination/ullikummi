using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;
using Microsoft.CodeAnalysis.CSharp;
using Ullikummi.Data.Connections;

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

    public class RoslynCodeGenerator
    {
        public interface ICanConvertToSyntaxNode
        {
            SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator);
        }

        public class Parameter : ICanConvertToSyntaxNode
        {
            public TypeName Type { get; set; }
            public string Name { get; set; }

            public SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
            {
                return syntaxGenerator.ParameterDeclaration(Name, Type.ToSyntaxNode(syntaxGenerator));
            }
        }

        public class TypeName : ICanConvertToSyntaxNode
        {
            public string Name { get; set; }

            private static readonly IReadOnlyDictionary<string, SpecialType> SpecialTypesMap = new Dictionary<string, SpecialType>
            {
                { "bool", SpecialType.System_Boolean },
                { "byte", SpecialType.System_Byte },
                { "sbyte", SpecialType.System_SByte },
                { "char", SpecialType.System_Char },
                { "decimal", SpecialType.System_Decimal },
                { "double", SpecialType.System_Double },
                { "float", SpecialType.System_Single },
                { "int", SpecialType.System_Int32 },
                { "uint", SpecialType.System_UInt32 },
                { "long", SpecialType.System_Int64 },
                { "ulong", SpecialType.System_UInt64 },
                { "object", SpecialType.System_Object },
                { "short", SpecialType.System_Int16 },
                { "ushort", SpecialType.System_UInt16 },
                { "string", SpecialType.System_String },
            };

            public SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
            {
                if (SpecialTypesMap.ContainsKey(Name))
                {
                    return syntaxGenerator.TypeExpression(SpecialTypesMap[Name]);
                }

                return syntaxGenerator.IdentifierName(Name);
            }
        }


        public class MethodDescription : ICanConvertToSyntaxNode
        {
            public TypeName ReturnType { get; set; }
            public string Name { get; set; }
            public IList<Parameter> Parameters { get; set; }
            public Accessibility Accessibility { get; set; }

            public MethodDescription()
            {
                Parameters = new List<Parameter>();
            }

            public SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
            {
                var parametersSyntaxNodes = Parameters.Select(parameter => parameter.ToSyntaxNode(syntaxGenerator));

                return syntaxGenerator.MethodDeclaration(Name, parametersSyntaxNodes, 
                    returnType: ReturnType.ToSyntaxNode(syntaxGenerator), 
                    accessibility: Accessibility);
            }
        }

        public class Type : ICanConvertToSyntaxNode
        {
            protected string _name;
            public IList<MethodDescription> Methods { get; set; }
            public IList<Type> InternalTypes { get; set; }
            public Accessibility Accessibility { get; set; }

            public virtual string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            public Type()
            {
                Methods = new List<MethodDescription>();
                InternalTypes = new List<Type>();
            }

            public virtual SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
            {
                var methodsSyntaxNodes = Methods.Select(method => method.ToSyntaxNode(syntaxGenerator));
                var internalTypesSyntaxNodes = InternalTypes.Select(internalType => internalType.ToSyntaxNode(syntaxGenerator));

                return syntaxGenerator.ClassDeclaration(Name, accessibility: Accessibility, members: methodsSyntaxNodes.Union(internalTypesSyntaxNodes));
            }
        }

        public class Interface : Type
        {
            public override string Name
            {
                get { return String.Concat("I", _name); }
            }

            public override SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
            {
                var methodsSyntaxNodes = Methods.Select(method => method.ToSyntaxNode(syntaxGenerator));

                return syntaxGenerator.InterfaceDeclaration(Name, accessibility: Accessibility, members: methodsSyntaxNodes);
            }
        }

        public class CodeFile
        {
            public IList<string> Usings { get; set; }
            public string Namespace { get; set; }
            public IList<Type> Types { get; set; }

            public CodeFile()
            {
                Usings = new List<string>();
                Types = new List<Type>();
            }
        }

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

            var transitionsClass = new Type()
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
