using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemFree : BaseCommand
    {
        public MemFree(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdMemFreeString)
        {
            CommandDescription = Resources.CmdMemFreeDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <address>\n" +
                                "\tEX: " + CommandString + " 0x00500000\n" +
                                "\t<address> - allocation pointer address\n";

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

            var strArgAddr = args[0];

            var varArgAddr = Context.IDE.Debugger.GetExpression(strArgAddr, false, 100);

            if (!varArgAddr.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgAddr + " is invalid!");
                return;
            }

            long lpAddress = 0;

            var numStyleAddr = NumberHelpers.IsHexNumber(varArgAddr.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr.Value, numStyleAddr, out lpAddress);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            MemoryHelpers.ProcFree(Context.IDE.Debugger.CurrentStackFrame, lpAddress);

            Context.CONSOLE.Write("Released memory at address: " + NumberHelpers.ToHex(lpAddress));
        }
    }
}
