using EnvDTE;
using System.IO;

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