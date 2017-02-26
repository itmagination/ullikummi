using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    internal interface ICanConvertToSyntaxNode
    {
        SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator);
    }
}
