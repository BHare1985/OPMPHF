using NUnit.Framework;

namespace OPMPHF.Tests;

public class Tests
{
    private static Func<string, int> ExampleRandomHashFunction(IList<string> strings, IReadOnlyList<int> output)
    {
        return str => output[strings.IndexOf(str)];
    }

    [Test]
    public void SmokeTest()
    {
        const int m = 12; // Define the domain of g

        var keys = new List<string>
        {
            "abacus", "cat", "dog", "flop", "home", "house", "son", "trip", "zoo"
        };

        var hashFunction = new OrderPreservingMinimalPerfectHash(m, new[]
        {
            ExampleRandomHashFunction(keys, new[] { 1, 7, 5, 4, 1, 0, 8, 11, 5 }),
            ExampleRandomHashFunction(keys, new[] { 6, 2, 7, 6, 10, 1, 11, 9, 3 })
        });

        hashFunction.Construct(keys);

        foreach (var key in keys)
        {
            var hash = hashFunction.Hash(key);
            Console.WriteLine($"h({key}) = " + hash);
            Assert.That(hash, Is.EqualTo(keys.IndexOf(key)));
        }
    }
}