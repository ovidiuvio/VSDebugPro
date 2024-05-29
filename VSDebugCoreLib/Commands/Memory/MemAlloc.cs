using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemAlloc : MemoryCommandBase
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

            var strArgSize = argv[0];

            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

            if (!varArgSize.IsValidValue)
            {
                Context.ConsoleEngine.Write("Argument <size>: " + strArgSize + " is invalid!");
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
                Context.ConsoleEngine.Write("Failed to evaluate command arguments!");
                return;
            }

            ulong qwPtr = MemoryHelpers.ProcAlloc(Context.IDE.Debugger.CurrentStackFrame, dataSize);

            if (0 == qwPtr)
            {
                Context.ConsoleEngine.Write("Failed to allocate memory!");
                return;
            }

            Context.ConsoleEngine.Write("Allocated: " + dataSize + " bytes at address: " + NumberHelpers.ToHex((long) qwPtr));
        }
    }
}
