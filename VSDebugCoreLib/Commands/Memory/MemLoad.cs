using System;
using System.Globalization;
using System.IO;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class MemLoad : MemoryCommandBase
    {
        public MemLoad(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdLoadMemString)
        {
            CommandDescription = Resources.CmdLoadMemDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <srcfile> <address> <size>\n" +
                                "\tEX: " + CommandString + " c:\\memdata.bin 0x00656789 200\n" +
                                "\t<srcfile>  - source file\n" +
                                "\t<address>  - write address, must be a hex address / pointer, can be an expression\n" +
                                "\t<size>     - size in bytes, can be an expression\n";

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

            var strArgDst = argv[1];
            var strArgSize = argv[2];
            var strArgFile = argv[0];

            // get file path
            var strPath = Path.GetDirectoryName(strArgFile);

            // then this was meant to be in the working directory
            if (strPath == string.Empty)
                strArgFile = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, strArgFile);

            if (!File.Exists(strArgFile))
            {
                Context.CONSOLE.Write("couldn`t open input file: " + strArgFile + " !");
                return;
            }

            var varArgDst = Context.IDE.Debugger.GetExpression(strArgDst, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

            if (!varArgDst.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <address>: " + strArgDst + " is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Argument <size>: " + strArgSize + " is invalid!");
                return;
            }

            long startAddress = 0;
            long dataSize = 0;
            var numStyleSource = NumberHelpers.IsHexNumber(varArgDst.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgDst.Value, numStyleSource, out startAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            var fileInfo = new FileInfo(strArgFile);

            if (fileInfo.Length < dataSize)
            {
                Context.CONSOLE.Write("Input file size:" + fileInfo.Length + " is less than the specified size:" +
                                      dataSize + " !");
                return;
            }

            if (!MemoryHelpers.LoadFileToMemory(strArgFile,
                    Context.IDE.Debugger.CurrentStackFrame,
                    startAddress,
                    dataSize
                ))
            {
                Context.CONSOLE.Write("Couldn`t load memory to address:" + "0x" + startAddress.ToString("X") + " !");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize + " bytes to address: " + "0x" + startAddress.ToString("X"));
        }
    }
}
