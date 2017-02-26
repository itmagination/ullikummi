using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Ullikummi.Data;

namespace Ullikummi.CodeGeneration.Objective.Roslyn
{
    internal class RoslynCodeGenerator
    {
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

            var codeFile = GraphTranslator.TranslateGraphToCodeFile(graph);

            // Get a workspace.
            var workspace = new AdhocWorkspace();

            // Get the SyntaxGenerator for the specified language.
            var generator = SyntaxGenerator.GetGenerator(workspace, language);

            var syntaxNodes = new List<SyntaxNode>();

            // Create using/Imports directives.
            foreach (var @namespace in codeFile.Usings)
            {
                syntaxNodes.Add(generator.NamespaceImportDeclaration(@namespace));
            }

            // Get types from CodeFile description.
            var typesSyntaxNodes = codeFile.Types.Select(type => type.ToSyntaxNode(generator));

            // Create namespace declaration with types.
            var namespaceDeclaration = generator.NamespaceDeclaration(codeFile.Namespace, typesSyntaxNodes);
            syntaxNodes.Add(namespaceDeclaration);

            var compilationUnit = generator.CompilationUnit(syntaxNodes).NormalizeWhitespace();

            // Materialize code itself.
            return compilationUnit.ToFullString();
        }
    }
}
