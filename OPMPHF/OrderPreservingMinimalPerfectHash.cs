namespace OPMPHF;

public class OrderPreservingMinimalPerfectHash
{
    private readonly UndirectedGraph _graph;
    private readonly IList<Func<string, int>> _rndHashFuncs;

    public OrderPreservingMinimalPerfectHash(int numNodes, IList<Func<string, int>>? rndHashFuncs = null)
    {
        _graph = new UndirectedGraph(numNodes);

        if (rndHashFuncs != null)
            _rndHashFuncs = rndHashFuncs;
        else
            throw new Exception("Must provide random hash functions");
    }

    public void Construct(List<string> keys)
    {
        foreach (var key in keys)
        {
            var h1T = _rndHashFuncs[0](key);
            var h2T = _rndHashFuncs[1](key);

            var desiredHash = keys.IndexOf(key);
            _graph.AddEdge((h1T, h2T), desiredHash);
        }

        LabelAcyclicGraph();
    }

    private void LabelFrom(int nodeIdx, int label)
    {
        var node = _graph.Nodes[nodeIdx];
        if (node.Visited && node.Label != label)
            throw new Exception("The graph is cyclic; STOP");

        node.Label = label;
        node.Visited = true;

        foreach (var neighborIdx in _graph.GetNeighboringNodeIndices(nodeIdx))
        {
            if (_graph.Nodes[neighborIdx].Visited)
                continue;

            var desiredHash = _graph.GetEdgeWeight((nodeIdx, neighborIdx));
            var n = _graph.NumEdges;
            LabelFrom(neighborIdx, (desiredHash - label + n) % n);
        }
    }

    private void LabelAcyclicGraph()
    {
        for (var nodeIdx = 0; nodeIdx < _graph.Nodes.Count; nodeIdx++)
            if (_graph.Nodes[nodeIdx].Visited == false)
                LabelFrom(nodeIdx, 0);
    }

    public int Hash(string key)
    {
        return _rndHashFuncs.Sum(hashFunc => _graph.Nodes[hashFunc(key)].Label) % _graph.NumEdges;
    }
}