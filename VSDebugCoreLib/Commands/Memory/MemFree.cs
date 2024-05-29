using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemFree : MemoryCommandBase
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

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = {' ', '\t'};
            var argv = text.Split(sp, 1, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 1)
            {
                Context.ConsoleEngine.Write(CommandHelp);
                return;
            }

            var strArgAddr = argv[0];

            var varArgAddr = Context.IDE.Debugger.GetExpression(strArgAddr, false, 100);

            if (!varArgAddr.IsValidValue)
            {
                Context.ConsoleEngine.Write("Argument <address>: " + strArgAddr + " is invalid!");
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
                Context.ConsoleEngine.Write("Failed to evaluate command arguments!");
                return;
            }

            MemoryHelpers.ProcFree(Context.IDE.Debugger.CurrentStackFrame, lpAddress);

            Context.ConsoleEngine.Write("Released memory at address: " + NumberHelpers.ToHex(lpAddress));
        }
    }
}
