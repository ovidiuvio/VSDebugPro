using System;
using System.Diagnostics;
using VSDebugCoreLib.UI;

namespace VSDebugCoreLib.Commands.UI
{
    public class BreakpointActionCommand : ShellCommand
    {
        public BreakpointActionCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProBreakpointAction, (int)PkgCmdIDList.CmdIDBreakpointAction)
        {
            CommandDescription = Resources.CmdAboutDesc;
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            new BreakpointActionWindow(Context).ShowDialog();
        }
    }
}