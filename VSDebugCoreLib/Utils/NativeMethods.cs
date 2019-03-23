using System;
using System.Runtime.InteropServices;

namespace VSDebugCoreLib.Utils
{
    internal class NativeMethods
    {
        public const uint PROCESS_QUERY_INFORMATION = (0x0400);
        public const uint PROCESS_VM_OPERATION = 0x0008;
        public const uint PROCESS_VM_READ = (0x0010);
        public const uint PROCESS_VM_WRITE = (0x0020);

        public const int NTDBG_OK = 0;
        public const int NTDBG_FAIL = -1;
        public const int NTDBG_INVALID_ADDRESS = -2;
        public const int NTDBG_ACCESS_DENIED = -3;
        public const int NTDBG_INVALID_SIZE = -4;

        public static string GetStatusString(int err)
        {
            switch (err)
            {
                case NTDBG_OK: return nameof(NTDBG_OK);
                case NTDBG_FAIL: return nameof(NTDBG_FAIL);
                case NTDBG_INVALID_ADDRESS: return nameof(NTDBG_INVALID_ADDRESS);
                case NTDBG_ACCESS_DENIED: return nameof(NTDBG_ACCESS_DENIED);
                case NTDBG_INVALID_SIZE: return nameof(NTDBG_INVALID_SIZE);
            }

            return "";
        }

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NtDbgOpenProcess(
              UInt32 dwDesiredAccess
            , Int32 bInheritHandle
            , UInt32 dwProcessId
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgCloseHandle(
                IntPtr hObject
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgReadProcessMemory(
                IntPtr hProcess
            , long lpBaseAddress
            , [In, Out] byte[] buffer
            , UInt32 size
            , out uint lpNumberOfBytesRead
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgWriteProcessMemory(
                IntPtr hProcess
            , long lpBaseAddress
            , [In] byte[] buffer
            , UInt32 nSize
            , out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgProcessMemCpy(
                IntPtr hSrcProcess
            , IntPtr hDstProcess
            , long lpSrcAddress
            , long lpDstAddress
            , UInt32 nSize
            , out uint lpNumberOfBytesCopied
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgProcessMemSet(
                IntPtr hProcess
            , long lpBaseAddress
            , UInt32 uByte
            , UInt32 nSize
            , out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 NtDbgProcessAlloc(
                IntPtr hProcess
            , UInt32 nSize
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 NtDbgProcessFree(
                IntPtr hProcess
            , UInt64 lpAddress
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }
    }
}