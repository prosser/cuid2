namespace Rosser.Ids;

using System;
using System.Collections.Generic;
using System.Numerics;

internal static class Base36Encoder
{
    internal const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

    // constants that we use in ToBase36CharArray
    private static readonly double Base36CharsLengthDivisor = Math.Log(Alphabet.Length, 2);

    private static readonly BigInteger BigInt36 = new(36);

    // assumes the input 'chars' is in big-endian ordering, MSB->LSB
    public static byte[]? DecodeBase36String(this string chars)
    {
        var bi = new BigInteger();
        for (int x = 0; x < chars.Length; x++)
        {
            int i = Alphabet.IndexOf(chars[x]);
            if (i < 0)
            {
                return null; // invalid character
            }

            bi *= BigInt36;
            bi += i;
        }

        return bi.ToByteArray();
    }

    public static string ToBase36String(this long n)
    {
        byte[] bytes = BitConverter.GetBytes(n);
        return ToBase36String(bytes);
    }

    public static string ToBase36String(this byte[] bytes)
    {
        // Estimate the result's length so we don't waste time realloc'ing
        int resultLength = (int)
            Math.Ceiling(bytes.Length * 8 / Base36CharsLengthDivisor);

        // We use a List so we don't have to CopyTo a StringBuilder's characters
        // to a char[], only to then Array.Reverse it later
        var result = new List<char>(resultLength);

        var dividend = new BigInteger(bytes);
        // IsZero's computation is less complex than evaluating "dividend > 0"
        // which invokes BigInteger.CompareTo(BigInteger)
        while (!dividend.IsZero)
        {
            dividend = BigInteger.DivRem(dividend, BigInt36, out BigInteger remainder);
            int digit_index = Math.Abs((int)remainder);
            result.Add(Alphabet[digit_index]);
        }

        // orient the characters in big-endian ordering
        if (BitConverter.IsLittleEndian)
        {
            result.Reverse();
        }

        // ToArray will also trim the excess chars used in length prediction
        return new string([.. result]);
    }
}