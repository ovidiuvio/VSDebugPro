using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VSDebugCoreLib.Utils
{
    public class CppDebuggerExpressionEvaluator : DebuggerExpressionEvaluator
    {
        public CppDebuggerExpressionEvaluator(DTE2 dte) : base(dte) { }

        protected override string FullyExpandExpressionJson(Expression rootExpr)
        {
            return JsonConvert.SerializeObject(ExpandExpressionToObject(rootExpr), Formatting.Indented);
        }

        private JToken ExpandExpressionToObject(Expression rootExpr)
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

        protected override bool IsPrimitiveType(EnvDTE.Expression expr)
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

        protected override bool IsCollection(EnvDTE.Expression expr)
        {
            string[] collectionTypes = { "array", "list", "set", "dictionary", "map", "tuple", "vector" };
            return collectionTypes.Any(type => expr.Type.ToLower().Contains(type)) ||
                   expr.Value.Trim().StartsWith("[") ||
                   (expr.DataMembers.Count > 0 && expr.DataMembers.Item(1).Name == "[0]") ||
                   Regex.IsMatch(expr.Value, @"^\{\s*size\s*=\s*\d+\s*\}$");
        }

        protected override bool IsDictionary(EnvDTE.Expression expr)
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

        private bool IsPair(Expression expr)
        {
            return expr.Type.StartsWith("std::pair<") || expr.Type.StartsWith("pair<");
        }

        protected override bool FilterDataMember(Expression parent, Expression child)
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
            // C++ filter variant index
            if (Regex.IsMatch(parent.Type, @"^std::variant<.*>$", RegexOptions.IgnoreCase))
            {
                string[] ExcludeKeywords = { "index" };
                if (ExcludeKeywords.Contains(child.Name))
                {
                    return true;
                }
            }
            // C++ filter shared_ptr meta members
            if (Regex.IsMatch(parent.Type, @"^std::shared_ptr<.*>$", RegexOptions.IgnoreCase))
            {
                string[] ExcludeKeywords = { "[control block]" };
                if (ExcludeKeywords.Contains(child.Name))
                {
                    return true;
                }
            }
            // C++ filter unique_ptr meta members
            if (Regex.IsMatch(parent.Type, @"^std::unique_ptr<.*>$", RegexOptions.IgnoreCase))
            {
                string[] ExcludeKeywords = { "[deleter]" };
                if (ExcludeKeywords.Contains(child.Name))
                {
                    return true;
                }
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
    }
}
