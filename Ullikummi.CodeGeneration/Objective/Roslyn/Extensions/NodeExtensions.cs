using System.Collections.Generic;
using Ullikummi.Data.Nodes;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Extensions
{
    internal static class NodeExtensions
    {
        public static string GetName(this Node node)
        {
            return PropertiesHelper.GetName(node);
        }

        public static IList<ParameterPair> GetParameters(this Node node)
        {
            return PropertiesHelper.GetParameters(node);
        }

        public static string GetReturn(this Node node)
        {
            return PropertiesHelper.GetReturn(node);
        }
    }
}
