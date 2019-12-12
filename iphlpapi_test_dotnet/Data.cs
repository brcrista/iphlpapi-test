using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Iphlpapi
{
    /// <summary>
    /// Defines the possible TCP offload states for a TCP connection.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/windows/win32/api/tcpmib/ne-tcpmib-tcp_connection_offload_state.
    /// </remarks>
    public enum TcpConnectionOffloadState
    {
        InHost,
        Offloading,
        Offloaded,
        Uploading,
        Max
    }

    /// <summary>
    /// Error codes defined in <c>winerror.h</c>.
    /// </summary>
    public static class StatusCode
    {
        public const uint NoError = 0;
        public const uint ErrorNotSupported = 50;
        public const uint ErrorInvalidParameter = 87;
        public const uint ErrorInsufficientBuffer = 122;
    }

    /// <summary>
    /// Contains information that describes an IPv4 TCP connection.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/windows/win32/api/tcpmib/ns-tcpmib-mib_tcprow2.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct TcpRow2
    {
        public TcpState state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;
        public TcpConnectionOffloadState offloadState;
    }
}
