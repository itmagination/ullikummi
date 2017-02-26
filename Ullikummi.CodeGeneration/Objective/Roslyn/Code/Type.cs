using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
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
}
