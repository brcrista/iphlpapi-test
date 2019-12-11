using System;
using System.Runtime.InteropServices;

namespace Iphlpapi
{
    /// <summary>
    /// Methods exposed by iphlpapi.dll.
    /// </summary>
    static class Iphlpapi
    {
        /// <summary>
        /// Retrieves the IPv4 TCP connection table.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/windows/win32/api/iphlpapi/nf-iphlpapi-gettcptable2.
        /// </remarks>
        [DllImport("iphlpapi.dll")]
        public static extern ulong GetTcpTable2(IntPtr tcpTable, ref ulong sizePointer, bool order); // TODO: ref MIB_TCPTABLE2?
    }

    static class StatusCode
    {
        public const ulong NO_ERROR = 0;
        public const ulong ERROR_NOT_SUPPORTED = 50;
        public const ulong ERROR_INVALID_PARAMETER = 87;
        public const ulong ERROR_INSUFFICIENT_BUFFER = 122;
    }

    static class Program
    {
        static unsafe int Main(string[] args)
        {
            var size = GetBufferSize();
            var buffer = new byte[size];

            fixed (byte* pBuffer = buffer)
            {
                ulong status = Iphlpapi.GetTcpTable2((IntPtr)pBuffer, ref size, order: true);

                switch (status)
                {
                    case StatusCode.NO_ERROR:
                        Console.WriteLine($"Returned {buffer[0]} connections");
                        return 0;
                    case StatusCode.ERROR_INSUFFICIENT_BUFFER:
                        Console.Error.WriteLine($"Insufficient buffer: required {size} bytes.");
                        return 1;
                    case StatusCode.ERROR_INVALID_PARAMETER:
                        Console.Error.WriteLine("Could not write to the buffer. Is it null?");
                        return 1;
                    case StatusCode.ERROR_NOT_SUPPORTED:
                        Console.Error.WriteLine("Not supported on this OS.");
                        return 1;
                    default:
                        Console.Error.WriteLine("An unknown error occurred.");
                        return 1;
                }
            }
        }

        private static unsafe ulong GetBufferSize()
        {
            var buffer = new byte[0];
            ulong size = 0;

            fixed (byte* pBuffer = buffer)
            {
                // Call for the size
                Iphlpapi.GetTcpTable2((IntPtr)pBuffer, ref size, order: true);
                return size;
            }
        }
    }
}
