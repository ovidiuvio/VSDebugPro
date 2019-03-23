using System.IO;
using EnvDTE;

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
}