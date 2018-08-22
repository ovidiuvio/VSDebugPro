using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EnvDTE;

namespace VSDebugCoreLib.Utils
{
    public class DebugHelpers
    {

        public static bool IsMiniDumpProcess(Process process)
        {
            string strExt = Path.GetExtension(process.Name.ToLower());

            if (".dmp" == strExt)
                return true;

            return false;
        }
    }
}
