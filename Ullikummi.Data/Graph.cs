using System;
using System.Collections.Generic;
using Ullikummi.Data.Connections;
using Ullikummi.Data.Edges;
using Ullikummi.Data.Nodes;

namespace Ullikummi.Data
{
    public class Graph
    {
        public IDictionary<string, Node> Nodes { get; set; }
        public IList<Edge> Edges { get; set; }
        public IDictionary<string, Connection> Connections { get; set; }
        public IDictionary<string, string> Metadata { get; set; }

        public Graph()
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();
            Connections = new Dictionary<string, Connection>();
            Metadata = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
