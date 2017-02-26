using System;
using System.Collections.Generic;
using System.Linq;
using Ullikummi.Data;
using Ullikummi.Data.Connections;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Extensions
{
    internal static class ConnectionExtensions
    {
        public static string GetName(this Connection connection)
        {
            if (!connection.Properties.ContainsKey(GraphConst.ConnectionProperties.Name))
            {
                throw new ApplicationException($"Name was not defined for connection '{connection.Identifier}'.");
            }

            return connection.Properties[GraphConst.ConnectionProperties.Name];
        }

        public static IList<ParameterPair> GetParameters(this Connection connection)
        {
            if (!connection.Properties.ContainsKey(GraphConst.ConnectionProperties.Parameters))
            {
                return new List<ParameterPair>();
            }

            var result = new List<ParameterPair>();

            var joinedParams = connection.Properties[GraphConst.ConnectionProperties.Parameters];

            return joinedParams.Split(new[] { GraphConst.ConnectionProperties.ValuesSeparator }, StringSplitOptions.None)
                .Select(complexValue =>
                {
                    var simpleValues = complexValue.Split(new[] { GraphConst.ConnectionProperties.ComplexValueSeparator }, StringSplitOptions.None);
                    if (simpleValues.Count() < 2)
                    {
                        throw new ApplicationException($"Parameters list in connection '{connection.Identifier}' is malformed!");
                    }

                    var parameterPair = new ParameterPair()
                    {
                        Type = simpleValues[0],
                        Name = simpleValues[1]
                    };

                    return parameterPair;
                })
                .ToList();
        }
    }
}
