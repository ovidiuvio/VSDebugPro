using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class MemFree : BaseCommand
    {
        public MemFree(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdMemFreeString)
        {
            CommandDescription = Resources.CmdMemFreeDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <address>\n" +
                                "\tEX: " + CommandString + " 0x00500000\n" +
                                "\t<address> - allocation pointer address\n";

            CommandStatusFlag = eCommandStatus.CommandStatus_Disabled;
        }

        public override eCommandStatus CommandStatus
        {
            get
            {
                if (null != Context.IDE.Debugger && null != Context.IDE.Debugger.DebuggedProcesses && Context.IDE.Debugger.DebuggedProcesses.Count > 0)
                {
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
            string[] argv = text.Split(sp, 1, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 1)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            string strArgAddr = argv[0];

            var varArgAddr = Context.IDE.Debugger.GetExpression(strArgAddr, false, 100);
            int processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgAddr.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgAddr + " is invalid!");
                return;
            }

            long lpAddress = 0;

            NumberStyles numStyleAddr = NumberHelpers.IsHexNumber(varArgAddr.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgAddr.Value, numStyleAddr, out lpAddress);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            int ntdbgStatus = NativeMethods.NTDBG_OK;
            if (NativeMethods.NTDBG_OK != (ntdbgStatus = MemoryHelpers.ProcFree(processId,lpAddress)))
            {
                Context.CONSOLE.Write("Failed to release memory!");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus.ToString() + " - " + NativeMethods.GetStatusString(ntdbgStatus) + ".");
                return;
            }

            Context.CONSOLE.Write("Released memory at address: " + NumberHelpers.ToHex(lpAddress));
        }
    }
}
