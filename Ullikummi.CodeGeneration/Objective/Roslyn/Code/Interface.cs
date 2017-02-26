using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
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
}
