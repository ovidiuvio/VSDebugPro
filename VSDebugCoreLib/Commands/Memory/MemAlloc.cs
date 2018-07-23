using System;
using System.Globalization;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class MemAlloc : BaseCommand
    {
        public MemAlloc(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdMemAllocString)
        {
            CommandDescription = Resources.CmdMemAllocDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <size>\n" +
                                "\tEX: " + CommandString + " 200\n" +
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
            string[] argv = text.Split(sp, 1, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != 1)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            string strArgSize = argv[0];

            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            int processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long dataSize = 0;

            NumberStyles numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }


            UInt64 qwPtr = 0;
            qwPtr = MemoryHelpers.ProcAlloc(processId, dataSize);

            if (0 == qwPtr)
            {
                Context.CONSOLE.Write("Failed to allocate memory!");
                return;
            }

            Context.CONSOLE.Write("Allocated: " + dataSize.ToString() + " bytes at address: " + NumberHelpers.ToHex((long)qwPtr));
        }
    }
}
