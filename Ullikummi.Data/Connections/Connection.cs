using System;
using System.Collections.Generic;

namespace Ullikummi.Data.Connections
{
    public class Connection : IConfigurable
    {
        public string Identifier { get; private set; }
        public IDictionary<string, string> Properties { get; private set; }

        public Connection(string identifier)
        {
            Identifier = identifier;
            Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
