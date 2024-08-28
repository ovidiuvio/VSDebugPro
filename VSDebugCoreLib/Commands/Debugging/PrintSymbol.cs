using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Debugging
{
    public class PrintSymbol : BaseCommand
    {
        private const char TknJson = 'j'; // New JSON flag

        public PrintSymbol(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "print")
        {
            CommandDescription = "Evaluates and prints the value of a symbol or expression.";
            CommandHelpString = "Syntax: print <optional flags> <expression>\n" +
                                "\tEvaluates <expression> and prints its value.\n" +
                                "\tFlags:\n" +
                                $"\t\t  -{TknJson}\t- Print result in JSON format.\n" + // Updated help string
                                "\t<expression> \t- symbol to evaluate\n" +
                                "\tExample: print myVariable\n" +
                                $"\tExample: print -{TknJson} myObject";
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            // Parse options
            bool jsonMode = false; // New JSON mode flag
            string expression;

            if (text.StartsWith("-"))
            {
                string options = text.Substring(1, 1).ToLower();
                jsonMode = options.Contains(TknJson); // Parse JSON flag
                expression = text.Substring(3).Trim();
            }
            else
            {
                expression = text;
            }

            // Create the evaluator based on the current language
            var _evaluator = DebugHelpers.CreateEvaluator(Context.IDE);

            try
            {
                string result = _evaluator.EvaluateExpression(expression, jsonMode); // Pass JSON mode
                Context.ConsoleEngine.Write(result);
            }
            catch (Exception ex)
            {
                Context.ConsoleEngine.Write($"Error evaluating expression: {ex.Message}");
            }
        }
    }
}
