using System;

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
                                " add imgcopy memcpy img1.data img0.data img0.height * img0.stride \n" +
                                "\tEX: " + CommandString + " del imgcopy\n" +
                                "\tEX: " + CommandString + " list\n";

            CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
        }

        public override ECommandStatus CommandStatus => CommandStatusFlag;

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = {' ', '\t'};
            var argv = text.Split(sp, 3, StringSplitOptions.RemoveEmptyEntries);
            var bRes = false;
            string alias = null;
            string value = null;

            switch (argv.Length)
            {
                case 0:
                    Context.ConsoleEngine.Write(CommandHelp);
                    return;

                case 1:
                {
                    if ("list" == argv[0])
                    {
                        if (Context.Settings.Alias.AliasList.Values.Count > 0)
                        {
                            Context.ConsoleEngine.WriteSeparator();

                            foreach (var item in Context.Settings.Alias.AliasList.Values)
                                Context.ConsoleEngine.Write(string.Format("{0,15}\t{1}", item.Item1, item.Item2));
                        }
                    }
                    else
                    {
                        Context.ConsoleEngine.Write(CommandHelp);
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
                                Context.ConsoleEngine.Write("Deleted alias: " + alias + ".");
                        }
                        else
                        {
                            Context.ConsoleEngine.Write("Alias: " + alias + " not found.");
                        }
                    }
                    else
                    {
                        Context.ConsoleEngine.Write(CommandHelp);
                    }
                }
                    return;

                case 3:
                {
                    alias = argv[1];
                    value = argv[2];

                    var aliasargv = value.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                    var aliascmd = aliasargv[0];

                    if ("add" == argv[0])
                    {
                        // prevent command hiding
                        if (null != Context.ConsoleEngine.FindCommand(alias))
                        {
                            Context.ConsoleEngine.Write("Unable to alias: " + alias + "!");
                            break;
                        }

                        if (null == Context.ConsoleEngine.FindCommand(aliascmd))
                        {
                            bRes = false;
                            Context.ConsoleEngine.Write("Command: " + "<" + aliascmd + ">" + " is not valid.");
                            break;
                        }

                        if (null == Context.Settings.Alias.FindAlias(alias))
                        {
                            bRes = Context.Settings.Alias.AddAlias(alias, value);
                        }
                        else
                        {
                            bRes = false;
                            Context.ConsoleEngine.Write("Alias: " + alias + " is already defined.");
                            break;
                        }

                        if (bRes)
                            Context.ConsoleEngine.Write("Added alias: " + alias + ".");
                        else
                            Context.ConsoleEngine.Write("Unable to alias: " + alias + "!");
                    }
                    else
                    {
                        Context.ConsoleEngine.Write(CommandHelp);
                    }
                }
                    return;
            }
        }
    }
}