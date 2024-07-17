using System;
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
        private const int MAX_DEPTH = 100;
        private const int MAX_CONTAINER_ELEMENTS = 1000;

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
                    if (expression.Type.Contains("std::map") || expression.Type.Contains("std::list"))
                    {
                        output.Append(FullyExpandExpression(expression));
                    }
                    else
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

        public string EvaluateExpression(string expression)
        {
            if (_dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode)
            {
                Expression expr = _dte.Debugger.GetExpression(expression, true, 100);
                if (expr.IsValidValue)
                {
                    return FullyExpandExpression(expr);
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

                for (int i = expr.DataMembers.Count; i > 0; i--)
                {
                    Expression childExpr = expr.DataMembers.Item(i);
                    string childPath = $"{path}.{childExpr.Name}";
                    if (!childExpr.Name.Contains("Raw View"))
                        stack.Push((childExpr, depth + 1, childPath));
                }

                // Limit expansion depth to prevent potential issues
                if (depth > MAX_DEPTH)
                {
                    result.AppendLine($"{indent}  <maximum depth reached>");
                    break;
                }
            }

            return result.ToString();
        }

        private bool IsStdMap(Expression expr)
        {
            return expr.Type.Contains("std::map") || expr.Type.Contains("std::unordered_map");
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

        private void ExpandStdMap(Expression mapExpr, int depth, string path, Stack<(Expression, int, string)> stack)
        {
            int size = GetMapSize(mapExpr);
            int elementsToShow = Math.Min(size, MAX_CONTAINER_ELEMENTS);

            for (int i = 0; i < elementsToShow; i++)
            {
                string nodeExpr = $"{mapExpr.Name}._Mypair._Myval2._Myval2._Myhead->_Parent->_Left";
                for (int j = 0; j < i; j++)
                {
                    nodeExpr = $"{nodeExpr}->_Parent";
                }
                Expression keyExpr = _dte.Debugger.GetExpression($"{nodeExpr}->_Myval.first");
                Expression valueExpr = _dte.Debugger.GetExpression($"{nodeExpr}->_Myval.second");

                string keyPath = $"{path}[{i}].key";
                string valuePath = $"{path}[{i}].value";

                stack.Push((valueExpr, depth + 2, valuePath));
                stack.Push((keyExpr, depth + 2, keyPath));

                Expression pairExpr = _dte.Debugger.GetExpression($"\"Pair {i}\"");
                stack.Push((pairExpr, depth + 1, $"{path}[{i}]"));
            }

            if (size > MAX_CONTAINER_ELEMENTS)
            {
                Expression dummyExpr = _dte.Debugger.GetExpression($"\"... {size - MAX_CONTAINER_ELEMENTS} more elements ...\"");
                stack.Push((dummyExpr, depth + 1, $"{path}[{MAX_CONTAINER_ELEMENTS}]"));
            }
        }

        private int GetMapSize(Expression mapExpr)
        {
            Expression sizeExpr = _dte.Debugger.GetExpression($"{mapExpr.Name}._Mypair._Myval2._Myval2._Mysize");
            if (sizeExpr.IsValidValue)
            {
                return int.Parse(sizeExpr.Value);
            }
            return 0;
        }
    }
}