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

        public override void Execute(string[] args)
        {
            base.Execute(args);

            if (0 == args.Length)
            {
                Context.CONSOLE.Write("For more information on a specific command, type help command-name");
                Context.CONSOLE.WriteSeparator();

                // list all commands
                foreach (var item in Context.CONSOLE.Commands)
                    if (item.CommandString != string.Empty)
                        Context.CONSOLE.Write(string.Format("{0,10}\t{1,10}", item.CommandString, item.CommandInfo));

                Context.CONSOLE.WriteSeparator();
            }
            else
            {
                var command = Context.CONSOLE.FindCommand(args[0]);

                if (null != command)
                {
                    Context.CONSOLE.Write(command.CommandString);
                    Context.CONSOLE.Write(command.CommandInfo);

                    if (command.CommandHelp != string.Empty)
                    {
                        Context.CONSOLE.WriteSeparator();

                        Context.CONSOLE.Write(command.CommandHelp);

                        Context.CONSOLE.WriteSeparator();
                    }
                }
                else
                {
                    Context.CONSOLE.Write("Command: " + "<" + args[0] + ">" + " is not valid.");
                }
            }
        }
    }
}
