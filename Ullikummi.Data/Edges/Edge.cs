using System.Collections.Generic;
using Ullikummi.Data.Connections;
using Ullikummi.Data.Nodes;

namespace Ullikummi.Data.Edges
{
    public class Edge
    {
        public Connection Connection { get; private set; }
        public Node Start { get; private set; }
        public Node End { get; private set; }
        public IDictionary<string, string> Properties { get; private set; }

        public Edge(Connection connection, Node start, Node end)
        {
            Connection = connection;
            Start = start;
            End = end;
            Properties = new Dictionary<string, string>();
        }
    }
}
