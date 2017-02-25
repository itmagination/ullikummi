using System.Collections.Generic;

namespace Ullikummi.DataReader
{
    public partial class DataFileGrammarParser
    {
        public partial class State_identifierContext
        {
            public string StateIdentifier { get; set; }
        }

        public partial class Connection_identityContext
        {
            public string ConnectionIdentity { get; set; }
        }

        public partial class Object_parameterContext
        {
            public string Parameter { get; set; }
            public string Value { get; set; }
        }

        public partial class Object_parametersContext
        {
            public IDictionary<string, string> Parameters { get; set; }
        }

        public partial class Parameter_valueContext
        {
            public string Separator { get; set; }
            public IList<string> Values { get; set; }
        }

        public partial class Parameter_valuesContext
        {
            public string Separator { get; set; }
            public IList<string> Values { get; set; }
        }

        public partial class Parameter_simple_valueContext
        {
            public string Separator { get; set; }
            public IList<string> Values { get; set; }
        }

        public partial class Parameter_complex_valueContext
        {
            public string Separator { get; set; }
            public IList<string> Values { get; set; }
        }
    }
}
