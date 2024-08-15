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
        private static readonly string[] ExcludedMembers = { "[Raw View]", "Raw View", "[comparator]", "[allocator]", "[capacity]" };

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
            var stack = new Stack<(EnvDTE.Expression Expr, int Depth, JToken Parent, string Key)>();
            var root = new JObject();

            stack.Push((rootExpr, 0, root, rootExpr.Name));

            while (stack.Count > 0)
            {
                var (expr, depth, parent, key) = stack.Pop();

                if (IsPrimitiveType(expr))
                {
                    SetJsonValue(parent, TrimQuotes(key), new JValue(ParsePrimitiveValue(TrimQuotes(expr.Value))));
                }
                else if (IsDictionary(expr))
                {
                    var node = new JObject();
                    SetJsonValue(parent, TrimQuotes(key), node);

                    for (int i = expr.DataMembers.Count; i > 0; i--)
                    {
                        Expression childExpr = expr.DataMembers.Item(i);
                        if (!FilterDataMember(expr, childExpr))
                        {
                            if (childExpr.Name.StartsWith("[") && childExpr.Name.EndsWith("]"))
                            {
                                var keyExpr = childExpr.DataMembers.Item(1); // "first" member
                                var valueExpr = childExpr.DataMembers.Item(2); // "second" member
                                stack.Push((valueExpr, depth + 1, node, keyExpr.Value.Trim('"', '\'')));
                            }
                        }
                    }
                }
                else if (IsCollection(expr))
                {
                    var node = new JArray();
                    SetJsonValue(parent, TrimQuotes(key), node);

                    for (int i = expr.DataMembers.Count; i > 0; i--)
                    {
                        Expression childExpr = expr.DataMembers.Item(i);
                        if (!FilterDataMember(expr, childExpr))
                        {
                            stack.Push((childExpr, depth + 1, node, TrimQuotes(RemoveBrackets(childExpr.Name))));
                        }
                    }
                }
                else if (IsPair(expr))
                {
                    var node = new JObject();
                    SetJsonValue(parent, TrimQuotes(key), node);

                    if (expr.DataMembers.Count >= 2)
                    {
                        stack.Push((expr.DataMembers.Item(1), depth + 1, node, "first"));
                        stack.Push((expr.DataMembers.Item(2), depth + 1, node, "second"));
                    }
                }
                else // Custom object or other complex type
                {
                    var node = new JObject();
                    SetJsonValue(parent, TrimQuotes(key), node);

                    for (int i = 1; i <= expr.DataMembers.Count; i++)
                    {
                        Expression childExpr = expr.DataMembers.Item(i);
                        if (!FilterDataMember(expr, childExpr))
                        {
                            stack.Push((childExpr, depth + 1, node, childExpr.Name));
                        }
                    }
                }

                // Limit expansion depth to prevent potential issues
                if (depth > MAX_DEPTH)
                {
                    SetJsonValue(parent, TrimQuotes(key), new JValue("<maximum depth reached>"));
                    continue;
                }
            }

            return root[rootExpr.Name];
        }

        private bool FilterDataMember(EnvDTE.Expression parent, EnvDTE.Expression child)
        {
            // filter banned nodes
            if (ExcludedMembers.Contains(child.Name))
            {
                return true;
            }

            // C++ filter unordered_ natvis members
            if (Regex.IsMatch(parent.Type, @"^std::unordered_(map|multimap|set|multiset)<.*>$", RegexOptions.IgnoreCase))
            {
                string[] ExcludeKeywords = { "[hash_function]", "[key_eq]" };
                if (ExcludeKeywords.Contains(child.Name))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPrimitiveType(EnvDTE.Expression expr)
        {
            if (expr.DataMembers.Count == 0)
                return true;

            string[] primitiveTypes = { "int", "float", "double", "str", "bool", "char", "number", "string", "boolean" };
            return primitiveTypes.Any(type => expr.Type.ToLower().Contains(type)) &&
                   !expr.Type.ToLower().Contains("[]") &&
                   !expr.Type.ToLower().Contains("list") &&
                   !expr.Type.ToLower().Contains("<") && !expr.Type.ToLower().Contains(">") &&
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

        private bool IsVector(EnvDTE.Expression expr)
        {
            return expr.Type.StartsWith("std::vector<") || expr.Type.StartsWith("vector<");
        }

        private bool IsPair(EnvDTE.Expression expr)
        {
            return expr.Type.StartsWith("std::pair<") || expr.Type.StartsWith("pair<");
        }

        private bool IsCollection(EnvDTE.Expression expr)
        {
            string[] collectionTypes = { "array", "list", "set", "dictionary", "map", "tuple", "vector" };
            return collectionTypes.Any(type => expr.Type.ToLower().Contains(type)) ||
                   expr.Value.Trim().StartsWith("[") ||
                   (expr.DataMembers.Count > 0 && expr.DataMembers.Item(1).Name == "[0]") ||
                   Regex.IsMatch(expr.Value, @"^\{\s*size\s*=\s*\d+\s*\}$");
        }

        private bool IsDictionary(EnvDTE.Expression expr)
        {

            string type = expr.Type.Trim();

            // Use regex to match exact types or types with generic parameters
            if (Regex.IsMatch(type, @"^(Dictionary|Map|dict|Hashtable)(<.*>)?$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(type, @"^std::(unordered_)?map<.*>$", RegexOptions.IgnoreCase) ||
                Regex.IsMatch(type, @"^std::(unordered_)?multimap<.*>$", RegexOptions.IgnoreCase))
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

        private string TrimQuotes(string str)
        {
            return str.Trim('"', '\'');
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