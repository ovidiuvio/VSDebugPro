using System;
using VSDebugCoreLib.UI;

namespace VSDebugCoreLib.Commands.UI
{
    public class AboutCommand : ShellCommand
    {
        public AboutCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProAbout, (int) PkgCmdIDList.CmdIDAbout, Resources.AboutCommandString)
        {
            CommandDescription = Resources.CmdAboutDesc;
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            new AboutWindow(Context).ShowDialog();
        }

        public override void Execute(string[] args)
        {
            base.Execute(args);

            MenuCallback(this, EventArgs.Empty);
        }
    }
}
