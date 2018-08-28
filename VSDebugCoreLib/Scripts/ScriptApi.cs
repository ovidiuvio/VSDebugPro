using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib.Scripting
{
    public class VSDScriptApi
    {
        public const string strApiInit = "__vsd_init__";
        public const string strApiExit = "__vsd_exit__";
        public const string strApiSelf = "__vsd_self__";

        static public void VsdPrint(VSDScriptObject self, string text)
        {
            self.Context.CONSOLE.Write(text);
        }

        static public void VsdConsoleExec(VSDScriptObject self, string cmd)
        {
            self.Context.CONSOLE.Execute(cmd);
        }
    }
}
