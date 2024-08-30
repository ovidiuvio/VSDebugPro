using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;

namespace VSDebugCoreLib.Utils
{
    public abstract class DebuggerExpressionEvaluator
    {
        protected readonly DTE2 _dte;
        protected static readonly string[] ExcludedMembers = { "[Raw View]", "Raw View", "[comparator]", "[allocator]", "[capacity]" };

        protected DebuggerExpressionEvaluator(DTE2 dte)
        {
            _dte = dte;
        }

        public string EvaluateExpression(string expression, bool jsonMode = false, int maxDepth = DebugHelpers.MAX_EVAL_DEPTH)
        {
            if (_dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgBreakMode)
            {
                EnvDTE.Expression expr = _dte.Debugger.GetExpression(expression, true, 100);
                if (expr.IsValidValue)
                {
                    return jsonMode ? FullyExpandExpressionJson(expr, maxDepth) : FullyExpandExpression(expr, maxDepth);
                }
                else
                {
                    var error = new { Error = $"Error evaluating expression: {expr.Value}" };
                    return jsonMode ? JsonConvert.SerializeObject(error, Formatting.Indented) : error.Error;
                }
            }
            else
            {
                var error = new { Error = "Debugger is not in break mode. Cannot evaluate expression." };
                return jsonMode ? JsonConvert.SerializeObject(error, Formatting.Indented) : error.Error;
            }
        }

        protected string FullyExpandExpression(Expression rootExpr, int maxDepth)
        {
            StringBuilder result = new StringBuilder();
            Stack<(Expression Expr, int Depth)> stack = new Stack<(Expression, int)>();

            stack.Push((rootExpr, 0));

            while (stack.Count > 0)
            {
                var (expr, depth) = stack.Pop();
                string indent = new string(' ', depth * 2);

                result.AppendLine($"{indent}{expr.Name} = {expr.Value}");

                for (int i = expr.DataMembers.Count; i > 0; i--)
                {
                    Expression childExpr = expr.DataMembers.Item(i);
                    if (!ExcludedMembers.Contains(childExpr.Name))
                    {
                        stack.Push((childExpr, depth + 1));
                    }
                }

                // Limit expansion depth to prevent potential issues
                if (depth >= maxDepth)
                {
                    result.AppendLine($"{indent}  <maximum depth reached>");
                    break;
                }
            }

            return result.ToString();
        }

        protected abstract string FullyExpandExpressionJson(Expression rootExpr, int maxDepth);
        protected abstract bool IsPrimitiveType(Expression expr);
        protected abstract bool IsCollection(Expression expr);
        protected abstract bool IsDictionary(Expression expr);

        protected virtual bool FilterDataMember(Expression parent, Expression child)
        {
            return ExcludedMembers.Contains(child.Name);
        }

        protected string TrimQuotes(string str) => str.Trim('"', '\'');

        protected object ParsePrimitiveValue(string value)
        {
            if (bool.TryParse(value, out bool boolResult))
                return boolResult;
            if (int.TryParse(value, out int intResult))
                return intResult;
            if (double.TryParse(value, out double doubleResult))
                return doubleResult;
            return value.Trim('"', '\'');
        }
    }
}
