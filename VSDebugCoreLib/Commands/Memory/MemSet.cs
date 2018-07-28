using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class MemSet : BaseCommand
    {
        public MemSet(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdMemSetString)
        {
            CommandDescription = Resources.CmdMemSetDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <dst> <val> <size>\n" +
                                "\tEX: " + CommandString + " 0x00656589 0xFF 200\n" +
                                "\t<dst>  - destination address\n" +
                                "\t<val>  - pattern value 0x00 - 0xFF\n" +
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
            string strArgVal = argv[1];

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgVal = Context.IDE.Debugger.GetExpression(strArgVal, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            int processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

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
            NumberStyles numStyleSrc = NumberHelpers.IsHexNumber(varArgVal.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleDst = NumberHelpers.IsHexNumber(varArgDst.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet = true;


            bRet = bRet && NumberHelpers.TryParseLong(varArgDst.Value, numStyleDst, out dstAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgVal.Value, numStyleSrc, out byteval);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            int ntdbgStatus = NativeMethods.NTDBG_OK;
            if (NativeMethods.NTDBG_OK != (ntdbgStatus = MemoryHelpers.ProcMemset(processId, dstAddress, (byte)byteval, dataSize)))
            {
                Context.CONSOLE.Write("Memory set dst:" + NumberHelpers.ToHex(dstAddress) + " " + ((byte)byteval).ToString() + " " + dataSize.ToString() + " failed!");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus.ToString() + " - " + NativeMethods.GetStatusString(ntdbgStatus) + ".");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize.ToString() + " bytes to address: " + NumberHelpers.ToHex(dstAddress));
        }
    }
}
