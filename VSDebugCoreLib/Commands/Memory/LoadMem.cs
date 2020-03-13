using System;
using System.Globalization;
using System.IO;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class LoadMem : BaseCommand
    {
        public LoadMem(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdLoadMemString)
        {
            CommandDescription = Resources.CmdLoadMemDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <srcfile> <address> <size>\n" +
                                "\tEX: " + CommandString + " c:\\memdata.bin 0x00656789 200\n" +
                                "\t<srcfile>  - source file\n" +
                                "\t<address>  - write address, must be a hex address / pointer, can be an expression\n" +
                                "\t<size>     - size in bytes, can be an expression\n";

            CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
        }

        public override ECommandStatus CommandStatus
        {
            get
            {
                if (null != Context.IDE.Debugger && null != Context.IDE.Debugger.DebuggedProcesses &&
                    Context.IDE.Debugger.DebuggedProcesses.Count > 0)
                {
                    if (DebugHelpers.IsMiniDumpProcess(Context.IDE.Debugger.CurrentProcess))
                        CommandStatusFlag = ECommandStatus.CommandStatusNaMiniDump;
                    else
                        CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
                }
                else
                {
                    CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
                }

                return CommandStatusFlag;
            }
        }

        public override void Execute(string[] args)
        {
            base.Execute(args);

            if (args.Length != 3)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            var strArgFile = args[0];
            var strArgDst = args[1];
            var strArgSize = args[2];

            // get file path
            var strPath = Path.GetDirectoryName(strArgFile);

            // then this was meant to be in the working directory
            if (strPath == string.Empty)
                strArgFile = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, strArgFile);

            if (!File.Exists(strArgFile))
            {
                Context.CONSOLE.Write("couldn`t open input file: " + strArgFile + " !");
                return;
            }

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            var processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgDst.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgDst + " is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long startAddress = 0;
            long dataSize = 0;
            var numStyleSource = NumberHelpers.IsHexNumber(varArgDst.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgDst.Value, numStyleSource, out startAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            var fileInfo = new FileInfo(strArgFile);

            if (fileInfo.Length < dataSize)
            {
                Context.CONSOLE.Write("Input file size:" + fileInfo.Length + " is less than the specified size:" +
                                      dataSize + " !");
                return;
            }

            var ntdbgStatus = NativeMethods.NtdbgOk;
            if (NativeMethods.NtdbgOk !=
                (ntdbgStatus = MemoryHelpers.LoadFileToMemory(strArgFile, processId, startAddress, dataSize)))
            {
                Context.CONSOLE.Write("Couldn`t load memory to address:" + "0x" + startAddress.ToString("X") + " !");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus + " - " + NativeMethods.GetStatusString(ntdbgStatus) +
                                      ".");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize + " bytes to address: " + "0x" + startAddress.ToString("X"));
        }
    }
}
