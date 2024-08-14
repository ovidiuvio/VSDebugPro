using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        private static readonly string[] ExcludedMembers = { "Raw View", "[comparator]", "[allocator]", "[capacity]" };

        public DebuggerExpressionEvaluator(DTE2 dte)
        {
            _dte = dte;
        }

        public string EvaluateExpression(string expression, bool jsonMode = true)
        {
            if (_dte.Debugger.CurrentMode == EnvDTE.dbgDebugMode.dbgBreakMode)
            {
                EnvDTE.Expression expr = _dte.Debugger.GetExpression(expression, true, 100);
                if (expr.IsValidValue)
                {
                    return jsonMode ? FullyExpandExpressionJson(expr) : FullyExpandExpression(expr);
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
                    if(!ExcludedMembers.Contains(childExpr.Name))
                    {
                        stack.Push((childExpr, depth + 1, childPath));
                    }
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

        private string FullyExpandExpressionJson(EnvDTE.Expression rootExpr)
        {
            return JsonConvert.SerializeObject(ExpandExpressionToObject(rootExpr), Formatting.Indented);
        }

        private JToken ExpandExpressionToObject(EnvDTE.Expression rootExpr)
        {
            var stack = new Stack<(EnvDTE.Expression Expr, int Depth, JToken Parent, string Key, string Path)>();
            HashSet<string> visitedPaths = new HashSet<string>();
            var root = new JObject();

            stack.Push((rootExpr, 0, root, rootExpr.Name, rootExpr.Name));

            while (stack.Count > 0)
            {
                var (expr, depth, parent, key, path) = stack.Pop();

                // Check for circular references
                if (visitedPaths.Contains(path))
                {
                    SetJsonValue(parent, key.Trim('"', '\''), new JValue("<circular reference>"));
                    continue;
                }

                visitedPaths.Add(path);

                if (IsPrimitiveType(expr))
                {
                    SetJsonValue(parent, key.Trim('"', '\''), new JValue(ParsePrimitiveValue(expr.Value)));
                }
                else
                {
                    var node = IsDictionary(expr) ? (JToken)new JObject() : new JArray();
                    SetJsonValue(parent, key.Trim('"', '\''), node);

                    for (int i = expr.DataMembers.Count; i > 0; i--)
                    {
                        Expression childExpr = expr.DataMembers.Item(i);
                        string childPath = $"{path}.{childExpr.Name}";
                        if (!ExcludedMembers.Contains(childExpr.Name))
                        {
                            stack.Push((childExpr, depth + 1, node, RemoveBrackets(childExpr.Name).Trim('"', '\''), childPath));
                        }
                    }
                }

                // Limit expansion depth to prevent potential issues
                if (depth > MAX_DEPTH)
                {
                    SetJsonValue(parent, key.Trim('"', '\''), new JValue("<maximum depth reached>"));
                    continue;
                }
            }

            return root[rootExpr.Name];
        }

        private bool IsPrimitiveType(EnvDTE.Expression expr)
        {
            if (expr.DataMembers.Count == 0) 
                return true;

            string[] primitiveTypes = { "int", "float", "double", "str", "bool", "char", "number", "string", "boolean" };
            return primitiveTypes.Any(type => expr.Type.ToLower().Contains(type)) &&
                   !expr.Type.ToLower().Contains("[]") &&
                   !expr.Type.ToLower().Contains("list") &&
                   !IsCollection(expr);
        }

        private object ParsePrimitiveValue(string value)
        {
            if (bool.TryParse(value, out bool boolResult))
                return boolResult;
            if (int.TryParse(value, out int intResult))
                return intResult;
            if (double.TryParse(value, out double doubleResult))
                return doubleResult;
            return value.Trim('"', '\'');
        }

        private bool IsCollection(EnvDTE.Expression expr)
        {
            string[] collectionTypes = { "array", "list", "set", "dictionary", "map", "tuple", "vector" };
            return collectionTypes.Any(type => expr.Type.ToLower().Contains(type)) ||
                   expr.Value.Trim().StartsWith("{") ||
                   expr.Value.Trim().StartsWith("[") ||
                   (expr.DataMembers.Count > 0 && expr.DataMembers.Item(1).Name == "[0]") ||
                   Regex.IsMatch(expr.Value, @"^\{\s*size\s*=\s*\d+\s*\}$");
        }

        private bool IsDictionary(EnvDTE.Expression expr)
        {
      
            string type = expr.Type.Trim();

            // Use regex to match exact types or types with generic parameters
            if (Regex.IsMatch(type, @"^(Dictionary|Map|dict|Hashtable)(<.*>)?$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(type, @"^std::(unordered_)?map<.*>$", RegexOptions.IgnoreCase))
            {
                return true;
            }

            // Special case for JavaScript/TypeScript object
            if (type.Equals("Object", StringComparison.OrdinalIgnoreCase) ||
                (expr.Value.Trim().StartsWith("{") && expr.Value.Trim().EndsWith("}") && !Regex.IsMatch(expr.Value, @"^\{\s*size\s*=\s*\d+\s*\}$")))
            {
                return true;
            }
        
            return false;
        }

        private void SetJsonValue(JToken parent, string key, JToken value)
        {
            if (parent is JObject jObj)
            {
                jObj[key] = value;
            }
            else if (parent is JArray jArr)
            {
                jArr.Add(value);
            }
        }

        private string RemoveBrackets(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Remove leading and trailing whitespace
            input = input.Trim();

            // Check if the string starts with '[' and ends with ']'
            if (input.StartsWith("[") && input.EndsWith("]"))
            {
                // Remove the first and last character
                return input.Substring(1, input.Length - 2);
            }

            // If the input doesn't have brackets, return it as is
            return input;
        }

        private string UnescapeCStyleString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Remove surrounding quotes if present
            input = input.Trim('"');

            // Replace escaped characters
            return Regex.Replace(input, @"\\([\\\""\'0abfnrtv]|x[0-9a-fA-F]{2}|u[0-9a-fA-F]{4})", m =>
            {
                string escaped = m.Groups[1].Value;
                switch (escaped[0])
                {
                    case '\\': return "\\";
                    case '"': return "\"";
                    case '\'': return "'";
                    case '0': return "\0";
                    case 'a': return "\a";
                    case 'b': return "\b";
                    case 'f': return "\f";
                    case 'n': return "\n";
                    case 'r': return "\r";
                    case 't': return "\t";
                    case 'v': return "\v";
                    case 'x':
                    case 'u':
                        int code = int.Parse(escaped.Substring(1), System.Globalization.NumberStyles.HexNumber);
                        return char.ConvertFromUtf32(code);
                    default:
                        return m.Value; // Return the original value if not recognized
                }
            });
        }
    }
}