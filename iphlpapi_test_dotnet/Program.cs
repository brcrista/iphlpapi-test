using System;
using System.Runtime.InteropServices;

namespace IphlpapiTest
{
    /// <summary>
    /// Methods from the Win32 heap API.
    /// </summary>
    static class HeapApi
    {
        /// <summary>
        /// Retrieves a handle to the default heap of the calling process.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/windows/win32/api/heapapi/nf-heapapi-getprocessheap.
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        /// <summary>
        /// Frees a memory block allocated from a heap.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/windows/win32/api/heapapi/nf-heapapi-heapfree.
        /// </remarks>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr heap, uint flags, IntPtr memoryBlock);
    }

    /// <summary>
    /// Methods exported by iphlpapi.dll.
    /// </summary>
    static class IphlpApi
    {
        /// <summary>
        /// Retrieves the IPv4 TCP connection table.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/windows/win32/api/iphlpapi/nf-iphlpapi-gettcptable2.
        /// </remarks>
        [DllImport("iphlpapi.dll")]
        public static extern uint GetTcpTable2(IntPtr tcpTable, ref uint sizePointer, bool order); // TODO: ref MIB_TCPTABLE2?

        /// <summary>
        /// Retrieves the table of bound ports.
        /// </summary>
        [DllImport("iphlpapi.dll")]
        public static extern uint InternalGetBoundTcpEndpointTable(ref HeapAllocHandle tcpTable, IntPtr heapHandle, uint flags);
    }

    static class Program
    {
        static unsafe int Main(string[] args)
        {
            uint status;

            // Print the table of connections
            // Call for the size
            uint size = 0;
            status = IphlpApi.GetTcpTable2(IntPtr.Zero, ref size, order: true);

            fixed (byte* buffer = new byte[size])
            {
                var bufferIntPtr = (IntPtr)buffer;
                status = IphlpApi.GetTcpTable2(bufferIntPtr, ref size, order: true);

                switch (status)
                {
                    case StatusCode.NoError:
                        var numConnections = *(uint*)bufferIntPtr;
                        Console.WriteLine($"Found {numConnections} connections:");

                        bufferIntPtr += sizeof(uint);
                        for (int i = 0; i < numConnections; i++)
                        {
                            var row = Marshal.PtrToStructure<TcpRow2>(bufferIntPtr);
                            Console.WriteLine(FormatTcpConnectionInfo(row.localAddr, row.localPort, row.remoteAddr, row.remotePort, row.state.ToString()));
                            bufferIntPtr += sizeof(TcpRow2);
                        }
                        break;
                    case StatusCode.ErrorInsufficientBuffer:
                        Console.Error.WriteLine($"Insufficient buffer: required {size} bytes.");
                        break;
                    case StatusCode.ErrorInvalidParameter:
                        Console.Error.WriteLine("Could not write to the buffer. Is it null?");
                        break;
                    case StatusCode.ErrorNotSupported:
                        Console.Error.WriteLine("Not supported on this OS.");
                        break;
                    default:
                        Console.Error.WriteLine("An unknown error occurred.");
                        break;
                }
            }

            if (status != StatusCode.NoError)
            {
                return 1;
            }

            // Print the table of bound ports
            var boundPortTable = new HeapAllocHandle();
            status = IphlpApi.InternalGetBoundTcpEndpointTable(ref boundPortTable, HeapAllocHandle.DangerousGetProcessHeap(), flags: 0);

            if (status == StatusCode.NoError)
            {
                var numConnections = *(uint*)boundPortTable.DangerousGetHandle();
                Console.WriteLine($"Found {numConnections} bound ports:");

                // We need to keep a pointer to the start of the memory block so we can free it at the end
                var currentRow = boundPortTable.DangerousGetHandle();
                currentRow += sizeof(uint);
                for (int i = 0; i < numConnections; i++)
                {
                    var row = Marshal.PtrToStructure<TcpRow2>(currentRow);
                    Console.WriteLine(FormatTcpConnectionInfo(row.localAddr, row.localPort, row.remoteAddr, row.remotePort, "BOUND"));
                    currentRow += sizeof(TcpRow2);
                }
            }
            else
            {
                Console.Error.WriteLine("An unknown error occurred.");
            }

            if (status != StatusCode.NoError)
            {
                return 1;
            }

            return 0;
        }

        private static string FormatTcpConnectionInfo(uint localAddr, uint localPort, uint remoteAddr, uint remotePort, string state)
            => $"Local address: {localAddr}:{localPort} Remote address: {remoteAddr}:{remotePort} State: {state}";
    }
}
