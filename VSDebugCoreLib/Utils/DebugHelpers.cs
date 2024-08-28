using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
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

        public static DebuggerExpressionEvaluator CreateEvaluator(DTE2 dte)
        {
            DebuggerLanguage language = DebuggerLanguageUtils.GetDebuggerLanguage(dte);

            switch (language)
            {
                case DebuggerLanguage.CPlusPlus:
                    return new CppDebuggerExpressionEvaluator(dte);
                // Add other language-specific evaluators here as they are implemented
                default:
                    return new DefaultDebuggerExpressionEvaluator(dte);
            }
        }
    }
}