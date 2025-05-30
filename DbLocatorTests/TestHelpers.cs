using System.Net;

namespace DbLocatorTests;

public static class TestHelpers
{
    public static string GetRandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = new Random();
        var result = new char[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }
        return new string(result);
    }

    public static IPAddress GetRandomIpAddress()
    {
        var random = new Random();
        var bytes = new byte[4];
        random.NextBytes(bytes);
        bytes[0] = (byte)(bytes[0] & 0x7F);
        return new IPAddress(bytes);
    }

    public static string GetRandomIpAddressString()
    {
        var ipAddress = GetRandomIpAddress();
        return ipAddress.ToString();
    }
}
