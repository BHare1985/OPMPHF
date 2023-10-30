using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace OPMPHF;

public class OrderPreservingMinimalPerfectHash
{
    private readonly UndirectedGraph _graph;
    private readonly IList<Func<string, int>> _rndHashFuncs;

    private static int Djbx33AHash(string key, int start)
    {
        return key.Aggregate(start, (current, character) => (current << 5) + current + character);
    }
    
    public static int ConvertSHA256ToInt32(string input, int seed)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seed + input));

            // Take the first 4 bytes (32 bits) and convert them to an int.
            int result = BitConverter.ToInt32(hashBytes, 0);

            return result;
        }
    }
    
    public OrderPreservingMinimalPerfectHash(int numNodes, IList<Func<string, int>>? rndHashFuncs = null)
    {
        _graph = new UndirectedGraph(numNodes);

        if (rndHashFuncs != null)
            _rndHashFuncs = rndHashFuncs;
        else
            _rndHashFuncs = new List<Func<string, int>>
            {
                key => ConvertSHA256ToInt32(key, 2556),
                key => ConvertSHA256ToInt32(key, 3424),
            };
    }

    public void Construct(List<string> keys)
    {
        var success = false;
        var seed = 0;
        for (; success == false && seed < 9999; seed++)
        {
            _graph.Reset();
            foreach (var key in keys)
            {
                var h1T = InternalHash(_rndHashFuncs[0], (seed == 0 ? "" : seed) + key);
                var h2T = InternalHash(_rndHashFuncs[1], (seed == 0 ? "" : seed) + key);

                var desiredHash = keys.IndexOf(key);
                if (h1T == h2T)
                {
                    success = false;
                    break;
                }
                success = _graph.AddEdge((h1T, h2T), desiredHash);
                if (!success)
                {
                    Trace.WriteLine((h1T, h2T));
                    break;
                }
                   
            }
        }

        if (success == false)
            throw new Exception("Cannot construct perfect hash function");

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
        return _rndHashFuncs.Sum(h => _graph.Nodes[InternalHash(h,key)].Label!.Value) % _graph.NumEdges;
    }

    private int InternalHash(Func<string, int> hashFunc, string key)
    {
        return Mod(hashFunc(key), _graph.Nodes.Count);
    }

    private static int Mod(int k, int n)
    {
        return (k %= n) < 0 ? k+n : k;
    }
}