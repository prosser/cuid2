namespace Rosser.Ids;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using OnixLabs.Security.Cryptography;

public static class Cuid2
{
    internal const int DefaultLength = 25;
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
    private const int BigLength = 32;

    // ~22k hosts before 50% chance of initial counter collision
    // with a remaining counter range of 9.0e+15 in JavaScript.
    private const int InitialCountMax = 476782367;

    /// <summary>
    /// Creates a new CUID2 id with the default length of 25 characters.
    /// </summary>
    /// <returns></returns>
    public static string CreateId()
    {
        return Init()();
    }

    public static string CreateId(int length)
    {
        return Init(length: length)();
    }

    public static Func<string> Init(
        // Fallback if the user does not pass in a CSPRNG. This should be OK
        // because we don't rely solely on the random number generator for entropy.
        // We also use the host fingerprint, current time, and a session counter.
        RandomNumberGenerator? random = null,
        Func<long>? counter = null,
        int length = DefaultLength,
        string? fingerprint = null)
    {
        if (length is < 2 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 2 and 32.");
        }

        random ??= RandomNumberGenerator.Create();
        counter ??= CreateCounter(RandomInt(random) * (long)InitialCountMax);
        fingerprint ??= CreateFingerprint(random);

        return () =>
        {
            char firstLetter = RandomLetter(random);

            // If we're lucky, the `.toString(36)` calls may reduce hashing rounds
            // by shortening the input to the hash function a little.
            string time = DateTimeOffset.UtcNow.Ticks.ToBase36String();
            string count = counter().ToBase36String();

            // The salt should be long enough to be globally unique across the full
            // length of the hash. For simplicity, we use the same length as the
            // intended id output.
            string salt = CreateEntropy(length, random);
            string hashInput = time + salt + count + fingerprint;

            return $"{firstLetter}{Hash(hashInput).Substring(1, length - 1)}";
        };
    }

    public static bool IsCuid(string id, int minLength = 2, int maxLength = BigLength)
    {
        int length = id.Length;
        return length >= minLength &&
          length <= maxLength &&
          id.All(char.IsLetterOrDigit);
    }

    internal static BigInteger BufToBigInt(byte[] buf)
    {
        BigInteger value = 0;
        foreach (byte b in buf)
        {
            value = (value << 8) + b;
        }

        return value;
    }

    internal static Func<long> CreateCounter(long count)
    {
        return () => count++;
    }

    internal static string CreateEntropy(int length = 4, RandomNumberGenerator? random = null)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");
        }

        random ??= RandomNumberGenerator.Create();
        byte[] buffer = new byte[length];
        random.GetBytes(buffer);

        StringBuilder entropy = new(length);
        for (int i = 0; i < length; i++)
        {
            _ = entropy.Append(Alphabet[buffer[i] % 36]);
        }

        return entropy.ToString();
    }

    internal static string CreateFingerprint(RandomNumberGenerator? random = null)
    {
        random ??= RandomNumberGenerator.Create();

        string globals = string.Join("", GetEnvironmentVariableValues());
        string sourceString = globals.Length > 0
            ? globals + CreateEntropy(BigLength, random)
            : CreateEntropy(BigLength, random);

        return Hash(sourceString).Substring(0, BigLength);

        static IEnumerable<string> GetEnvironmentVariableValues()
        {
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                yield return entry.Value?.ToString() ?? "";
            }
        }
    }

    private static string Hash(string input)
    {
        // Drop the first character because it will bias the histogram
        // to the left.
        using var sha3 = Sha3.CreateSha3Hash256();
        string hash = sha3.ComputeHash(Encoding.UTF8.GetBytes(input)).ToBase36String();

        return hash.Substring(1, hash.Length - 1);
    }

    private static int RandomInt(RandomNumberGenerator random)
    {
        byte[] buffer = new byte[4];
        random.GetBytes(buffer);
        return BitConverter.ToInt32(buffer, 0);
    }

    private static char RandomLetter(RandomNumberGenerator random)
    {
        byte[] buffer = new byte[1];
        random.GetBytes(buffer);
        return Alphabet[buffer[0] % 36];
    }
}