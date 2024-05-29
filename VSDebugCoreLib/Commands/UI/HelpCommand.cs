using System;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.UI
{
    public class HelpCommand : ShellCommand
    {
        public HelpCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProHelp, (int) PkgCmdIDList.CmdIDHelp, Resources.HelpCmdString)
        {
            CommandDescription = Resources.CmdHelpDesc;
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            MiscHelpers.LaunchLink(@"http://" + Resources.HelpWebsite);
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = {' ', '\t'};
            var argv = text.Split(sp, 2);

            if (1 == argv.Length && argv[0] == string.Empty)
            {
                Context.ConsoleEngine.Write("For more information on a specific command, type help command-name");
                Context.ConsoleEngine.WriteSeparator();

                // list all commands
                foreach (var item in Context.ConsoleEngine.Commands)
                    if (item.CommandString != string.Empty)
                        Context.ConsoleEngine.Write(string.Format("{0,10}\t{1,10}", item.CommandString, item.CommandInfo));

                Context.ConsoleEngine.WriteSeparator();
            }
            else if (argv.Length >= 1 && argv[0] != string.Empty)
            {
                var command = Context.ConsoleEngine.FindCommand(argv[0]);

                if (null != command)
                {
                    Context.ConsoleEngine.Write(command.CommandString);
                    Context.ConsoleEngine.Write(command.CommandInfo);

                    if (command.CommandHelp != string.Empty)
                    {
                        Context.ConsoleEngine.WriteSeparator();

                        Context.ConsoleEngine.Write(command.CommandHelp);

                        Context.ConsoleEngine.WriteSeparator();
                    }
                }
                else
                {
                    Context.ConsoleEngine.Write("Command: " + "<" + argv[0] + ">" + " is not valid.");
                }
            }
        }
    }
}