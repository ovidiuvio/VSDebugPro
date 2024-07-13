using EnvDTE;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualStudio.Debugger;

namespace VSDebugCoreLib.Utils
{
    public class MemoryHelpers
    {
        private const int MAXIMUM_BLOCK_SIZE = (1 << 16);

        public static bool LoadFileToMemory(string fileName, StackFrame stackFrame, long fromAddress, long lengthToWrite)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                var readBuffer = new byte[Math.Min(lengthToWrite, MAXIMUM_BLOCK_SIZE)];

                while (lengthToWrite > 0)
                {
                    var readBytes = fs.Read(readBuffer, 0, Math.Min((int)lengthToWrite, readBuffer.Length));

                    if (0 == readBytes)
                    {
                        return false;
                    }

                    var writeBuffer = readBuffer;

                    if (readBytes != readBuffer.Length)
                    {
                        writeBuffer = new byte[readBytes];
                        Buffer.BlockCopy(readBuffer, 0, writeBuffer, 0, readBytes);
                    }

                    process.WriteMemory((ulong)fromAddress, writeBuffer);
                    fromAddress += writeBuffer.Length;
                    lengthToWrite -= writeBuffer.Length;
                }
            }

            return true;
        }

        public static bool WriteMemoryToFile(string fileName, StackFrame stackFrame, long fromAddress,
            long lengthToRead, FileMode fileMode = FileMode.Create)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);

            using (var fs = new FileStream(fileName, fileMode))
            {
                var buffer = new byte[Math.Min(lengthToRead, MAXIMUM_BLOCK_SIZE)];

                while (lengthToRead > 0)
                {
                    if (buffer.Length > lengthToRead)
                    {
                        Array.Resize(ref buffer, (int)lengthToRead);
                    }

                    var byteCount = process.ReadMemory((ulong)fromAddress, DkmReadMemoryFlags.None, buffer);
                    if (buffer.Length != byteCount)
                    {
                        return false;
                    }

                    fs.Write(buffer, 0, byteCount);
                    fromAddress += byteCount;
                    lengthToRead -= byteCount;
                }
            }

            return 0 == lengthToRead;
        }

        public static bool WriteHexDumpToFile(string fileName, StackFrame stackFrame, long fromAddress,
            long lengthToRead, int columns, int bytesPerRow, FileMode fileMode = FileMode.Create)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);
            var fs = new FileStream(fileName, fileMode);
            using (var writer = new StreamWriter(fs))
            {
                var buffer = new byte[bytesPerRow];
                long currentAddress = fromAddress;

                while (lengthToRead > 0)
                {
                    if (buffer.Length > lengthToRead)
                    {
                        Array.Resize(ref buffer, (int)lengthToRead);
                    }

                    var byteCount = process.ReadMemory((ulong)currentAddress, DkmReadMemoryFlags.None, buffer);
                    if (buffer.Length != byteCount)
                    {
                        return false;
                    }

                    writer.Write($"{currentAddress:X8}  ");

                    for (var i = 0; i < byteCount; i++)
                    {
                        writer.Write($"{buffer[i]:X2} ");
                        if ((i + 1) % columns == 0 && i != byteCount - 1)
                            writer.Write(" ");
                    }

                    // Pad with spaces if less than bytesPerRow bytes were read
                    for (var i = byteCount; i < buffer.Length; i++)
                    {
                        writer.Write("   ");
                        if ((i + 1) % columns == 0 && i != buffer.Length - 1)
                            writer.Write(" ");
                    }

                    writer.Write(" |");
                    for (var i = 0; i < byteCount; i++)
                    {
                        writer.Write(char.IsControl((char)buffer[i]) ? '.' : (char)buffer[i]);
                    }

                    writer.WriteLine("|");

                    currentAddress += byteCount;
                    lengthToRead -= byteCount;
                }
            }

            return true;
        }

        public static bool ProcMemoryCopy(StackFrame stackFrame, long dstAddress, long srcAddress, long length)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);
            var buffer = new byte[Math.Min(length, MAXIMUM_BLOCK_SIZE)];

            while (length > 0)
            {
                if (buffer.Length > length)
                {
                    Array.Resize(ref buffer, (int)length);
                }

                var byteCount = process.ReadMemory((ulong)srcAddress, DkmReadMemoryFlags.None, buffer);
                if (buffer.Length != byteCount)
                {
                    return false;
                }

                process.WriteMemory((ulong)dstAddress, buffer);

                srcAddress += byteCount;
                dstAddress += byteCount;
                length -= byteCount;
            }

            return true;
        }

        public static bool ProcMemset(StackFrame stackFrame, long dstAddress, byte val, long length)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);
            var buffer = new byte[Math.Min(length, MAXIMUM_BLOCK_SIZE)];
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = val;
            }

            while (length > 0)
            {
                if (buffer.Length > length)
                {
                    Array.Resize(ref buffer, (int)length);
                }
                process.WriteMemory((ulong)dstAddress, buffer);
                length -= buffer.Length;
                dstAddress += buffer.Length;
            }

            return 0 == length;
        }

        public static ulong ProcAlloc(StackFrame stackFrame, long size)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);

            return process.AllocateVirtualMemory(0,
                (int)size,
                0x3000, // MEM_COMMIT | MEM_RESERVE
                0x04    // PAGE_READWRITE
            );
        }

        public static void ProcFree(StackFrame stackFrame, long address)
        {
            var process = DkmMethods.GetDkmProcess(stackFrame);

            process.FreeVirtualMemory(
                (ulong)address,
                0,
                0x8000  // MEM_RELEASE 
            );
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
