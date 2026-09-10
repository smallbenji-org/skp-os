using System.Net;

namespace SKP.OS.Backend.Networking;

public static class SubnetMatcher
{
    public static bool IsInAnySubnet(IPAddress address, IEnumerable<string> cidrSubnets)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        foreach (var cidr in cidrSubnets)
        {
            var trimmed = cidr?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            if (IsInSubnet(address, trimmed))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsInSubnet(IPAddress address, string cidr)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        var slash = cidr.IndexOf('/');
        if (slash <= 0 || slash == cidr.Length - 1)
        {
            return false;
        }

        if (!IPAddress.TryParse(cidr[..slash], out var networkIp) ||
            !int.TryParse(cidr[(slash + 1)..], out var prefix))
        {
            return false;
        }

        var addressBytes = address.GetAddressBytes();
        var networkBytes = networkIp.GetAddressBytes();
        if (addressBytes.Length != networkBytes.Length)
        {
            return false;
        }

        var totalBits = addressBytes.Length * 8;
        if (prefix < 0 || prefix > totalBits)
        {
            return false;
        }

        var fullBytes = prefix / 8;
        for (var i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
            {
                return false;
            }
        }

        var remainingBits = prefix % 8;
        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(0xFF << (8 - remainingBits));
        return (addressBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }
}