using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VSDebugCoreLib.Utils
{
    class NativeMethods {
        public const uint PROCESS_QUERY_INFORMATION = (0x0400);
        public const uint PROCESS_VM_OPERATION      = 0x0008;
        public const uint PROCESS_VM_READ           = (0x0010);
        public const uint PROCESS_VM_WRITE          = (0x0020);

        public const int  NTDBG_OK					= 0; 
        public const int  NTDBG_FAIL				= -1;
        public const int  NTDBG_INVALID_ADDRESS		= -2;
        public const int  NTDBG_ACCESS_DENIED       = -3;
        public const int  NTDBG_INVALID_SIZE        = -4;

        public static string GetStatusString(int err)
        {
            switch(err)
            {
                case NTDBG_OK: return nameof(NTDBG_OK);
                case NTDBG_FAIL: return nameof(NTDBG_FAIL);
                case NTDBG_INVALID_ADDRESS: return nameof(NTDBG_INVALID_ADDRESS);
                case NTDBG_ACCESS_DENIED: return nameof(NTDBG_ACCESS_DENIED);
                case NTDBG_INVALID_SIZE: return nameof(NTDBG_INVALID_SIZE);
            }

            return "";
        }

        [DllImport("libntdbg.dll")]
        public static extern IntPtr NtDbgOpenProcess(
              UInt32 dwDesiredAccess
            , Int32  bInheritHandle
            , UInt32 dwProcessId
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgCloseHandle(
                IntPtr hObject
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgReadProcessMemory(
                IntPtr hProcess
            ,   long   lpBaseAddress
            ,   [In, Out] byte[] buffer
            ,   UInt32 size
            ,   out uint lpNumberOfBytesRead
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgWriteProcessMemory(
                IntPtr hProcess
            ,   long lpBaseAddress
            ,   [In] byte[] buffer
            ,   UInt32 nSize
            ,   out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgProcessMemCpy(
                IntPtr hSrcProcess
            ,   IntPtr hDstProcess
            ,   long   lpSrcAddress
            ,   long   lpDstAddress
            ,   UInt32 nSize
            ,   out uint lpNumberOfBytesCopied
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgProcessMemSet(
                IntPtr hProcess
            ,   long lpBaseAddress
            ,   UInt32 uByte
            ,   UInt32 nSize
            ,   out uint lpNumberOfBytesWritten
        );

        [DllImport("libntdbg.dll")]
        public static extern UInt64 NtDbgProcessAlloc(
                IntPtr hProcess
            ,   UInt32 nSize
        );

        [DllImport("libntdbg.dll")]
        public static extern Int32 NtDbgProcessFree(
                IntPtr hProcess
            ,   UInt64 lpAddress
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        // Imaging API
        public const int CVT_OK                  =  0;
        public const int CVT_FAIL                = -1;
        public const int CVT_INVALID_ARG         = -2;
        public const int CVT_NULLPTR             = -3;
        public const int CVT_NOT_SUPPORTED       = -4;

        [StructLayout(LayoutKind.Sequential)]
        public struct cvtColorPlane
        {
            public IntPtr data;
            public UInt32 stride;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct cvtColorImage
        {
            public Int32 width;
            public Int32 height;
            public UInt32 format;
            public cvtColorPlane p0;
            public cvtColorPlane p1;
            public cvtColorPlane p2;
            public cvtColorPlane p3;
        }

        [DllImport("libCvtColor.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 cvtColor(
                ref cvtColorImage src
            ,   ref cvtColorImage dst
        );

        [DllImport("libCvtColor.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 cvtGetImageSize(
                ref cvtColorImage img
            ,   out UInt64 size
        );

        [DllImport("libCvtColor.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 cvtGetPlaneSize(
                ref cvtColorImage img
            ,   UInt32 plane
            ,   out UInt64 size
        );

        [DllImport("libCvtColor.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int32 cvtGetImagePlanes(
                UInt32 format
            ,   out UInt32 numPlanes
        );
    }

}
