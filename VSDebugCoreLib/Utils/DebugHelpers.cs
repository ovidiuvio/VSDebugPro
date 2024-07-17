using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;

namespace VSDebugCoreLib.Utils
{
    public class DebugHelpers
    {
        public static bool IsMiniDumpProcess(Process process)
        {
            var strExt = Path.GetExtension(process.Name.ToLower());

            if (".dmp" == strExt)
                return true;

            return false;
        }

    }

    public class DebuggerExpressionEvaluator
    {
        private readonly DTE2 _dte;

        public DebuggerExpressionEvaluator(DTE2 dte)
        {
            _dte = dte;
        }

        public string EvaluateAndExpandSymbol(string symbolName, string indent = "")
        {
            var output = new StringBuilder();
            var symbolsToEvaluate = new Stack<(string Symbol, string expression, string Indent)>();
            symbolsToEvaluate.Push((symbolName, symbolName, indent));

            while (symbolsToEvaluate.Count > 0)
            {
                var (currentSymbol, currentExpression, currentIndent) = symbolsToEvaluate.Pop();

                var expression = _dte.Debugger.GetExpression(currentExpression, true, 100);
                if (!expression.IsValidValue)
                {
                    output.AppendLine($"{currentIndent}{currentSymbol} = <Invalid Expression>");
                    continue; // Skip to the next symbol
                }

                output.AppendLine($"{currentIndent}{currentSymbol} = {expression.Value} ({expression.Type})");

                if (IsCollectionType(expression))
                {
                    var sizeExpr = _dte.Debugger.GetExpression($"{currentExpression}.size()", false, 100);
                    int size = sizeExpr.IsValidValue ? int.Parse(sizeExpr.Value) : 0;
                    if (!sizeExpr.IsValidValue)
                    {
                        Match match = Regex.Match(expression.Value, @"size=(\d+)");

                        if (match.Success)
                        {
                            size = int.Parse(match.Groups[1].Value);
                        }
                    }

                    // Push collection elements onto the stack in reverse order for evaluation
                    for (int i = size - 1; i >= 0; i--)
                    {
                        if (expression.Type.Contains("std::list"))
                        {
                            symbolsToEvaluate.Push(($"{currentSymbol}[{i}]", $"$LinkedListItem({currentExpression}._Mypair._Myval2._Myhead->_Next, {i}, _Next)->_Myval", currentIndent + "  "));
                        }
                        else
                        {
                            symbolsToEvaluate.Push(($"{currentSymbol}[{i}]", $"{currentExpression}[{i}]", currentIndent + "  "));
                        }
                    }
                }
                else if (expression.DataMembers.Count > 0)
                {
                    for (int i = expression.DataMembers.Count - 1; i >= 0; i--)
                    {
                        var element = expression.DataMembers.Item(i + 1);
                        symbolsToEvaluate.Push(($"{currentSymbol}.{element.Name}", $"{currentExpression}.{element.Name}", currentIndent + "  "));
                    }
                }
            }

            return output.ToString();
        }

        private bool IsCollectionType(Expression expression)
        {
            return expression.Type.Contains("std::vector") ||
                   expression.Type.Contains("std::list") ||
                   expression.Type.Contains("std::map") ||
                   expression.Type.EndsWith("]");// || // For C# arrays
                   //(expression.Value is string && expression.Value.StartsWith("{") && expression.Value.EndsWith("}")); // For C# collections
        }

        public string EvaluateExpression(string expression, bool fullyExpand = false)
        {
            if (_dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode)
            {
                Expression expr = _dte.Debugger.GetExpression(expression);
                if (expr.IsValidValue)
                {
                    if (fullyExpand)
                    {
                        return FullyExpandExpression(expr);
                    }
                    else
                    {
                        return expr.Value;
                    }
                }
                else
                {
                    return $"Error evaluating expression: {expr.Value}";
                }
            }
            else
            {
                return "Debugger is not in break mode. Cannot evaluate expression.";
            }
        }

        private string FullyExpandExpression(Expression rootExpr)
        {
            StringBuilder result = new StringBuilder();
            Stack<(Expression Expr, int Depth, string Path)> stack = new Stack<(Expression, int, string)>();
            HashSet<string> visitedPaths = new HashSet<string>();

            stack.Push((rootExpr, 0, rootExpr.Name));

            while (stack.Count > 0)
            {
                var (expr, depth, path) = stack.Pop();
                string indent = new string(' ', depth * 2);

                // Check for circular references
                if (visitedPaths.Contains(path))
                {
                    result.AppendLine($"{indent}{expr.Name} = <circular reference>");
                    continue;
                }

                visitedPaths.Add(path);

                result.AppendLine($"{indent}{expr.Name} = {expr.Value}");

                if (IsCollection(expr))
                {
                    ExpandCollection(expr, depth, path, stack);
                }
                else
                {
                    // Push children to stack in reverse order to maintain correct output order
                    for (int i = expr.DataMembers.Count - 1; i >= 0; i--)
                    {
                        Expression childExpr = expr.DataMembers.Item(i + 1);
                        string childPath = $"{path}.{childExpr.Name}";
                        stack.Push((childExpr, depth + 1, childPath));
                    }
                }

                // Limit expansion depth to prevent potential issues
                if (depth > 100)
                {
                    result.AppendLine($"{indent}  <maximum depth reached>");
                    break;
                }
            }

            return result.ToString();
        }

        private bool IsCollection(Expression expr)
        {
            return expr.Type.Contains("std::vector") ||
                    expr.Type.Contains("std::list") ||
                    expr.Type.Contains("std::map") ||
                    expr.Type.EndsWith("]") || // For C# arrays
                    (expr.Value is string && expr.Value.StartsWith("{") && expr.Value.EndsWith("}"));
        }

        private void ExpandCollection(Expression expr, int depth, string path, Stack<(Expression, int, string)> stack)
        {
            var sizeExpr = _dte.Debugger.GetExpression($"{expr.Name}.size()", false, 100);
            int size = sizeExpr.IsValidValue ? int.Parse(sizeExpr.Value) : 0;

            for (int i = size - 1; i >= 0; i--)
            {
                string elementExpr = $"{expr.Name}[{i}]";
                Expression element = _dte.Debugger.GetExpression(elementExpr, true, 100);
                string elementPath = $"{path}[{i}]";
                stack.Push((element, depth + 1, elementPath));
            }
        }
    }
}