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
        private readonly DebuggerExpressionEvaluator _evaluator;

        public PrintSymbol(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, "print")
        {
            CommandDescription = "Evaluates and prints the value of a symbol or expression.";
            CommandHelpString = "Syntax: print <expression>\n" +
                                "\tEvaluates <expression> and prints its value.\n" +
                                "\tExample: print myVariable\n";

            _evaluator = new DebuggerExpressionEvaluator(context.IDE);
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            string expression = text;

            try
            {
                string result = _evaluator.EvaluateExpression(expression);
                Context.ConsoleEngine.Write(result);
            }
            catch (Exception ex)
            {
                Context.ConsoleEngine.Write($"Error evaluating expression: {ex.Message}");
            }
        }
    }
}
