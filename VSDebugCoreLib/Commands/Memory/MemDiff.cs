using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemDiff : BaseCommand
    {
        public MemDiff(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdMemDiffString)
        {
            CommandDescription = Resources.CmdMemDiffDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <addr1> <addr2> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0x00656789 200\n" +
                                "\t<addr1>  - data source 1\n" +
                                "\t<addr2>  - data source 2\n" +
                                "\t<size>   - size in bytes\n";

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

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = {' ', '\t'};
            var argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 3)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            var strArgAddr1 = argv[0];
            var strArgAddr2 = argv[1];
            var strArgSize = argv[2];

            var varArgAddr1 = Context.IDE.Debugger.GetExpression(strArgAddr1, false, 100);
            var varArgAddr2 = Context.IDE.Debugger.GetExpression(strArgAddr2, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

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
            var numStyleAddr1 = NumberHelpers.IsHexNumber(varArgAddr1.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleAddr2 = NumberHelpers.IsHexNumber(varArgAddr2.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr1.Value, numStyleAddr1, out addr1);
            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr2.Value, numStyleAddr2, out addr2);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            // get file path
            var strFile1 = Path.GetTempFileName();
            var strFile2 = Path.GetTempFileName();
            var strDiffTool = Context.Settings.GeneralSettings.DiffTool;

            if (string.Empty == strDiffTool)
            {
                Context.CONSOLE.Write("Diff tool not set!");
                return;
            }

            if (!MemoryHelpers.WriteMemoryToFile(strFile1,
                     Context.IDE.Debugger.CurrentStackFrame,
                     addr1,
                     dataSize
                 ))
            {
                Context.CONSOLE.Write("Failed to read data from address: " + NumberHelpers.ToHex(addr1) + "!");
                return;
            }

            if (!MemoryHelpers.WriteMemoryToFile(strFile2,
                    Context.IDE.Debugger.CurrentStackFrame,
                    addr2,
                    dataSize
                ))
            {
                Context.CONSOLE.Write("Failed to read data from address: " + NumberHelpers.ToHex(addr2) + "!");
                return;
            }

            Process.Start(strDiffTool, strFile1 + " " + strFile2);
        }
    }
}
