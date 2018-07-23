using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace VSDebugCoreLib.Utils
{
    public class MemoryHelpers 
    {
        public struct ByteData
        {
            public byte[] data;
            public UInt64 size;
        }

        public static bool TryReadProcessMemory(byte[] buffer, int processId, long startAddress, long size)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_READ|NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet   = false;
            int st      = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

           
            st = NativeMethods.NtDbgReadProcessMemory(handle
                    , startAddress
                    , buffer
                    , (uint)Math.Min(size, buffer.Length)
                    , out nIOBytes
                );            

            NativeMethods.NtDbgCloseHandle(handle);

            if (st == NativeMethods.NTDBG_OK)
                bRet = true;

            return bRet;
        }

        public static bool LoadFileToMemory(string fileName, int processId, long fromAddress, long lengthToWrite)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet = true;
            int  st   = NativeMethods.NTDBG_OK;

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                byte[] buffer = new byte[4096];
                int read;
                for (long i = 0; (i < lengthToWrite) && (NativeMethods.NTDBG_OK == st); i += read)
                {
                    read = fs.Read(buffer, 0, (int)Math.Min(lengthToWrite - i, buffer.Length));

                    uint nIOBytes = 0;
                    st = NativeMethods.NtDbgWriteProcessMemory(handle
                        , fromAddress + i
                        , buffer
                        , (uint)read
                        , out nIOBytes
                    );
                }
            }

            if (NativeMethods.NTDBG_OK != st)
                bRet = false;

            NativeMethods.NtDbgCloseHandle(handle);

            return bRet;
        }

        public static bool WriteMemoryToFile(string fileName, int processId, long fromAddress, long lengthToRead, FileMode fileMode = FileMode.Create) 
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet   = false;
            int st      = NativeMethods.NTDBG_OK;

            using (FileStream fs = new FileStream(fileName, fileMode)) 
            {
                byte[] buffer = new byte[4096];
                uint   nIOBytes = 0;


                for (long i = 0; (i < lengthToRead) && (NativeMethods.NTDBG_OK == st); i += nIOBytes) 
                {
                    st = NativeMethods.NtDbgReadProcessMemory(handle
                        , fromAddress + i
                        , buffer
                        , (UInt32)Math.Min(lengthToRead - i, buffer.Length)
                        , out nIOBytes
                    );

                    if (st == NativeMethods.NTDBG_OK)
                        fs.Write(buffer, 0, (int)nIOBytes);
                }

            }

            NativeMethods.NtDbgCloseHandle(handle);

            if (st == NativeMethods.NTDBG_OK)
                bRet = true;

            return bRet;
        }

        public static bool ProcMemoryCopy( int processId, long dstAddress, long srcAddress, long length )
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet   = false;
            int  st     = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

            st  = NativeMethods.NtDbgProcessMemCpy( handle
                ,   handle
                ,   srcAddress
                ,   dstAddress
                ,   (UInt32)length
                ,   out nIOBytes
                );

            if (st == NativeMethods.NTDBG_OK)
                bRet = true;

            NativeMethods.NtDbgCloseHandle(handle);

            return bRet;
        }

        public static bool ProcMemset(int processId, long dstAddress, byte val, long length)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet = false;
            int st = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

            st = NativeMethods.NtDbgProcessMemSet(handle
                    , dstAddress
                    , val
                    , (UInt32)length
                    , out nIOBytes
                );

            if (st == NativeMethods.NTDBG_OK)
                bRet = true;

            NativeMethods.NtDbgCloseHandle(handle);

            return bRet;
        }

        public static UInt64 ProcAlloc(int processId, long size)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            UInt64 dwRet = 0;

            dwRet = NativeMethods.NtDbgProcessAlloc(handle, (UInt32)size);

            NativeMethods.NtDbgCloseHandle(handle);

            return dwRet;
        }

        public static bool ProcFree(int processId, long address)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            bool bRet = false;
            int st = NativeMethods.NTDBG_OK;

            st = NativeMethods.NtDbgProcessFree(handle, (UInt64)address);

            if (st == NativeMethods.NTDBG_OK)
                bRet = true;

            NativeMethods.NtDbgCloseHandle(handle);

            return bRet;
        }

        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }
    }
}
