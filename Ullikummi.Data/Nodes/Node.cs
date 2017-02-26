using System;
using System.Collections.Generic;

namespace Ullikummi.Data.Nodes
{
    public class Node : IConfigurable
    {
        public string Identifier { get; private set; }
        public bool IsStart { get; private set; }
        public bool IsEnd { get; private set; }
        public IDictionary<string,string> Properties { get; private set; }

        public Node(string identifier)
        {
            Identifier = identifier;

            IsStart = IsStartStateIdentifier(identifier);
            IsEnd = IsEndStateIdentifier(identifier);

            Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        private bool IsStartStateIdentifier(string identifier)
        {
            return (identifier ?? String.Empty).StartsWith("!");
        }

        private bool IsEndStateIdentifier(string identifier)
        {
            return (identifier ?? String.Empty).StartsWith("$");
        }
    }
}
