using System;
using System.Globalization;
using System.IO;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    internal class DumpMem : BaseCommand
    {
        private const char TknForce = 'f';
        private const char TknAppend = 'a';

        public DumpMem(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.DumpMemString)
        {
            CommandDescription = Resources.CmdDumpMemDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <optional flags> <filename> <address> <size>\n" +
                                "\tEX: " + CommandString + " c:\\memdump.bin 0x00656789 200\n" +
                                "\tEX: " + CommandString + " -f c:\\memdump.bin 0x00656789 200\n" +
                                "\tFlags:\n" +
                                "\t\t  -" + TknForce + "   - Force file overwrite.\n" +
                                "\t\t  -" + TknAppend + "   - Append to the file.\n" +
                                "\t<filename> - output filename\n" +
                                "\t<address>  - read address, must be a hex address / pointer, can be an expression\n" +
                                "\t<size>     - size in bytes, can be an expression\n";

            CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
        }

        public override ECommandStatus CommandStatus
        {
            get
            {
                if (null != Context.IDE.Debugger && null != Context.IDE.Debugger.DebuggedProcesses &&
                    Context.IDE.Debugger.DebuggedProcesses.Count > 0)
                    CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
                else
                    CommandStatusFlag = ECommandStatus.CommandStatusDisabled;

                return CommandStatusFlag;
            }
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = {' ', '\t'};
            var argv = text.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length < 2)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            // check for optional parameters
            var strArgParam = argv[0];
            var strArgFile = "";
            var strArgSource = "";
            var strArgSize = "";
            var chrFlag = '\0';

            if ('-' == strArgParam[0])
            {
                // max 2 characters for flags
                if (2 == strArgParam.Length)
                {
                    chrFlag = char.ToLower(strArgParam[1]);
                }
                else
                {
                    Context.CONSOLE.Write("Specified flag: " + strArgParam + " is invalid!");
                    return;
                }
            }

            var reqNArg = '\0' == chrFlag ? 3 : 4;
            argv = text.Split(sp, reqNArg, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != reqNArg)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            strArgFile = '\0' == chrFlag ? argv[0] : argv[1];
            strArgSource = '\0' == chrFlag ? argv[1] : argv[2];
            strArgSize = '\0' == chrFlag ? argv[2] : argv[3];

            // get file path
            var strPath = Path.GetDirectoryName(strArgFile);

            // check if we have full path
            if (strPath != string.Empty)
            {
                // create folder if it doesn`t exist
                if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
            }
            // then this was meant to be in the working directory
            else
            {
                strArgFile = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, strArgFile);

                strPath = Path.GetDirectoryName(strArgFile);

                // create folder if it doesn`t exist
                if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
            }

            // if append or force is not specified, return error
            if (File.Exists(strArgFile) && !(TknForce == chrFlag || TknAppend == chrFlag))
            {
                Context.CONSOLE.Write("Output file name: " + strArgFile + " is in use!");
                Context.CONSOLE.Write("Use -" + TknForce + "/-" + TknAppend + " to overwrite/append");
                return;
            }

            var varArgSource = Context.IDE.Debugger.GetExpression(strArgSource, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);

            if (!varArgSource.IsValidValue)
            {
                Context.CONSOLE.Write("Address: <" + strArgSource + "> is invalid!");
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Size: <" + strArgSize + "> is invalid!");
                return;
            }

            long startAddress = 0;
            long dataSize = 0;
            var numStyleSource = NumberHelpers.IsHexNumber(varArgSource.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value)
                ? NumberStyles.HexNumber
                : NumberStyles.Integer;
            var bRet = true;

            bRet = bRet && NumberHelpers.TryParseLong(varArgSource.Value, numStyleSource, out startAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            var fileMode = FileMode.Create;
            // if force flag is specified, and the file exists, then delete it
            if (TknForce == chrFlag && File.Exists(strArgFile)) File.Delete(strArgFile);

            if (TknAppend == chrFlag && File.Exists(strArgFile)) fileMode = FileMode.Append;

            if (!MemoryHelpers.WriteMemoryToFile(strArgFile,
                     Context.IDE.Debugger.CurrentStackFrame,
                     startAddress,
                     dataSize,
                     fileMode
                 ))
            {
                Context.CONSOLE.Write("Couldn`t dump memory to file!");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize + " bytes to: " + MiscHelpers.GetClickableFileName(strArgFile));
        }
    }
}
