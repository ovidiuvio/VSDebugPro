using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using VSDebugCoreLib.Console;

namespace VSDebugCoreLib.Commands.UI
{
    public class OpenConsoleCommand : ShellCommand
    {
        public OpenConsoleCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProConsole, (int) PkgCmdIDList.CmdIDConsole)
        {
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = Context.PACKAGE.FindToolWindow(typeof(ConsoleWindow), 0, true);
            if (null == window || null == window.Frame) throw new NotSupportedException(Resources.CanNotCreateWindow);
            var windowFrame = (IVsWindowFrame) window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}