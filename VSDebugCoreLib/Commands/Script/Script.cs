using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSDebugCoreLib.Commands.Script
{
    class ScriptCommand : BaseCommand
    {
        public ScriptCommand(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdScriptString)
        {
            CommandDescription = Resources.CmdScriptDesc;

            CommandHelpString = "";

            CommandStatusFlag = eCommandStatus.CommandStatus_Enabled;
        }

        public override eCommandStatus CommandStatus => CommandStatusFlag;

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = new char[] { ' ', '\t' };
            string[] argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);

            switch (argv.Length)
            {
                case 0:
                    Context.CONSOLE.Write(CommandHelp);
                    return;
                case 1:
                    {
                        if ("list" == argv[0])
                        {
                           
                        }
                        else if ("reload-all" == argv[0])
                        {
                            Context.ScriptEngine.ReloadScripts(Context.Settings.GeneralSettings.WorkingDirectory);
                        }
                        else
                        {
                            Context.CONSOLE.Write(CommandHelp);
                        }
                    }
                    return;
            }

        }
    }
}
