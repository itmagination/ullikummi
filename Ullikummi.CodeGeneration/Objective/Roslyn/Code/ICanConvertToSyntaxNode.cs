using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    public interface ICanConvertToSyntaxNode
    {
        SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator);
    }
}
