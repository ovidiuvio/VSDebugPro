using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Debugging
{
    public class ExportSymbol : BaseCommand
    {
        private const char TknForce = 'f';
        private const char TknAppend = 'a';
        private const char TknJson = 'j';
        private const char TknDepth = 'd';

        public ExportSymbol(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "export")
        {
            CommandDescription = "Fully expands a symbol or expression and saves the result to a file.";
            CommandHelpString = "Syntax: export <optional flags> <filename> <expression>\n" +
                                "\tEvaluates <expression> and saves its value to <filename>.\n" +
                                "\tFlags:\n" +
                                $"\t\t  -{TknForce}\t- Force file overwrite.\n" +
                                $"\t\t  -{TknAppend}\t- Append to the file.\n" +
                                $"\t\t  -{TknJson}\t- Export result in JSON format.\n" +
                                $"\t\t  -{TknDepth} <num>\t- Specify evaluation depth (default is {DebugHelpers.MAX_EVAL_DEPTH}).\n" +
                                "\t<filename>   \t- output filename\n" +
                                "\t<expression> \t- symbol to evaluate\n" +
                                "\tExample: export output.txt myVariable\n" +
                                $"\tExample: export -{TknForce} output.txt myObject\n" +
                                $"\tExample: export -{TknJson} -{TknDepth} 10 output.txt myComplexObject\n" +
                                $"\tExample: export -{TknDepth} 5 -{TknJson} -{TknForce} output.txt myComplexObject";
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            var args = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (args.Length < 2)
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            // Parse options
            bool forceOverwrite = false;
            bool appendToFile = false;
            bool jsonMode = false;
            int depth = DebugHelpers.MAX_EVAL_DEPTH;
            string filename;
            string expression;

            var argsList = new List<string>(args);

            for (int i = 0; i < argsList.Count - 2; i++)
            {
                if (argsList[i].StartsWith("-"))
                {
                    string option = argsList[i].Substring(1).ToLower();
                    if (option == TknForce.ToString()) forceOverwrite = true;
                    else if (option == TknAppend.ToString()) appendToFile = true;
                    else if (option == TknJson.ToString()) jsonMode = true;
                    else if (option == TknDepth.ToString())
                    {
                        if (i + 1 < argsList.Count - 2 && int.TryParse(argsList[i + 1], out int parsedDepth))
                        {
                            depth = parsedDepth;
                            argsList.RemoveAt(i + 1); // Remove the depth value
                        }
                        else
                        {
                            Context.ConsoleEngine.Write($"Invalid depth specified after -{TknDepth}. Using default depth.");
                        }
                    }
                    argsList.RemoveAt(i);
                    i--;
                }
            }

            if (argsList.Count < 2)
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            filename = argsList[argsList.Count - 2];
            expression = argsList[argsList.Count - 1];

            // Create the evaluator based on the current language
            var _evaluator = DebugHelpers.CreateEvaluator(Context.IDE);

            try
            {
                string result = _evaluator.EvaluateExpression(expression, jsonMode, depth);

                // Ensure the directory exists
                string directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    filename = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, filename);
                    directory = Path.GetDirectoryName(filename);
                    Directory.CreateDirectory(directory);
                }

                // Check if file exists
                if (File.Exists(filename) && !(forceOverwrite || appendToFile))
                {
                    Context.ConsoleEngine.Write($"Output file {filename} already exists. Use -{TknForce} to overwrite or -{TknAppend} to append.");
                    return;
                }

                // Determine FileMode
                FileMode fileMode = appendToFile ? FileMode.Append : FileMode.Create;

                // Write the result to the file
                using (StreamWriter writer = new StreamWriter(filename, appendToFile))
                {
                    writer.WriteLine(result);
                }

                Context.ConsoleEngine.Write($"Result exported to {MiscHelpers.GetClickableFileName(filename)}");
            }
            catch (Exception ex)
            {
                Context.ConsoleEngine.Write($"Error exporting result: {ex.Message}");
            }
        }
    }
}