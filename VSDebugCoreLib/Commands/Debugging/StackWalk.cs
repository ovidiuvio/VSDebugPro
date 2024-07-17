using System;
using System.IO;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio.Debugger.Interop;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Debugging
{
    internal class StackWalk : BaseCommand
    {
        public StackWalk(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "stackwalk")
        {
            CommandDescription = "Performs a structured dump of the call stack for current or all threads, with optional file output.";
            CommandHelpString = "Syntax: <" + CommandString + "> [-o <filename>] [depth] [all]\n" +
                                "\tEX: " + CommandString + " -o stack.txt\n" +
                                "\tEX: " + CommandString + " -o stack.txt 10\n" +
                                "\tEX: " + CommandString + " -o stack.txt all\n" +
                                "\tEX: " + CommandString + " -o stack.txt 5 all\n" +
                                "\t-o <filename> - Optional. If specified, writes the dump to the specified file.\n" +
                                "\t<depth> - Optional. Number of frames to dump per thread. Default is all frames.\n" +
                                "\t<all> - Optional. If specified, dumps stack for all threads. Otherwise, only current thread.\n";
            CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            string[] args = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int depth = int.MaxValue;
            bool dumpAllThreads = false;
            string outputFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++; // Skip the next argument as it's the filename
                }
                else if (int.TryParse(args[i], out int parsedDepth))
                {
                    depth = parsedDepth;
                }
                else if (args[i].ToLower() == "all")
                {
                    dumpAllThreads = true;
                }
            }

            if (Context.IDE.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                Context.ConsoleEngine.Write("The debugger must be in break mode to use this command.");
                return;
            }

            StringBuilder output = new StringBuilder();
            output.AppendLine("Stack Dump:");
            output.AppendLine("===========");

            if (dumpAllThreads)
            {
                foreach (EnvDTE.Thread thread in Context.IDE.Debugger.CurrentProgram.Threads)
                {
                    DumpThreadStack(thread, depth, output);
                }
            }
            else
            {
                DumpThreadStack(Context.IDE.Debugger.CurrentThread, depth, output);
            }

            string dumpContent = output.ToString();

            // Output to file if specified
            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    // get file path
                    var strPath = Path.GetDirectoryName(outputFile);

                    // check if we have full path
                    if (strPath != string.Empty)
                    {
                        // create folder if it doesn`t exist
                        if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
                    }
                    // then this was meant to be in the working directory
                    else
                    {
                        outputFile = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, outputFile);

                        strPath = Path.GetDirectoryName(outputFile);

                        // create folder if it doesn`t exist
                        if (!Directory.Exists(strPath)) Directory.CreateDirectory(strPath);
                    }

                    if (File.Exists(outputFile))
                    {
                        Context.ConsoleEngine.Write("Output file name: " + outputFile + " is in use!");
                        return;
                    }

                    File.WriteAllText(outputFile, dumpContent);
                    Context.ConsoleEngine.Write($"Stack dump written to file: {MiscHelpers.GetClickableFileName(outputFile)}");
                }
                catch (Exception ex)
                {
                    Context.ConsoleEngine.Write($"Error writing to file: {ex.Message}");
                }
            }
            else
            {
                Context.ConsoleEngine.Write(dumpContent);
            }
        }

        private void DumpThreadStack(EnvDTE.Thread thread, int depth, StringBuilder output)
        {
            output.AppendLine($"Thread ID: {thread.ID}, Name: {thread.Name}");
            output.AppendLine("-------------------------------------------");

            EnvDTE.StackFrames frames = thread.StackFrames;

            for (int i = 0; i < frames.Count && i < depth; i++)
            {
                EnvDTE.StackFrame frame = frames.Item(i + 1);
                output.AppendLine($"Frame {i}:");
                output.AppendLine($"  Function: {frame.FunctionName}");

                // Dump local variables
                output.AppendLine("  Local Variables:");
                foreach (Expression local in frame.Locals)
                {
                    output.AppendLine($"    {local.Name} = {local.Value} ({local.Type})");
                }

                // Dump function parameters
                output.AppendLine("  Parameters:");
                foreach (Expression param in frame.Arguments)
                {
                    output.AppendLine($"    {param.Name} = {param.Value} ({param.Type})");
                }

                output.AppendLine();
            }

            output.AppendLine();
        }
    }
}
