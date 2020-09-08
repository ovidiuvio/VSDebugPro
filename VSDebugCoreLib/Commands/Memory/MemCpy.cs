using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemCpy : BaseCommand
    {
        public MemCpy(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdMemCpyString)
        {
            CommandDescription = Resources.CmdMemCpyDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <dst> <src> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0x00656789 200\n" +
                                "\t<dst>  - destination address\n" +
                                "\t<src>  - source address\n" +
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

            if (args.Length != 3)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            var strArgDst = args[0];
            var strArgSrc = args[1];
            var strArgSize = args[2];

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgSrc = Context.IDE.Debugger.GetExpression(strArgSrc, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

            if (!varArgDst.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgDst + " is invalid!");
                return;
            }

            if (!varArgSrc.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgSrc + " is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long srcAddress = 0;
            long dstAddress = 0;
            long dataSize = 0;
            var numStyleSrc = NumberHelpers.IsHexNumber(varArgSrc.Value)
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
            bRet = bRet && NumberHelpers.TryParseLong(varArgSrc.Value, numStyleSrc, out srcAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            if (!MemoryHelpers.ProcMemoryCopy(
                   Context.IDE.Debugger.CurrentStackFrame,
                   dstAddress,
                   srcAddress,
                   dataSize
               ))
            {
                Context.CONSOLE.Write("Memory copy src:" + NumberHelpers.ToHex(srcAddress) + " dst:" +
                                      NumberHelpers.ToHex(dstAddress) + " " + dataSize + " failed!");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize + " bytes to address: " + NumberHelpers.ToHex(dstAddress));
        }
    }
}
