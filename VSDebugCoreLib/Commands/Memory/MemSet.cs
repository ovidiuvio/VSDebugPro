using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemSet : MemoryCommandBase
    {
        public MemSet(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdMemSetString)
        {
            CommandDescription = Resources.CmdMemSetDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <dst> <val> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0xFF 200\n" +
                                "\t<dst>  - destination address\n" +
                                "\t<val>  - pattern value 0x00 - 0xFF\n" +
                                "\t<size> - size in bytes\n";

            CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
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

            var strArgDst = argv[0];
            var strArgSize = argv[2];
            var strArgVal = argv[1];

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgVal = Context.IDE.Debugger.GetExpression(strArgVal, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

            if (!varArgDst.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgDst + " is invalid!");
                return;
            }

            if (!varArgVal.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <val>: " + strArgVal + " is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long byteval = 0;
            long dstAddress = 0;
            long dataSize = 0;
            var numStyleSrc = NumberHelpers.IsHexNumber(varArgVal.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleDst = NumberHelpers.IsHexNumber(varArgDst.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgDst.Value, numStyleDst, out dstAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgVal.Value, numStyleSrc, out byteval);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            if (!MemoryHelpers.ProcMemset(
                    Context.IDE.Debugger.CurrentStackFrame,
                    dstAddress,
                    (byte)byteval,
                    dataSize
                ))
            {
                Context.CONSOLE.Write("Memory set dst:" + NumberHelpers.ToHex(dstAddress) + " " + (byte)byteval + " " +
                                      dataSize + " failed!");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize + " bytes to address: " + NumberHelpers.ToHex(dstAddress));
        }
    }
}
