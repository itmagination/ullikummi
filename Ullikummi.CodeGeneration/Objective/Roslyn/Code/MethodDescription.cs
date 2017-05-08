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
        public bool IsEndStateTransition { get; set; }

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
}
