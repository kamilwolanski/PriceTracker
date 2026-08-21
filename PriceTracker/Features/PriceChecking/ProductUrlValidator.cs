using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace PriceTracker.Features.PriceChecking
{
    public static class ProductUrlValidator
    {
        public static bool IsValid(
            string url,
            [NotNullWhen(true)] out Uri? uri)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
            {
                uri = null;
                return false;
            }

            if (parsedUri.Scheme != Uri.UriSchemeHttp &&
                parsedUri.Scheme != Uri.UriSchemeHttps)
            {
                uri = null;
                return false;
            }

            if (IsBlockedHost(parsedUri.Host))
            {
                uri = null;
                return false;
            }

            uri = parsedUri;
            return true;
        }

        private static bool IsBlockedHost(string host)
        {
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!IPAddress.TryParse(host, out var ipAddress))
                return false;

            if (IPAddress.IsLoopback(ipAddress))
                return true;

            if (ipAddress.Equals(IPAddress.Any) ||
                ipAddress.Equals(IPAddress.IPv6Any) ||
                ipAddress.Equals(IPAddress.None))
            {
                return true;
            }

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                return IsBlockedIPv4(ipAddress);

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                return IsBlockedIPv6(ipAddress);

            return false;
        }

        private static bool IsBlockedIPv4(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();

            return bytes[0] == 10 ||
                bytes[0] == 127 ||
                bytes[0] == 169 && bytes[1] == 254 ||
                bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 ||
                bytes[0] == 192 && bytes[1] == 168;
        }

        private static bool IsBlockedIPv6(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();

            return bytes[0] == 0xfc ||
                bytes[0] == 0xfd ||
                bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80;
        }
    }
}
