namespace OPMPHF;

public class OrderPreservingMinimalPerfectHash
{
    private readonly int?[] _g; // Array for the g function
    private readonly Dictionary<(int, int), int> _graphEdges = new(); // Dictionary to store graph edges
    private readonly IList<Func<string, int>> _rndHashFuncs; // List of hash functions
    private readonly Dictionary<int, List<int>> _neighbors = new(); // Dictionary to store neighbors
    private readonly List<string> _s; // List of strings

    public OrderPreservingMinimalPerfectHash(List<string> strings, int m, IList<Func<string, int>>? rndHashFuncs = null)
    {
        _g = new int?[m];
        _s = strings;

        if (rndHashFuncs != null)
            _rndHashFuncs = rndHashFuncs;
        else
            throw new Exception("Must provide random hash functions");

        foreach (var t in _s)
        {
            var h1T = _rndHashFuncs[0](t);
            var h2T = _rndHashFuncs[1](t);

            if (_graphEdges.ContainsKey((h1T, h2T)) || _graphEdges.ContainsKey((h2T, h1T)))
                // Handle duplicate edge labels here
                throw new Exception("Duplicate edge label detected; STOP");

            var desiredHash = _s.IndexOf(t); // Use the index of t in S as desired hash

            _graphEdges[(h1T, h2T)] = desiredHash;
            _graphEdges[(h2T, h1T)] = desiredHash;

            // Update the neighbors data structure
            _neighbors.TryAdd(h1T, new List<int>());
            _neighbors[h1T].Add(h2T);

            _neighbors.TryAdd(h2T, new List<int>());
            _neighbors[h2T].Add(h1T);
        }

        // Label the graph using the provided LabelAcyclicGraph procedure
        LabelAcyclicGraph();
    }

    private void LabelFrom(int v, int c)
    {
        if (_g[v].HasValue && _g[v] != c)
            throw new Exception("The graph is cyclic; STOP");

        _g[v] = c;

        foreach (var neighbor in _neighbors[v])
        {
            //neighbor has been visited
            if (_g[neighbor].HasValue) continue;

            var desiredHash = _graphEdges[(v, neighbor)];
            var n = _s.Count;
            LabelFrom(neighbor, (desiredHash - c + n) % n);
        }
    }

    private void LabelAcyclicGraph()
    {
        for (var v = 0; v < _g.Length; v++)
            if (_g[v].HasValue == false)
                LabelFrom(v, 0);
    }

    public int Hash(string t)
    {
        return _rndHashFuncs.Sum(h => _g[h(t)]!.Value) % _s.Count;
    }
}