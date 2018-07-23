using System;
using VSDebugCoreLib.UI;

namespace VSDebugCoreLib.Commands.UI
{
    public class SettingsCommand : ShellCommand
    {
        public override void MenuCallback(object sender, EventArgs e)
        {
            new SettingsWindow( Context ).ShowDialog();
        }

        public SettingsCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProSettings, (int)PkgCmdIDList.CmdIDSettings, Resources.CmdSettingsString)
        {
            CommandDescription = Resources.CmdSettingsDesc;
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            MenuCallback(this, EventArgs.Empty);
        }


    }
}
