namespace Rosser.Ids.UnitTests;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using static TestUtils;
public class Histogram
{
    private const int N = 100000;

    [Fact]
    public void Histogram_Has_EvenCharacterFrequency()
    {
        // Arrange
        Info($"Creating {N} unique IDs...");

        (string[] ids, _, _) = CreateIdPool(N);

        HashSet<string> set = new(ids);

        // Assert
        set.Should().HaveCount(N, "there should be no collisions");
        ids.Should().HaveCount(N, "should have generated this many ids");

        const double Tolerance = 0.1;
        const int IdLength = 23;
        const int TotalLetters = IdLength * N;
        const int Base = 36;
        int expectedBinSize = TotalLetters / Base;
        double minBinSize = Math.Round(expectedBinSize * (1 - Tolerance));
        double maxBinSize = Math.Round(expectedBinSize * (1 + Tolerance));

        // Act
        // Drop the first character because it will always be a letter, making
        // the letter frequency skewed.
        IEnumerable<string> testIds = ids.Select(id => id.Substring(1));
        ConcurrentDictionary<char, int> charFrequencies = [];

        Parallel.ForEach(testIds, (id) =>
        {
            foreach (char c in id)
            {
                charFrequencies.AddOrUpdate(c, 1, (_, count) => count + 1);
            }
        });

        Info("Testing character frequency...");
        Info($"expectedBinSize: {expectedBinSize}");
        Info($"minBinSize: {minBinSize}");
        Info($"maxBinSize: {maxBinSize}");
        Info($"charFrequencies: {string.Join(", ", charFrequencies.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"))}");

        // Assert
        charFrequencies.Should().HaveCount(Base, "all characters should be used");
        foreach (int count in charFrequencies.Values)
        {
            double d = count;
            d.Should().BeInRange(minBinSize, maxBinSize, "should be within distribution tolerance");
        }
    }
}
