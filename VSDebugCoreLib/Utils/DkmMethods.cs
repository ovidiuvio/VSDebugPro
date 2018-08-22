using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Debugger;
using Microsoft.VisualStudio.Debugger.CallStack;

namespace VSDebugCoreLib.Utils
{
    public class DkmMethods
    {
        public static DkmProcess GetDkmProcess(StackFrame stackFrame)
        {
            if(null != stackFrame)
            {
                DkmStackFrame dkmStackFrame = DkmStackFrame.ExtractFromDTEObject(stackFrame);
                return dkmStackFrame.Process;
            }

            return null;
        }
    }
}
