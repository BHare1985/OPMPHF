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

    private void ResetVisited()
    {
        foreach (var n in Nodes)
            n.Visited = false;
    }

    public bool AddEdge((int v, int w) edge, int label)
    {
        var (v, w) = edge;

        if (v >= Nodes.Count || w >= Nodes.Count)
            throw new ArgumentException("Invalid node number specified.");

        Nodes[v].NeighborIndices.Add(w);
        Nodes[w].NeighborIndices.Add(v);

        return _edges.TryAdd(ReorderEdge(edge), label);
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

    public int GetEdgeLabel((int v, int w) edge)
    {
        if (_edges.TryGetValue(ReorderEdge(edge), out var value))
            return value;

        throw new ArgumentException("Edge not found.");
    }

    public bool IsAcyclic()
    {
        var rv = Nodes.Where(node => !node.Visited).All(node => !IsAcyclicDfs(node, null));
        ResetVisited();
        return rv;
    }

    private bool IsAcyclicDfs(Node current, Node? parent)
    {
        current.Visited = true;

        foreach (var neighborIndex in current.NeighborIndices)
            if (!Nodes[neighborIndex].Visited)
            {
                if (IsAcyclicDfs(Nodes[neighborIndex], current))
                    return true;
            }
            else if (Nodes[neighborIndex] != parent)
            {
                // If the neighbor has been visited and
                // is not the parent, it's a back edge, indicating a cycle.
                return true;
            }

        return false;
    }

    public class Node
    {
        internal readonly ISet<int> NeighborIndices = new HashSet<int>();

        public bool Visited { get; set; }
        public int? Label { get; set; }
    }
}