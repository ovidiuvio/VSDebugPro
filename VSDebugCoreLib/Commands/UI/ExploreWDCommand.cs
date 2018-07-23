using System;
using System.Diagnostics;

namespace VSDebugCoreLib.Commands.UI
{
    public class ExploreWDCommand : ShellCommand
    {
        public override void MenuCallback(object sender, EventArgs e)
        {
            string windir = Environment.GetEnvironmentVariable("WINDIR");

            Process.Start( windir + "\\explorer.exe", Context.Settings.GeneralSettings.WorkingDirectory );
        }

        public ExploreWDCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProExploreWD, (int)PkgCmdIDList.cmdIDExploreWD)
        {
            CommandDescription = Resources.CmdAboutDesc;
        }

    }
}
