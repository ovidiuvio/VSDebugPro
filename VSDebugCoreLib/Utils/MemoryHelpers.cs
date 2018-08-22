using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.Debugger;

namespace VSDebugCoreLib.Utils
{
    public class MemoryHelpers 
    {
        public struct ByteData
        {
            public byte[] data;
            public UInt64 size;
        }

        public static int TryReadProcessMemory(byte[] buffer, int processId, long startAddress, long size)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_READ|NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            int st      = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

           
            st = NativeMethods.NtDbgReadProcessMemory(handle
                    , startAddress
                    , buffer
                    , (uint)Math.Min(size, buffer.Length)
                    , out nIOBytes
                );            

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static int LoadFileToMemory(string fileName, int processId, long fromAddress, long lengthToWrite)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

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

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static bool LoadFileToMemory(string fileName, DkmProcess process, long fromAddress, long lengthToWrite)
        {
            bool bRes = false;

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                int bufferSize = 4096;
                byte[] buffer = new byte[bufferSize];
                int read;
                long i = 0;
                for (i = 0; (i < lengthToWrite); i += read)
                {
                    read = fs.Read(buffer, 0, (int)Math.Min(lengthToWrite - i, buffer.Length));

                    if(read != bufferSize)
                    {
                        byte[] tmp = new byte[read];
                        Buffer.BlockCopy(buffer, 0, tmp, 0, read);

                        process.WriteMemory((ulong)(fromAddress + i), tmp);
                    }
                    else
                        process.WriteMemory((ulong)(fromAddress + i), buffer);
                }

                if (i == lengthToWrite)
                    bRes = true;
            }

            return bRes;
        }

        public static int WriteMemoryToFile(string fileName, int processId, long fromAddress, long lengthToRead, FileMode fileMode = FileMode.Create) 
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

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

            return st;
        }

        public static unsafe bool WriteMemoryToFile(string fileName, DkmProcess process, long fromAddress, long lengthToRead, FileMode fileMode = FileMode.Create)
        {
            bool bRes = false;

            using (FileStream fs = new FileStream(fileName, fileMode))
            {
                byte[] buffer = new byte[4096];
                int nIOBytes = 0;

                long i = 0;

                for (i = 0; (i < lengthToRead); i += nIOBytes)
                {
                    fixed (void* pBuffer = buffer)
                    {
                        nIOBytes = process.ReadMemory((ulong)(fromAddress + i), DkmReadMemoryFlags.None, pBuffer, (int)Math.Min(lengthToRead - i, buffer.Length));
                    }

                    fs.Write(buffer, 0, nIOBytes);
                }

                if (i == lengthToRead)
                    bRes = true;
            }

            return bRes;
        }

        public static int ProcMemoryCopy( int processId, long dstAddress, long srcAddress, long length )
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            int  st     = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

            st  = NativeMethods.NtDbgProcessMemCpy( handle
                ,   handle
                ,   srcAddress
                ,   dstAddress
                ,   (UInt32)length
                ,   out nIOBytes
                );

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static int ProcMemset(int processId, long dstAddress, byte val, long length)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            int st = NativeMethods.NTDBG_OK;

            uint nIOBytes = 0;

            st = NativeMethods.NtDbgProcessMemSet(handle
                    , dstAddress
                    , val
                    , (UInt32)length
                    , out nIOBytes
                );

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static UInt64 ProcAlloc(int processId, long size)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            UInt64 dwRet = 0;

            dwRet = NativeMethods.NtDbgProcessAlloc(handle, (UInt32)size);

            NativeMethods.NtDbgCloseHandle(handle);

            return dwRet;
        }

        public static int ProcFree(int processId, long address)
        {
            IntPtr handle = NativeMethods.NtDbgOpenProcess(NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE | NativeMethods.PROCESS_QUERY_INFORMATION, 0, (uint)processId);

            int st = NativeMethods.NTDBG_OK;

            st = NativeMethods.NtDbgProcessFree(handle, (UInt64)address);

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
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
