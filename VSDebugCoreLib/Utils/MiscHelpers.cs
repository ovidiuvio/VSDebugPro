using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib.Utils
{
    public class MiscHelpers
    {
        public static string GetClickableFileName(string strFile)
        {
            return "<file://" + strFile + ">.";
        }

        public static void LaunchLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch (Exception)
            {
                // Do nothing if default application handler is not associated.
            }
        }
    }
}
