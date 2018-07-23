using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using VSDebugCoreLib.Console;

namespace VSDebugCoreLib.Commands
{
    public class OpenConsoleCommand : ShellCommand
    {
        public override void MenuCallback(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = Context.PACKAGE.FindToolWindow(typeof(ConsoleWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public OpenConsoleCommand(VSDebugContext context)
            :base(context, GuidList.GuidVSDebugProConsole, (int)PkgCmdIDList.CmdIDConsole)
        {
            
        }
    }
}
