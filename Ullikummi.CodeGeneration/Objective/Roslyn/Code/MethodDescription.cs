using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    internal class MethodDescription : ICanConvertToSyntaxNode
    {
        public TypeName ReturnType { get; set; }
        public string Name { get; set; }
        public IList<Parameter> Parameters { get; set; }
        public Accessibility Accessibility { get; set; }
        public DeclarationModifiers DeclarationModifiers { get; set; }
        public IList<Func<SyntaxGenerator, SyntaxNode>> StatementsBuilders { get; set; }

        //TODO [WS] Remove this frome here - makes no sense.
        public bool IsEndStateTransition { get; set; }

        public MethodDescription()
        {
            Parameters = new List<Parameter>();
            StatementsBuilders = new List<Func<SyntaxGenerator, SyntaxNode>>();
        }

        public SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
        {
            var parametersSyntaxNodes = Parameters.Select(parameter => parameter.ToSyntaxNode(syntaxGenerator));

            var methodSyntaxNode = syntaxGenerator.MethodDeclaration(Name, parametersSyntaxNodes,
                returnType: ReturnType.ToSyntaxNode(syntaxGenerator),
                accessibility: Accessibility,
                modifiers: DeclarationModifiers,
                statements: StatementsBuilders.Select(statementsBuilder => statementsBuilder(syntaxGenerator))
                );

            return methodSyntaxNode;
        }
    }
}
