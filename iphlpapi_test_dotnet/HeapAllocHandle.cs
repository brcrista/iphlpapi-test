using System;
using Microsoft.Win32.SafeHandles;

namespace IphlpapiTest
{
    /// <summary>
    /// A safe handle for a handle whose object is allocated with <c>HeapAlloc</c>.
    /// </summary>
    public sealed class HeapAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private static readonly IntPtr processHeap = HeapApi.GetProcessHeap();

        public static IntPtr DangerousGetProcessHeap() => processHeap;

        public HeapAllocHandle()
            : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return HeapApi.HeapFree(processHeap, 0u, handle);
        }
    }
}
