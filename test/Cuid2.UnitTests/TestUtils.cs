namespace Rosser.Ids.UnitTests;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using System.Linq;
using System.Numerics;
using Rosser.Ids;

internal static class TestUtils
{
    public static int[] BuildHistogram(BigInteger[] numbers, int bucketCount = 20)
    {
        int[] buckets = new int[bucketCount];
        int counter = 1;
        BigInteger bucketLength = BigInteger.Pow(36, 23) / new BigInteger(bucketCount);

        foreach (BigInteger number in numbers)
        {
            if (counter % bucketLength == 0)
            {
                Console.WriteLine(number);

                int bucket = (int)(number / bucketLength);
                if (counter % bucketLength == 0)
                {
                    Console.WriteLine(bucket);
                }

                buckets[bucket] += 1;
                counter++;
            }
        }

        return buckets;
    }

    public static (string[] ids, BigInteger[] numbers, int[] histogram) CreateIdPool(int max = 100000)
    {
        ConcurrentDictionary<string, object?> set = new();

        IEnumerable<int> range = Enumerable.Range(0, max);
        ConcurrentBag<int> collisions = [];
        range.AsParallel().ForAll(i =>
        {
            if (!set.TryAdd(Cuid2.CreateId(), null))
            {
                collisions.Add(i);
                Info($"Collision at: {i}");
            }
        });

        if (collisions.Count > 0)
        {
            Info($"Collisions detected: {collisions.Count}");
        }
        else
        {
            Info("No collisions detected");
        }

        string[] ids = [.. set.Keys];
        BigInteger[] numbers = ids.Select(x => IdToBigInt(x.Substring(1))).ToArray();
        int[] histogram = BuildHistogram(numbers);
        return (ids, numbers, histogram);
    }

    public static BigInteger IdToBigInt(string id, int radix = 36)
    {
        return id.Aggregate(
            new BigInteger(0),
            (r, v) => r * radix + Base36Encoder.Alphabet.IndexOf(v));
    }

    public static void Info(string text)
    {
        Console.WriteLine($"# - {text}");
    }

    public static void Info<T>(T[] values)
    {
        Console.WriteLine($"# - {string.Join(", ", values)}");
    }
}