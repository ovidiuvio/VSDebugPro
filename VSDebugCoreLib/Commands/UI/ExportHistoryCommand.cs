using System;
using System.Diagnostics;

namespace VSDebugCoreLib.Commands.UI
{
    public class ExportHistoryCommand : ShellCommand
    {
        public ExportHistoryCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProExportHistory, (int) PkgCmdIDList.cmdIDExportHistory)
        {
            CommandDescription = "repeat last console command";
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            
        }
    }
}