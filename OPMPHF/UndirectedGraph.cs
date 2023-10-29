namespace OPMPHF;

public class UndirectedGraph
{
    private readonly Dictionary<(int v, int w), int> _edges = new();

    public readonly List<Node> Nodes = new();

    public UndirectedGraph(int numNodes)
    {
        for (var i = 0; i < numNodes; i++)
            Nodes.Add(new Node());
    }

    public int NumEdges => _edges.Count;

    public void AddEdge((int v, int w) edge, int weight)
    {
        var (v, w) = edge;

        if (v >= Nodes.Count || w >= Nodes.Count)
            throw new ArgumentException("Invalid node number specified.");

        Nodes[v].NeighborIndices.Add(w);
        Nodes[w].NeighborIndices.Add(v);

        _edges[ReorderEdge(edge)] = weight;
    }

    private static (int, int) ReorderEdge((int v, int w) edge)
    {
        var (v, w) = edge;

        // treat (v, w) and (w, v) as the same edge
        return v <= w ? (v, w) : (w, v);
    }

    public IEnumerable<int> GetNeighboringNodeIndices(int v)
    {
        return Nodes[v].NeighborIndices;
    }

    public int GetEdgeWeight((int v, int w) edge)
    {
        if (_edges.TryGetValue(ReorderEdge(edge), out var value)) return value;

        throw new ArgumentException("Edge not found.");
    }

    public class Node
    {
        internal readonly ISet<int> NeighborIndices = new HashSet<int>();

        public bool Visited { get; set; }
        public int Label { get; set; }
    }
}