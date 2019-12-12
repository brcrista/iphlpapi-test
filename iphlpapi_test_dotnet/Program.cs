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
        public static extern uint GetTcpTable2(IntPtr tcpTable, ref uint sizePointer, bool order); // TODO: ref MIB_TCPTABLE2?
    }

    static class Program
    {
        static unsafe int Main(string[] args)
        {
            uint status;
            uint size = 0;

            // Call for the size
            status = Iphlpapi.GetTcpTable2(IntPtr.Zero, ref size, order: true);

            fixed (byte* buffer = new byte[size])
            {
                var bufferIntPtr = (IntPtr)buffer;
                status = Iphlpapi.GetTcpTable2(bufferIntPtr, ref size, order: true);

                switch (status)
                {
                    case StatusCode.NoError:
                        var numConnections = *(uint*)bufferIntPtr;
                        Console.WriteLine($"Found {numConnections} connections");

                        bufferIntPtr += sizeof(uint);
                        for (int i = 0; i < numConnections; i++)
                        {
                            var row = Marshal.PtrToStructure<TcpRow2>(bufferIntPtr);
                            Console.WriteLine(row.ToString());
                            bufferIntPtr += sizeof(TcpRow2);
                        }
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
