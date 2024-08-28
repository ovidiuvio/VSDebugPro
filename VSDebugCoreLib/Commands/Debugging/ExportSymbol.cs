using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Debugging
{
    public class ExportSymbol : BaseCommand
    {
        private const char TknForce = 'f';
        private const char TknAppend = 'a';
        private const char TknJson = 'j'; // New JSON flag

        public ExportSymbol(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "export")
        {
            CommandDescription = "Fully expands a symbol or expression and saves the result to a file.";
            CommandHelpString = "Syntax: export <optional flags> <filename> <expression>\n" +
                                "\tEvaluates <expression> and saves its value to <filename>.\n" +
                                "\tFlags:\n" +
                                $"\t\t  -{TknForce}\t- Force file overwrite.\n" +
                                $"\t\t  -{TknAppend}\t- Append to the file.\n" +
                                $"\t\t  -{TknJson}\t- Export result in JSON format.\n" + // Updated help string
                                "\t<filename>   \t- output filename\n" +
                                "\t<expression> \t- symbol to evaluate\n" +
                                "\tExample: export output.txt myVariable\n" +
                                $"\tExample: export -{TknForce} output.txt myObject";
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = { ' ', '\t' };
            var argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length < 2)
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            // Parse options
            bool forceOverwrite = false;
            bool appendToFile = false;
            bool jsonMode = false; // New JSON mode flag
            string filename;
            string expression;

            int argIndex = 0;

            if (argv[0].StartsWith("-"))
            {
                string options = argv[0].Substring(1).ToLower();
                forceOverwrite = options.Contains(TknForce);
                appendToFile = options.Contains(TknAppend);
                jsonMode = options.Contains(TknJson); // Parse JSON flag
                argIndex++;
            }

            if (argIndex + 1 >= argv.Length)
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            filename = argv[argIndex++];
            expression = argv[argIndex];

            // Create the evaluator based on the current language
            var _evaluator = DebugHelpers.CreateEvaluator(Context.IDE);

            try
            {
                string result = _evaluator.EvaluateExpression(expression, jsonMode); // Pass JSON mode

                // Ensure the directory exists
                string directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directory))
                {
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                }
                else
                {
                    filename = Path.Combine(Context.Settings.GeneralSettings.WorkingDirectory, filename);

                    directory = Path.GetDirectoryName(filename);

                    // create folder if it doesn`t exist
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
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
