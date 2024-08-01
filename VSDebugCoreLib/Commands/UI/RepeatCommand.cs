using System;
using System.Diagnostics;

namespace VSDebugCoreLib.Commands.UI
{
    public class RepeatCommand : ShellCommand
    {
        public RepeatCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProRepeatCmd, (int) PkgCmdIDList.CmdIDRepeatCmd)
        {
            CommandDescription = "repeat last console command";
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            Context.Console.ExecuteLast();
        }
    }
}