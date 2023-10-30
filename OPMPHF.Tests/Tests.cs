using NUnit.Framework;

namespace OPMPHF.Tests;

public class Tests
{
    private static Func<string, int, int> ExampleRandomHashFunction(IList<string> keys, IReadOnlyList<int> output)
    {
        return (key, _) => output[keys.IndexOf(key)];
    }

    [Test]
    public void SmokeTestSpec()
    {
        var keys = new List<string>
        {
            "abacus", "cat", "dog", "flop", "home", "house", "son", "trip", "zoo"
        };

        var opmph = new OrderPreservingMinimalPerfectHash(12, rndHashFuncs: new[]
        {
            ExampleRandomHashFunction(keys, new[] { 1, 7, 5, 4, 1, 0, 8, 11, 5 }),
            ExampleRandomHashFunction(keys, new[] { 6, 2, 7, 6, 10, 1, 11, 9, 3 })
        });

        opmph.Construct(keys);


        AssertHashFunction(keys, opmph.Hash);
    }

    [Test]
    public void SmokeTestMinimal()
    {
        var keys = new List<string>
        {
            "A", "B", "E", "C", "F", "D", "G", "I", "Z"
        };

        var opmph = new OrderPreservingMinimalPerfectHash(keys.Count + 1, 1, new[]
        {
            ExampleRandomHashFunction(keys, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }),
            ExampleRandomHashFunction(keys, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
        });

        opmph.Construct(keys);


        AssertHashFunction(keys, opmph.Hash);
    }

    [Test]
    public void SmokeTest()
    {
        var keys = Enumerable.Range(0, 10000).Select(i => $"key{i}").ToList();
        var opmph = new OrderPreservingMinimalPerfectHash(2 * keys.Count, int.MaxValue);
        opmph.Construct(keys);

        AssertHashFunction(keys, opmph.Hash);
    }

    [Test]
    public void ImportTest()
    {
        var keys = Enumerable.Range(0, 10).Select(i => $"key{i}").ToList();
        var m = keys.Count + 5;
        var opmph = new OrderPreservingMinimalPerfectHash(m, int.MaxValue);
        var (numNodes, numKeys, validSeed, g) = opmph.Construct(keys);
        AssertHashFunction(keys, opmph.Hash);

        Assert.AreEqual(m, g.Length);

        var newOpmph = new OrderPreservingMinimalPerfectHash(numNodes);
        newOpmph.Import(numKeys, validSeed, g);
        AssertHashFunction(keys, newOpmph.Hash);
    }

    private static void AssertHashFunction(List<string> keys, Func<string, int> hashFunction)
    {
        var expectedHashes = new List<int>();
        var actualHashes = new List<int>();
        foreach (var key in keys)
        {
            var hash = hashFunction(key);
            expectedHashes.Add(keys.IndexOf(key));
            actualHashes.Add(hash);
            Console.WriteLine($"h({key}) = " + hash);
        }

        CollectionAssert.AreEqual(actualHashes, expectedHashes);
    }
}