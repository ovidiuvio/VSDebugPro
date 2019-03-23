using System;

namespace VSDebugCoreLib.Commands.Core
{
    internal class AliasCommand : BaseCommand
    {
        public AliasCommand(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.CmdIDAbout, Resources.CmdAliasString)
        {
            CommandDescription = Resources.CmdAliasDesc;

            CommandHelpString = "Syntax: <" + CommandString + ">" + " <add/del/list> <name> <string>\n" +
                                "\tEX: " + CommandString + " add imgcopy memcpy img1.data img0.data img0.height * img0.stride \n" +
                                "\tEX: " + CommandString + " del imgcopy\n" +
                                "\tEX: " + CommandString + " list\n";

            CommandStatusFlag = eCommandStatus.CommandStatus_Enabled;
        }

        public override eCommandStatus CommandStatus => CommandStatusFlag;

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = new char[] { ' ', '\t' };
            string[] argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);
            bool bRes = false;
            string alias = null;
            string value = null;

            switch (argv.Length)
            {
                case 0:
                    Context.CONSOLE.Write(CommandHelp);
                    return;

                case 1:
                    {
                        if ("list" == argv[0])
                        {
                            if (Context.Settings.Alias.AliasList.Values.Count > 0)
                            {
                                Context.CONSOLE.WriteSeparator();

                                foreach (var item in Context.Settings.Alias.AliasList.Values)
                                {
                                    Context.CONSOLE.Write(String.Format("{0,15}\t{1}", item.Alias, item.Value));
                                }
                            }
                        }
                        else
                        {
                            Context.CONSOLE.Write(CommandHelp);
                        }
                    }
                    return;

                case 2:
                    {
                        alias = argv[1];

                        if ("del" == argv[0])
                        {
                            if (null != Context.Settings.Alias.FindAlias(alias))
                            {
                                bRes = Context.Settings.Alias.DelAlias(alias);
                                if (bRes)
                                    Context.CONSOLE.Write("Deleted alias: " + alias + ".");
                            }
                            else
                                Context.CONSOLE.Write("Alias: " + alias + " not found.");
                        }
                        else
                        {
                            Context.CONSOLE.Write(CommandHelp);
                        }
                    }
                    return;

                case 3:
                    {
                        alias = argv[1];
                        value = argv[2];

                        string[] aliasargv = value.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                        string aliascmd = aliasargv[0];

                        if ("add" == argv[0])
                        {
                            // prevent command hiding
                            if (null != Context.CONSOLE.FindCommand(alias))
                            {
                                Context.CONSOLE.Write("Unable to alias: " + alias + "!");
                                break;
                            }

                            if (null == Context.CONSOLE.FindCommand(aliascmd))
                            {
                                bRes = false;
                                Context.CONSOLE.Write("Command: " + "<" + aliascmd + ">" + " is not valid.");
                                break;
                            }

                            if (null == Context.Settings.Alias.FindAlias(alias))
                            {
                                bRes = Context.Settings.Alias.AddAlias(alias, value);
                            }
                            else
                            {
                                bRes = false;
                                Context.CONSOLE.Write("Alias: " + alias + " is already defined.");
                                break;
                            }

                            if (bRes)
                                Context.CONSOLE.Write("Added alias: " + alias + ".");
                            else
                                Context.CONSOLE.Write("Unable to alias: " + alias + "!");
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