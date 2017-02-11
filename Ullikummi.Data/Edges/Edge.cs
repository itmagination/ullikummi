using System.Collections.Generic;
using Ullikummi.Data.Nodes;

namespace Ullikummi.Data.Edges
{
    public class Edge
    {
        public string Identifier { get; private set; }
        public Node Start { get; private set; }
        public Node End { get; private set; }
        public IDictionary<string, string> Properties { get; private set; }

        public Edge(string identifier, Node start, Node end)
        {
            Identifier = identifier;
            Start = start;
            End = end;
            Properties = new Dictionary<string, string>();
        }
    }
}
