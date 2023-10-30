using System.Security.Cryptography;
using System.Text;

namespace OPMPHF;

using rndHashFunc = Func<string, int, int>;

public class OrderPreservingMinimalPerfectHash
{
    private readonly int _maxSeed;
    private readonly IList<rndHashFunc> _rndHashFuncs;
    private int[] _g = Array.Empty<int>();
    private UndirectedGraph _graph = new(0);
    private int _numKeys;
    private int _numNodes;
    private int _validSeed;

    public OrderPreservingMinimalPerfectHash(int numNodes, int maxSeed = 1000, IList<rndHashFunc>? rndHashFuncs = null)
    {
        _numNodes = numNodes;
        _maxSeed = maxSeed;

        if (rndHashFuncs != null)
            _rndHashFuncs = rndHashFuncs;
        else
            _rndHashFuncs = new List<rndHashFunc>
            {
                (key, seed) => ConvertSha256ToInt32(key, seed + 1),
                (key, seed) => ConvertSha256ToInt32(key, seed + 2)
            };
    }

    private static int ConvertSha256ToInt32(string input, int seed)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed + input));

        // Take the first 4 bytes (32 bits) and convert them to an int.
        return BitConverter.ToInt32(hashBytes, 0);
    }

    public void Import(int numKeys, int validSeed, int[] g)
    {
        _numKeys = numKeys;
        _validSeed = validSeed;
        _g = g;
    }

    public (int numNodes, int numKeys, int validSeed, int[] g) Construct(List<string> keys)
    {
        var seedFound = false;

        var seed = 0;
        for (; seedFound == false && seed < _maxSeed; seed++)
        {
            _graph = new UndirectedGraph(_numNodes);
            foreach (var key in keys)
            {
                var h1T = InternalHash(_rndHashFuncs[0], key, seed);
                var h2T = InternalHash(_rndHashFuncs[1], key, seed);

                var rank = keys.IndexOf(key);
                if (h1T == h2T || _graph.AddEdge((h1T, h2T), rank) == false)
                    break;
            }

            if (_graph.NumEdges < keys.Count)
                continue;

            seedFound = _graph.IsAcyclic();
        }

        if (seedFound == false)
            throw new Exception($"Cannot construct perfect hash function after trying {_maxSeed} seeds");

        LabelAcyclicGraph();

        _numNodes = _graph.Nodes.Count;
        _numKeys = _graph.NumEdges;
        _validSeed = seed - 1;
        _g = _graph.Nodes.Select(x => x.Label!.Value).ToArray();

        return (_numNodes, _numKeys, _validSeed, _g);
    }

    private void Label(int nodeIdx, int label)
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

            var rank = _graph.GetEdgeLabel((nodeIdx, neighborIdx));
            Label(neighborIdx, Mod(rank - label, _graph.NumEdges));
        }
    }

    private void LabelAcyclicGraph()
    {
        for (var nodeIdx = 0; nodeIdx < _graph.Nodes.Count; nodeIdx++)
            if (_graph.Nodes[nodeIdx].Visited == false)
                Label(nodeIdx, 0);
    }

    public int Hash(string key)
    {
        var sum = _rndHashFuncs
            .Select(hashFunc => InternalHash(hashFunc, key, _validSeed))
            .Select(hashKey => _g[hashKey])
            .Sum();

        return sum % _numKeys;
    }

    private int InternalHash(rndHashFunc hashFunc, string key, int seed)
    {
        return Mod(hashFunc(key, seed), _numNodes);
    }

    private static int Mod(int k, int n)
    {
        return (k %= n) < 0 ? k + n : k;
    }
}