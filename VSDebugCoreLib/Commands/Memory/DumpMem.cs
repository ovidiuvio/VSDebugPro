using System;
using System.Globalization;
using System.IO;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    class DumpMem : BaseCommand
    {
        const char tkn_force    = 'f';
        const char tkn_append   = 'a';

        public DumpMem(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.DumpMemString)
        {
            CommandDescription = Resources.CmdDumpMemDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <optional flags> <filename> <address> <size>\n" +
                                "\tEX: " + CommandString + " c:\\memdump.bin 0x00656789 200\n"  +
                                "\tEX: " + CommandString + " -f c:\\memdump.bin 0x00656789 200\n" +
                                "\tFlags:\n" +
                                "\t\t  -" + tkn_force + "   - Force file overwrite.\n" +
                                "\t\t  -" + tkn_append + "   - Append to the file.\n" +
                                "\t<filename> - output filename\n" +
                                "\t<address>  - read address, must be a hex address / pointer, can be an expression\n" +
                                "\t<size>     - size in bytes, can be an expression\n";

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
            string[] argv = text.Split(sp,2,StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length < 2)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            }

            // check for optional parameters
            string strArgParam = argv[0];
            string strArgFile = "";
            string strArgSource = "";
            string strArgSize = "";
            char chrFlag = '\0';

            if ('-' == strArgParam[0])
            {
                // max 2 characters for flags
                if (2 == strArgParam.Length)
                {
                    chrFlag = Char.ToLower(strArgParam[1]);
                }
                else
                {
                    Context.CONSOLE.Write("Specified flag: " + strArgParam + " is invalid!");
                    return;
                }
            }

            int reqNArg = ('\0' == chrFlag) ? 3 : 4;
            argv = text.Split(sp, reqNArg, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length != reqNArg)
            {
                Context.CONSOLE.Write(CommandHelp);
                return;
            } 
            

            strArgFile   = ('\0' == chrFlag) ? argv[0] : argv[1];
            strArgSource = ('\0' == chrFlag) ? argv[1] : argv[2];
            strArgSize   = ('\0' == chrFlag) ? argv[2] : argv[3];

            // get file path
            string strPath = Path.GetDirectoryName(strArgFile);

            // check if we have full path
            if (strPath != string.Empty)
            {
                // create folder if it doesn`t exist
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
            }
            // then this was meant to be in the working directory
            else
            {
                strArgFile = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, strArgFile);

                strPath    = Path.GetDirectoryName(strArgFile);

                // create folder if it doesn`t exist
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
            }           

            // if append or force is not specified, return error
            if (File.Exists(strArgFile) && !( tkn_force == chrFlag || tkn_append == chrFlag) )
            {
                Context.CONSOLE.Write("Output file name: " + strArgFile + " is in use!");
                Context.CONSOLE.Write("Use -" + tkn_force + "/-" + tkn_append + " to overwrite/append");
                return;
            }

            var varArgSource = Context.IDE.Debugger.GetExpression(strArgSource, false, 100);
            var varArgSize = Context.IDE.Debugger.GetExpression(strArgSize, false, 100);
            int processId = Context.IDE.Debugger.CurrentProcess.ProcessID;

            if (!varArgSource.IsValidValue)
            {
                Context.CONSOLE.Write("Address: <"+strArgSource+"> is invalid!");                
                return;
            }

            if (!varArgSize.IsValidValue)
            {
                Context.CONSOLE.Write("Size: <" + strArgSize + "> is invalid!");                
                return;
            }

            long startAddress  = 0;
            long dataSize      = 0;
            NumberStyles numStyleSource = NumberHelpers.IsHexNumber( varArgSource.Value ) ? NumberStyles.HexNumber : NumberStyles.Integer;
            NumberStyles numStyleSize = NumberHelpers.IsHexNumber(varArgSize.Value) ? NumberStyles.HexNumber : NumberStyles.Integer;
            bool bRet        = true;


            bRet = bRet && NumberHelpers.TryParseLong(varArgSource.Value, numStyleSource, out startAddress);
            bRet = bRet && NumberHelpers.TryParseLong(varArgSize.Value, numStyleSize, out dataSize);

            if (!bRet)
            {
                Context.CONSOLE.Write("Failed to evaluate command arguments!");
                return;
            }

            FileMode fileMode = FileMode.Create;
            // if force flag is specified, and the file exists, then delete it
            if( tkn_force == chrFlag && File.Exists(strArgFile) )
            {
                File.Delete(strArgFile);
            }

            if (tkn_append == chrFlag && File.Exists(strArgFile))
            {
                fileMode = FileMode.Append;
            }

            int ntdbgStatus = NativeMethods.NTDBG_OK;
            if (NativeMethods.NTDBG_OK != (ntdbgStatus = MemoryHelpers.WriteMemoryToFile(strArgFile, processId, startAddress, dataSize, fileMode)))
            {
                File.Delete(strArgFile);

                Context.CONSOLE.Write("Couldn`t dump memory to file!");
                Context.CONSOLE.Write("Error code:" + ntdbgStatus.ToString() + " - " + NativeMethods.GetStatusString(ntdbgStatus) + ".");
                return;
            }

            Context.CONSOLE.Write("Wrote: " + dataSize.ToString() + " bytes to: " + MiscHelpers.GetClickableFileName(strArgFile) );
        }
    }
}
