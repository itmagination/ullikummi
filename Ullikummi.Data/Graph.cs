using System.Collections.Generic;
using Ullikummi.Data.Edges;
using Ullikummi.Data.Nodes;

namespace Ullikummi.Data
{
    public class Graph
    {
        public IDictionary<string, Node> Nodes { get; set; }
        public IList<Edge> Edges { get; set; }

        public Graph()
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();
        }
    }
}
