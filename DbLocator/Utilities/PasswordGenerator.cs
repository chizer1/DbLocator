using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace DbLocator.Utilities;

internal static partial class PasswordGenerator
{
    private const string ValidChars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UpperCaseRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowerCaseRegex();

    [GeneratedRegex(@"[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[\W_]")]
    private static partial Regex SpecialCharRegex();

    internal static string GenerateRandomPassword(int length)
    {
        length = Math.Clamp(length, 8, 50);

        string password;
        do
        {
            password = GeneratePassword(length);
        } while (!IsValidPassword(password));

        return password;
    }

    private static string GeneratePassword(int length)
    {
        byte[] randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        StringBuilder result = new StringBuilder(length);
        foreach (byte b in randomBytes)
        {
            result.Append(ValidChars[b % ValidChars.Length]);
        }

        return result.ToString();
    }

    private static bool IsValidPassword(string password)
    {
        return UpperCaseRegex().IsMatch(password)
            && LowerCaseRegex().IsMatch(password)
            && DigitRegex().IsMatch(password)
            && SpecialCharRegex().IsMatch(password);
    }
}
