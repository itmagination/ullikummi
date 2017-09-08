using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    internal class Type : ICanConvertToSyntaxNode
    {
        protected string _name;
        public IList<MethodDescription> Methods { get; set; }
        public IList<Type> InternalTypes { get; set; }
        public Accessibility Accessibility { get; set; }
        public IList<TypeName> BaseTypes { get; set; }
        public bool IsPartial { get; set; }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Type()
        {
            Methods = new List<MethodDescription>();
            InternalTypes = new List<Type>();
            BaseTypes = new List<TypeName>();
        }

        public virtual SyntaxNode ToSyntaxNode(SyntaxGenerator syntaxGenerator)
        {
            var methodsSyntaxNodes = Methods.Select(method => method.ToSyntaxNode(syntaxGenerator));
            var internalTypesSyntaxNodes = InternalTypes.Select(internalType => internalType.ToSyntaxNode(syntaxGenerator));

            var declarationModifiers = DeclarationModifiers.None.WithPartial(IsPartial);

            var typeSyntaxNode = syntaxGenerator.ClassDeclaration(Name, 
                accessibility: Accessibility, 
                modifiers: declarationModifiers,
                members: methodsSyntaxNodes.Union(internalTypesSyntaxNodes));

            foreach(var baseType in BaseTypes)
            {
                var baseTypeSyntaxNode = baseType.ToSyntaxNode(syntaxGenerator);
                typeSyntaxNode = syntaxGenerator.AddBaseType(typeSyntaxNode, baseTypeSyntaxNode);
            }

            syntaxGenerator.AddMembers(typeSyntaxNode, Methods.Select(method => method.ToSyntaxNode(syntaxGenerator)));

            return typeSyntaxNode;
        }
    }
}
