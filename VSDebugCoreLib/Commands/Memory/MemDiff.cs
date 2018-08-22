using System;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class MemDiff : BaseCommand
    {
        public MemDiff(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdMemDiffString)
        {
            CommandDescription = Resources.CmdMemDiffDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <addr1> <addr2> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0x00656789 200\n" +
                                "\t<addr1>  - data source 1\n" +
                                "\t<addr2>  - data source 2\n" +
                                "\t<size>   - size in bytes\n";

            CommandStatusFlag = eCommandStatus.CommandStatus_Disabled;
        }

        public override eCommandStatus CommandStatus
        {
            get
            {
                if (null != Context.IDE.Debugger && null != Context.IDE.Debugger.DebuggedProcesses && Context.IDE.Debugger.DebuggedProcesses.Count > 0)
                {
                    if (DebugHelpers.IsMiniDumpProcess(Context.IDE.Debugger.CurrentProcess))
                        CommandStatusFlag = eCommandStatus.CommandStatus_NA_MiniDump;
                    else
                        CommandStatusFlag = eCommandStatus.CommandStatus_Enabled;
                }
                else
                {
                    CommandStatusFlag = eCommandStatus.CommandStatus_Disabled;
                }

                return CommandStatusFlag;
            }
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = new char[] { ' ', '\t' };
            string[] argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 3)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            string strArgAddr1 = argv[0];
            string strArgAddr2 = argv[1];
            string strArgSize  = argv[2];


            var varArgAddr1 = Context.IDE.Debugger.GetExpression(strArgAddr1, false, 100);
            var varArgAddr2 = Context.IDE.Debugger.GetExpression(strArgAddr2, false, 100);
            var varArgSize  = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            int processId   = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgAddr1.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgAddr1 + " is invalid!");
                return;
            }

            if (!varArgAddr2.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgAddr2 + " is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long addr1 = 0;
            long addr2 = 0;
            long dataSize = 0;
            NumberStyles numStyleAddr1 = NumberHelpers.IsHexNumber(varArgAddr1.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleAddr2 = NumberHelpers.IsHexNumber(varArgAddr2.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet = true;


            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr1.Value, numStyleAddr1, out addr1);
            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr2.Value, numStyleAddr2, out addr2);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            // get file path
            string strPath      = Path.GetTempPath();
            string strFile1     = Path.GetTempFileName();
            string strFile2     = Path.GetTempFileName();
            string strDiffTool  = Context.Settings.GeneralSettings.DiffTool;

            if (string.Empty == strDiffTool)
            {
                Context.CONSOLE.Write("Diff tool not set!");
                return;
            }

            int ntdbgStatus = NativeMethods.NTDBG_OK;
            if (NativeMethods.NTDBG_OK != (ntdbgStatus = MemoryHelpers.WriteMemoryToFile(strFile1, processId, addr1, dataSize)))
            {
                Context.CONSOLE.Write("Failed to read data from address: " + NumberHelpers.ToHex(addr1) + "!");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus.ToString() + " - " + NativeMethods.GetStatusString(ntdbgStatus) + ".");
                return;
            }

            if (NativeMethods.NTDBG_OK != (ntdbgStatus = MemoryHelpers.WriteMemoryToFile(strFile2, processId, addr2, dataSize)))
            {
                Context.CONSOLE.Write("Failed to read data from address: " + NumberHelpers.ToHex(addr2) + "!");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus.ToString() + " - " + NativeMethods.GetStatusString(ntdbgStatus) + ".");
                return;
            }

            Process.Start(strDiffTool, strFile1 + " " + strFile2);
            return;
            

        }
    }
}
