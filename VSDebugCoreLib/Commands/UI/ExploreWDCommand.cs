using System;
using System.Diagnostics;

namespace VSDebugCoreLib.Commands.UI
{
    public class ExploreWdCommand : ShellCommand
    {
        public ExploreWdCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProExploreWD, (int) PkgCmdIDList.cmdIDExploreWD)
        {
            CommandDescription = Resources.CmdAboutDesc;
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            var windir = Environment.GetEnvironmentVariable("WINDIR");

            Process.Start(windir + "\\explorer.exe", Context.Settings.GeneralSettings.WorkingDirectory);
        }
    }
}