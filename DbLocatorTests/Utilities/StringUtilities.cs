namespace DbLocatorTests.Utilities;

public static class StringUtilities
{
    public static string RandomString(int length)
    {
        var rand = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(
            [.. Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)])]
        );
    }
}
