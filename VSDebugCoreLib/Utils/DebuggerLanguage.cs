using EnvDTE;
using EnvDTE80;
using System;

namespace VSDebugCoreLib.Utils
{
    public enum DebuggerLanguage
    {
        Unknown,
        CPlusPlus,
        CSharp,
        TypeScript,
        JavaScript,
        Python,
        AspNet
    }

    public class DebuggerLanguageUtils
    {

        public static DebuggerLanguage GetDebuggerLanguage(DTE2 dte)
        {
            try
            {
                string language = dte.ActiveDocument.Language;
                switch (language.ToLowerInvariant())
                {
                    case "c/c++":
                        return DebuggerLanguage.CPlusPlus;
                    case "csharp":
                        return DebuggerLanguage.CSharp;
                    case "typescript":
                        return DebuggerLanguage.TypeScript;
                    case "javascript":
                        return DebuggerLanguage.JavaScript;
                    case "python":
                        return DebuggerLanguage.Python;
                    default:
                        if (language.StartsWith("asp", StringComparison.OrdinalIgnoreCase))
                            return DebuggerLanguage.AspNet;
                        return DebuggerLanguage.Unknown;
                }
            }
            catch (Exception)
            {
                return DebuggerLanguage.Unknown;
            }
        }
    }
}
