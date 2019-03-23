using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.Debugger;

namespace VSDebugCoreLib.Utils
{
    public class MemoryHelpers
    {
        public static int TryReadProcessMemory(byte[] buffer, int processId, long startAddress, long size)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmRead | NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            uint nIOBytes = 0;

            var st = NativeMethods.NtDbgReadProcessMemory(handle
                , startAddress
                , buffer
                , (uint) Math.Min(size, buffer.Length)
                , out nIOBytes
            );

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static int LoadFileToMemory(string fileName, int processId, long fromAddress, long lengthToWrite)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmOperation | NativeMethods.ProcessVmWrite |
                NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            var st = NativeMethods.NtdbgOk;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var buffer = new byte[4096];
                int read;
                for (long i = 0; i < lengthToWrite && NativeMethods.NtdbgOk == st; i += read)
                {
                    read = fs.Read(buffer, 0, (int) Math.Min(lengthToWrite - i, buffer.Length));

                    uint nIOBytes = 0;
                    st = NativeMethods.NtDbgWriteProcessMemory(handle
                        , fromAddress + i
                        , buffer
                        , (uint) read
                        , out nIOBytes
                    );
                }
            }

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static bool LoadFileToMemory(string fileName, DkmProcess process, long fromAddress, long lengthToWrite)
        {
            var bRes = false;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var bufferSize = 4096;
                var buffer = new byte[bufferSize];
                int read;
                long i = 0;
                for (i = 0; i < lengthToWrite; i += read)
                {
                    read = fs.Read(buffer, 0, (int) Math.Min(lengthToWrite - i, buffer.Length));

                    if (read != bufferSize)
                    {
                        var tmp = new byte[read];
                        Buffer.BlockCopy(buffer, 0, tmp, 0, read);

                        process.WriteMemory((ulong) (fromAddress + i), tmp);
                    }
                    else
                    {
                        process.WriteMemory((ulong) (fromAddress + i), buffer);
                    }
                }

                if (i == lengthToWrite)
                    bRes = true;
            }

            return bRes;
        }

        public static int WriteMemoryToFile(string fileName, int processId, long fromAddress, long lengthToRead,
            FileMode fileMode = FileMode.Create)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmRead | NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            var st = NativeMethods.NtdbgOk;

            using (var fs = new FileStream(fileName, fileMode))
            {
                var buffer = new byte[4096];
                uint nIOBytes = 0;

                for (long i = 0; i < lengthToRead && NativeMethods.NtdbgOk == st; i += nIOBytes)
                {
                    st = NativeMethods.NtDbgReadProcessMemory(handle
                        , fromAddress + i
                        , buffer
                        , (uint) Math.Min(lengthToRead - i, buffer.Length)
                        , out nIOBytes
                    );

                    if (st == NativeMethods.NtdbgOk)
                        fs.Write(buffer, 0, (int) nIOBytes);
                }
            }

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static unsafe bool WriteMemoryToFile(string fileName, DkmProcess process, long fromAddress,
            long lengthToRead, FileMode fileMode = FileMode.Create)
        {
            var bRes = false;

            using (var fs = new FileStream(fileName, fileMode))
            {
                var buffer = new byte[4096];
                var nIOBytes = 0;

                long i = 0;

                for (i = 0; i < lengthToRead; i += nIOBytes)
                {
                    fixed (void* pBuffer = buffer)
                    {
                        nIOBytes = process.ReadMemory((ulong) (fromAddress + i), DkmReadMemoryFlags.None, pBuffer,
                            (int) Math.Min(lengthToRead - i, buffer.Length));
                    }

                    fs.Write(buffer, 0, nIOBytes);
                }

                if (i == lengthToRead)
                    bRes = true;
            }

            return bRes;
        }

        public static int ProcMemoryCopy(int processId, long dstAddress, long srcAddress, long length)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmOperation | NativeMethods.ProcessVmRead | NativeMethods.ProcessVmWrite |
                NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            uint nIOBytes = 0;

            var st = NativeMethods.NtDbgProcessMemCpy(handle
                , handle
                , srcAddress
                , dstAddress
                , (uint) length
                , out nIOBytes
            );

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static int ProcMemset(int processId, long dstAddress, byte val, long length)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmOperation | NativeMethods.ProcessVmRead | NativeMethods.ProcessVmWrite |
                NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            var st = NativeMethods.NtdbgOk;

            uint nIOBytes = 0;

            st = NativeMethods.NtDbgProcessMemSet(handle
                , dstAddress
                , val
                , (uint) length
                , out nIOBytes
            );

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static ulong ProcAlloc(int processId, long size)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmOperation | NativeMethods.ProcessVmRead | NativeMethods.ProcessVmWrite |
                NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            var dwRet = NativeMethods.NtDbgProcessAlloc(handle, (uint) size);

            NativeMethods.NtDbgCloseHandle(handle);

            return dwRet;
        }

        public static int ProcFree(int processId, long address)
        {
            var handle = NativeMethods.NtDbgOpenProcess(
                NativeMethods.ProcessVmOperation | NativeMethods.ProcessVmRead | NativeMethods.ProcessVmWrite |
                NativeMethods.ProcessQueryInformation, 0, (uint) processId);

            var st = NativeMethods.NtDbgProcessFree(handle, (ulong) address);

            NativeMethods.NtDbgCloseHandle(handle);

            return st;
        }

        public static MemoryStream SerializeToStream(object o)
        {
            var stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            var o = formatter.Deserialize(stream);
            return o;
        }

        public struct ByteData
        {
            public byte[] data;
            public ulong size;
        }
    }
}