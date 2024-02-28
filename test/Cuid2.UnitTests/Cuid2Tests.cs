namespace Rosser.Ids.UnitTests;

using System.Numerics;
using FluentAssertions;
using static TestUtils;

public class Cuid2Tests
{
    public static TheoryData<int> GetValidLengths()
    {
        TheoryData<int> data = [];

        for (int i = 2; i <= 32; i++)
        {
            data.Add(i);
        }

        return data;
    }

    [Fact]
    public void CreateId_Returns_StringOfDefaultLength()
    {
        string id = Cuid2.CreateId();
        id.Should().NotBeNullOrEmpty().And.HaveLength(Cuid2.DefaultLength);
    }

    [Theory]
    [MemberData(nameof(GetValidLengths))]
    public void CreateId_WithLength_Returns_StringOfThatLength(int length)
    {
        string id = Cuid2.CreateId(length);
        id.Should().NotBeNullOrEmpty().And.HaveLength(length);
    }

    [Fact]
    public void CreateCounter_ReturnsFuncThatIncrements()
    {
        // Arrange
        Func<long> counter = Cuid2.CreateCounter(10);
        long[] expected = [10, 11, 12, 13];

        // Act
        long[] actual = [counter(), counter(), counter(), counter()];

        Info(actual);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void BufToBigInt_WithEmptyBuffer_ReturnsBigInt0()
    {
        // Arrange
        byte[] buf = [];
        BigInteger expected = 0;

        // Act
        BigInteger actual = Cuid2.BufToBigInt(buf);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BufToBigInt_WithBuffer_ReturnsBigInt()
    {
        // Arrange
        byte[] buf = [0xff, 0xff, 0xff, 0xff];
        BigInteger expected = 4294967295;

        // Act
        BigInteger actual = Cuid2.BufToBigInt(buf);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void CreateFingerprint_ReturnsStringOfSufficientLength()
    {
        // Arrange
        string fingerprint = Cuid2.CreateFingerprint();

        // Assert
        fingerprint.Should().NotBeNullOrEmpty();
        fingerprint.Length.Should().BeGreaterOrEqualTo(24);
    }

    [Fact]
    public void IsCuid_WithValidId_ReturnsTrue()
    {
        // Arrange
        string id = Cuid2.CreateId();

        // Act
        bool result = Cuid2.IsCuid(id);

        // Assert
        result.Should().BeTrue();
    }
}