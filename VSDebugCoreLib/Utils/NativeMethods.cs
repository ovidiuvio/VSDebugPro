using System;
using System.Runtime.InteropServices;

namespace VSDebugCoreLib.Utils
{
    internal class NativeMethods
    {
        public const uint ProcessQueryInformation = 0x0400;
        public const uint ProcessVmOperation = 0x0008;
        public const uint ProcessVmRead = 0x0010;
        public const uint ProcessVmWrite = 0x0020;

        public const int NtdbgOk = 0;
        public const int NtdbgFail = -1;
        public const int NtdbgInvalidAddress = -2;
        public const int NtdbgAccessDenied = -3;
        public const int NtdbgInvalidSize = -4;

        public static string GetStatusString(int err)
        {
            switch (err)
            {
                case NtdbgOk: return nameof(NtdbgOk);
                case NtdbgFail: return nameof(NtdbgFail);
                case NtdbgInvalidAddress: return nameof(NtdbgInvalidAddress);
                case NtdbgAccessDenied: return nameof(NtdbgAccessDenied);
                case NtdbgInvalidSize: return nameof(NtdbgInvalidSize);
            }

            return "";
        }

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NtDbgOpenProcess(
            uint dwDesiredAccess
            , int bInheritHandle
            , uint dwProcessId
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgCloseHandle(
            IntPtr hObject
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgReadProcessMemory(
            IntPtr hProcess
            , long lpBaseAddress
            , [In] [Out] byte[] buffer
            , uint size
            , out uint lpNumberOfBytesRead
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgWriteProcessMemory(
            IntPtr hProcess
            , long lpBaseAddress
            , [In] byte[] buffer
            , uint nSize
            , out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgProcessMemCpy(
            IntPtr hSrcProcess
            , IntPtr hDstProcess
            , long lpSrcAddress
            , long lpDstAddress
            , uint nSize
            , out uint lpNumberOfBytesCopied
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgProcessMemSet(
            IntPtr hProcess
            , long lpBaseAddress
            , uint uByte
            , uint nSize
            , out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong NtDbgProcessAlloc(
            IntPtr hProcess
            , uint nSize
        );

        [DllImport("libntdbg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NtDbgProcessFree(
            IntPtr hProcess
            , ulong lpAddress
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct MemoryBasicInformation
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