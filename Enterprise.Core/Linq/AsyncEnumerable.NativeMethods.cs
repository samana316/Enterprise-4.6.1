using System;
using System.Runtime.InteropServices;

namespace Enterprise.Core.Linq
{
    partial class AsyncEnumerable
    {
        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        }
    }
}
