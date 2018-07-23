using System;
using VSDebugCoreLib.UI;

namespace VSDebugCoreLib.Commands
{
    public class AboutCommand : ShellCommand 
    {
        public override void MenuCallback(object sender, EventArgs e)
        {
            new AboutWindow( Context ).ShowDialog();
        }

        public AboutCommand(VSDebugContext context)
            :base(context, GuidList.GuidVSDebugProAbout, (int)PkgCmdIDList.CmdIDAbout, Resources.AboutCommandString )
        {
            CommandDescription = Resources.CmdAboutDesc;
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            MenuCallback(this, EventArgs.Empty);
        }

        
    }
}
