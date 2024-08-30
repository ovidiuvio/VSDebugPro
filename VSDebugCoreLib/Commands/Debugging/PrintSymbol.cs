using System;
using System.Collections.Generic;
using System.Linq;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Debugging
{
    public class PrintSymbol : BaseCommand
    {
        private const char TknJson = 'j'; // JSON flag
        private const char TknDepth = 'd'; // Depth flag

        public PrintSymbol(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "print")
        {
            CommandDescription = "Evaluates and prints the value of a symbol or expression.";
            CommandHelpString = "Syntax: print <optional flags> <expression>\n" +
                                "\tEvaluates <expression> and prints its value.\n" +
                                "\tFlags:\n" +
                                $"\t\t  -{TknJson}\t- Print result in JSON format.\n" +
                                $"\t\t  -{TknDepth} <num>\t- Specify evaluation depth (default is {DebugHelpers.MAX_EVAL_DEPTH}).\n" +
                                "\t<expression> \t- symbol to evaluate\n" +
                                "\tExample: print myVariable\n" +
                                $"\tExample: print -{TknJson} myObject\n" +
                                $"\tExample: print -{TknJson} -{TknDepth} 10 myComplexObject\n" +
                                $"\tExample: print -{TknDepth} 5 -{TknJson} myComplexObject";
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
            bool jsonMode = false;
            int depth = DebugHelpers.MAX_EVAL_DEPTH;
            string expression;

            var args = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var argsList = new List<string>(args);

            for (int i = 0; i < argsList.Count; i++)
            {
                if (argsList[i].StartsWith("-"))
                {
                    string option = argsList[i].Substring(1).ToLower();
                    if (option == TknJson.ToString())
                    {
                        jsonMode = true;
                        argsList.RemoveAt(i);
                        i--;
                    }
                    else if (option == TknDepth.ToString())
                    {
                        if (i + 1 < argsList.Count && int.TryParse(argsList[i + 1], out int parsedDepth))
                        {
                            depth = parsedDepth;
                            argsList.RemoveAt(i);
                            argsList.RemoveAt(i); // Remove the depth value too
                            i--;
                        }
                        else
                        {
                            Context.ConsoleEngine.Write($"Invalid depth specified after -{TknDepth}. Using default depth.");
                        }
                    }
                }
            }

            expression = string.Join(" ", argsList);

            // Create the evaluator based on the current language
            var _evaluator = DebugHelpers.CreateEvaluator(Context.IDE);

            try
            {
                string result = _evaluator.EvaluateExpression(expression, jsonMode, depth);
                Context.ConsoleEngine.Write(result);
            }
            catch (Exception ex)
            {
                Context.ConsoleEngine.Write($"Error evaluating expression: {ex.Message}");
            }
        }
    }
}