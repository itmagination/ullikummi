using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Ullikummi.Data;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Extensions
{
    internal static class GraphExtensions
    {
        public const string DefaultName = "Ullikummi";
        public const string DefaultNamespace = "Ullikummi";
        public const Accessibility DefaultAccessibility = Accessibility.Public;

        public static string GetName(this Graph graph)
        {
            if (!graph.Metadata.ContainsKey(GraphConst.Metadata.Name))
            {
                return DefaultName;
            }

            return graph.Metadata[GraphConst.Metadata.Name];
        }

        public static IList<string> GetUsings(this Graph graph)
        {
            if (!graph.Metadata.ContainsKey(GraphConst.Metadata.Using))
            {
                return new List<string>();
            }

            var namespaces = graph.Metadata[GraphConst.Metadata.Using];

            return namespaces.Split(new[] { GraphConst.Metadata.UsingNamespaceSeparator }, StringSplitOptions.None);
        }

        public static string GetNamespace(this Graph grap)
        {
            if (!grap.Metadata.ContainsKey(GraphConst.Metadata.Namespace))
            {
                return DefaultNamespace;
            }

            return grap.Metadata[GraphConst.Metadata.Namespace];
        }

        public static Accessibility GetAccessibility(this Graph graph)
        {
            if (!graph.Metadata.ContainsKey(GraphConst.Metadata.Accessibility))
            {
                return DefaultAccessibility;
            }

            var accessibilityString = graph.Metadata[GraphConst.Metadata.Accessibility];

            Accessibility accessibility;
            if (!Enum.TryParse(accessibilityString, true, out accessibility))
            {
                throw new ApplicationException($"Unknown accessibility value {accessibilityString}!");
            }

            return accessibility;
        }
    }
}
