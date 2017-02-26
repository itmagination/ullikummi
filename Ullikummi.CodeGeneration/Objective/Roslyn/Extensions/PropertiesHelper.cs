using System;
using System.Collections.Generic;
using System.Linq;
using Ullikummi.Data;
using Ullikummi.CodeGeneration.Objective.Roslyn.Code;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Extensions
{
    internal static class PropertiesHelper
    {
        public const string DefaultReturn = "void";

        public static string GetName(IConfigurable configurable)
        {
            if (!configurable.Properties.ContainsKey(GraphConst.Properties.Name))
            {
                throw new ApplicationException($"Name was not defined.");
            }

            return configurable.Properties[GraphConst.Properties.Name];
        }

        public static string GetReturn(IConfigurable configurable)
        {
            if (!configurable.Properties.ContainsKey(GraphConst.Properties.Return))
            {
                return DefaultReturn;
            }

            return configurable.Properties[GraphConst.Properties.Return];
        }

        public static IList<ParameterPair> GetParameters(IConfigurable configurable)
        {
            if (!configurable.Properties.ContainsKey(GraphConst.Properties.Parameters))
            {
                return new List<ParameterPair>();
            }

            var result = new List<ParameterPair>();

            var joinedParams = configurable.Properties[GraphConst.Properties.Parameters];

            return joinedParams.Split(new[] { GraphConst.Properties.PropertyValuesSeparator }, StringSplitOptions.None)
                .Select(complexValue =>
                {
                    var simpleValues = complexValue.Split(new[] { GraphConst.Properties.PropertyComplexValueSeparator }, StringSplitOptions.None);
                    if (simpleValues.Count() < 2)
                    {
                        throw new ApplicationException($"Parameters list is malformed!");
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
