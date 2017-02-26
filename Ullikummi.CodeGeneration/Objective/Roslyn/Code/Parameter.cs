using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    public class Parameter : ICanConvertToSyntaxNode
    {
        public TypeName Type { get; set; }
        public string Name { get; set; }

        public SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
        {
            return syntaxGenerator.ParameterDeclaration(Name, Type.ToSyntaxNode(syntaxGenerator));
        }
    }
}
