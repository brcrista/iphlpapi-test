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
        public static extern int GetTcpTable2(IntPtr tcpTable, ref int sizePointer, bool order); // TODO: ref MIB_TCPTABLE2?
    }

    static class StatusCode
    {
        public const int NoError = 0;
        public const int ErrorNotSupported = 50;
        public const int ErrorInvalidParameter = 87;
        public const int ErrorInsufficientBuffer = 122;
    }

    static class Program
    {
        static unsafe int Main(string[] args)
        {
            int status;
            int size = 0;

            // Call for the size
            status = Iphlpapi.GetTcpTable2(IntPtr.Zero, ref size, order: true);

            fixed (byte* buffer = new byte[size])
            {
                status = Iphlpapi.GetTcpTable2((IntPtr)buffer, ref size, order: true);

                switch (status)
                {
                    case StatusCode.NoError:
                        Console.WriteLine($"Returned {*(int*)buffer} connections");
                        return 0;
                    case StatusCode.ErrorInsufficientBuffer:
                        Console.Error.WriteLine($"Insufficient buffer: required {size} bytes.");
                        return 1;
                    case StatusCode.ErrorInvalidParameter:
                        Console.Error.WriteLine("Could not write to the buffer. Is it null?");
                        return 1;
                    case StatusCode.ErrorNotSupported:
                        Console.Error.WriteLine("Not supported on this OS.");
                        return 1;
                    default:
                        Console.Error.WriteLine("An unknown error occurred.");
                        return 1;
                }
            }
        }
    }
}
