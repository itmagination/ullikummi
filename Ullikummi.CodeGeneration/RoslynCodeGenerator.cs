using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;

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
    }

    public static class GraphExtensions
    {
        public const string DefaultNamespace = "Ullikummi";
        public const Accessibility DefaultAccessibility = Accessibility.Public;

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

    public class RoslynCodeGenerator
    {
        public class Parameters
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }

        public class MethodDescription
        {
            public string ReturnType { get; set; }
            public string Name { get; set; }
            public IList<Parameters> Parameters { get; set; }

            public MethodDescription()
            {
                Parameters = new List<Parameters>();
            }
        }

        public class Type
        {
            protected string _name;
            public IList<MethodDescription> Methods { get; set; }
            public IList<Type> InternalTypes { get; set; }

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
        }

        public class Interface : Type
        {
            public override string Name
            {
                get { return String.Concat("I", _name); }
            } 
        }

        public class CodeFile
        {
            public IList<string> Usings { get; set; }
            public string Namespace { get; set; }
            public IList<Type> Types { get; set; }
            public Accessibility Accessibility { get; set; }

            public CodeFile()
            {
                Usings = new List<string>();
                Types = new List<Type>();
            }
        }

        public CodeFile TranslateGraphToCodeFile(Graph graph)
        {
            if(graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var codeFile = new CodeFile();

            codeFile.Usings = graph.GetUsings();
            codeFile.Namespace = graph.GetNamespace();
            codeFile.Accessibility = graph.GetAccessibility();

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
            foreach(var @namespace in codeFile.Usings)
            {
                syntaxNodes.Add(generator.NamespaceImportDeclaration(@namespace));
            }

            var namespaceDeclaration = generator.NamespaceDeclaration(codeFile.Namespace);
            syntaxNodes.Add(namespaceDeclaration);

            var compilationUnit = generator.CompilationUnit(syntaxNodes).NormalizeWhitespace();

            return compilationUnit.ToFullString();

            //var internalStateInterfaces = new Dictionary<string, SyntaxNode>();

            //foreach(var edge in graph.Edges)
            //{
            //    if(edge.Connection != null)
            //    {
            //        var interfaceNameWithoutPrefix = edge.Start.Identifier;

            //        SyntaxNode interfaceDeclaration = null;
            //        internalStateInterfaces.TryGetValue(interfaceNameWithoutPrefix, out interfaceDeclaration);
            //        if(interfaceDeclaration == null)
            //        {
            //            var interfaceName = String.Concat("I", interfaceNameWithoutPrefix);

            //            interfaceDeclaration = generator.InterfaceDeclaration(interfaceName, null, Accessibility.Public);

            //            interfaceDeclaration.ReplaceNodes()

            //        }
            //    }
            //}
        }
    }
}
