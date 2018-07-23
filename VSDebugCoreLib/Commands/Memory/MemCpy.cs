using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class MemCpy : BaseCommand
    {
        public MemCpy(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdMemCpyString)
        {
            CommandDescription = Resources.CmdMemCpyDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <dst> <src> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0x00656789 200\n" +
                                "\t<dst>  - destination address\n" +
                                "\t<src>  - source address\n" +
                                "\t<size> - size in bytes\n";

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
            string[] argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 3)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            string strArgDst = argv[0];
            string strArgSize = argv[2];
            string strArgSrc = argv[1];

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgSrc = Context.IDE.Debugger.GetExpression(strArgSrc, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            int processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

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
            NumberStyles numStyleSrc  = NumberHelpers.IsHexNumber(varArgSrc.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleDst  = NumberHelpers.IsHexNumber(varArgDst.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet = true;


            bRet = bRet && NumberHelpers.TryParseLong(varArgDst.Value, numStyleDst, out dstAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSrc.Value, numStyleSrc, out srcAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }
                      

            if (!MemoryHelpers.ProcMemoryCopy( processId, dstAddress, srcAddress, dataSize))
            {
                Context.CONSOLE.Write("Memory copy src:" + NumberHelpers.ToHex(srcAddress) + " dst:" + NumberHelpers.ToHex(dstAddress) + " " + dataSize.ToString() + " failed!");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize.ToString() + " bytes to address: " + NumberHelpers.ToHex(dstAddress));
        }
    }
}
