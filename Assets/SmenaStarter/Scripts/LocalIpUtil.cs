using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SmenaStarter
{
    public static class LocalIpUtil
    {
        public const int Port = 7777;

        public static string GetLanIPv4()
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                foreach (var info in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (info.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    var ip = info.Address;
                    if (IPAddress.IsLoopback(ip))
                        continue;
                    var bytes = ip.GetAddressBytes();
                    if (bytes[0] == 169 && bytes[1] == 254)
                        continue;
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }
    }
}
