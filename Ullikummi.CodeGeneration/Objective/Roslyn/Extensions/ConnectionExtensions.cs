using System.Collections.Generic;
using Ullikummi.Data.Connections;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Extensions
{
    internal static class ConnectionExtensions
    {
        public static string GetName(this Connection connection)
        {
            return PropertiesHelper.GetName(connection);
        }

        public static IList<ParameterPair> GetParameters(this Connection connection)
        {
            return PropertiesHelper.GetParameters(connection);
        }
    }
}
