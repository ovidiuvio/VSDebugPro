using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemAlloc : BaseCommand
    {
        public MemAlloc(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdMemAllocString)
        {
            CommandDescription = Resources.CmdMemAllocDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <size>\n" +
                                "\tEX: " + CommandString + " 200\n" +
                                "\t<size> - size in bytes\n";

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

            if (args.Length != 1)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            var strArgSize = args[0];

            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            var processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long dataSize = 0;

            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            ulong qwPtr = 0;
            qwPtr = MemoryHelpers.ProcAlloc(processId, dataSize);

            if (0 == qwPtr)
            {
                Context.CONSOLE.Write("Failed to allocate memory!");
                return;
            }

            Context.CONSOLE.Write("Allocated: " + dataSize + " bytes at address: " + NumberHelpers.ToHex((long) qwPtr));
        }
    }
}
