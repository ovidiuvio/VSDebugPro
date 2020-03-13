using System;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Core
{
    internal class AliasCommand : BaseCommand
    {
        public AliasCommand(VSDebugContext context)
            : base(context, (int) PkgCmdIDList.CmdIDAbout, Resources.CmdAliasString)
        {
            CommandDescription = Resources.CmdAliasDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <add/del/list> <name> <string>\n" +
                                "\tEX: " + CommandString +
                                " add imgcopy memcpy img1.data img0.data \"img0.height * img0.stride\" \n" +
                                "\tEX: " + CommandString + " del imgcopy\n" +
                                "\tEX: " + CommandString + " list\n";

            CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
        }

        public override ECommandStatus CommandStatus => CommandStatusFlag;

        public override void Execute(string[] args)
        {
            base.Execute(args);

            if (0 == args.Length)
            {
                Context.CONSOLE.Write(CommandHelp);
            }
            else if (1 == args.Length)
            {
                if ("list" == args[0])
                {
                    if (Context.Settings.Alias.AliasList.Values.Count > 0)
                    {
                        Context.CONSOLE.WriteSeparator();

                        foreach (var item in Context.Settings.Alias.AliasList.Values)
                        {
                            var text = string.Format("{0,15}\t{1}", item.Alias, item.Value);
                            foreach (var arg in item.Arguments)
                            {
                                if (arg.Contains(" ") || arg.Contains("\t"))
                                {
                                    text += string.Format(" \"{0}\"", arg);
                                }
                                else
                                {
                                    text += string.Format(" {0}", arg);
                                }
                            }
                            Context.CONSOLE.Write(text);
                        }
                    }
                    else
                    {
                        Context.CONSOLE.WriteSeparator();
                        Context.CONSOLE.Write("no aliases available");

                    }
                }
                else
                {
                    Context.CONSOLE.Write(CommandHelp);
                }
            }
            else if (2 == args.Length)
            {
                if ("del" == args[0])
                {
                    var alias = args[1];
                    if (null != Context.Settings.Alias.FindAlias(alias))
                    {
                        if (Context.Settings.Alias.DelAlias(alias))
                            Context.CONSOLE.Write("Deleted alias: " + alias + ".");
                    }
                    else
                    {
                        Context.CONSOLE.Write("Alias: " + alias + " not found.");
                    }
                }
                else
                {
                    Context.CONSOLE.Write(CommandHelp);
                }
            }
            else
            {
                var action = args[0];
                var alias = args[1];

                var aliasCmd = args[2];
                var aliasArgs = MiscHelpers.ShiftArray(args, 3);

                if ("add" == action)
                {
                    // prevent command hiding
                    if (null != Context.CONSOLE.FindCommand(alias))
                    {
                        Context.CONSOLE.Write("Unable to alias: " + alias + "!");
                        return;
                    }

                    if (null == Context.CONSOLE.FindCommand(aliasCmd))
                    {
                        Context.CONSOLE.Write("Command: " + "<" + aliasCmd + ">" + " is not valid.");
                        return;
                    }

                    if (null != Context.Settings.Alias.FindAlias(alias))
                    {
                        Context.CONSOLE.Write("Alias: " + alias + " is already defined.");
                        return;
                    }

                    if (Context.Settings.Alias.AddAlias(alias, aliasCmd, aliasArgs))
                    {
                        Context.CONSOLE.Write("Added alias: " + alias + ".");
                    }
                    else
                    {
                        Context.CONSOLE.Write("Unable to alias: " + alias + "!");
                    }
                }
                else
                {
                    Context.CONSOLE.Write(CommandHelp);
                }
            }
        }
    }
}
