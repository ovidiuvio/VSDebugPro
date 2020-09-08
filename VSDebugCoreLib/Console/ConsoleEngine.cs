using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using VSDebugCoreLib.Commands;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Console
{
    public class ConsoleEngine
    {
        private readonly Dictionary<string, IConsoleCommand> _commandsMap = new Dictionary<string, IConsoleCommand>();

        public ConsoleEngine(VSDebugContext context, ICollection<BaseCommand> commands)
        {
            Context = context;
            _commands = commands;
        }

        protected VSDebugContext Context { get; }

        private ICollection<BaseCommand> _commands { get; }

        public ICollection<BaseCommand> Commands => _commands;

        public ITextBuffer StdOut { get; set; }

        public IConsoleCommand FindCommand(string CommandString)
        {
            _commandsMap.TryGetValue(CommandString, out var command);

            if (command == null)
            {
            }

            return command;
        }

        public void Execute(string text)
        {
            try
            {
                var args = MiscHelpers.ParseCommand(text);

                if (args.Length == 0)
                    return;

                var commandStr = args[0];
                var argv = MiscHelpers.ShiftArray(args);

                string alias;
                if (null != (alias = Context.Settings.Alias.FindAliasCommand(commandStr)))
                {
                    var aliasArgs = Context.Settings.Alias.FindAliasArguments(commandStr);
                    commandStr = alias;

                    var newArgv = new string[argv.Length + aliasArgs.Length];
                    // first saved args, then additional ones
                    Array.Copy(aliasArgs, newArgv, aliasArgs.Length);
                    Array.Copy(argv, 0, newArgv, aliasArgs.Length, argv.Length);
                    argv = newArgv;
                }

                var command = FindCommand(commandStr);
                if (command == null)
                {
                    var strError = "Command: " + "<" + commandStr + ">" + " is not valid.";
                    Write(strError);
                    return;
                }

                if (ECommandStatus.CommandStatusDisabled == command.CommandStatus)
                {
                    var strError = "Command: " + "<" + commandStr + ">" + " is only available while debugging.";
                    Write(strError);

                    return;
                }

                if (ECommandStatus.CommandStatusNaMiniDump == command.CommandStatus)
                {
                    var strError = "Command: " + "<" + commandStr + ">" + " is not available for minidumps.";
                    Write(strError);

                    return;
                }

                command.Execute(argv);
            }
            catch (Exception e)
            {
                Debugger.Log(0, "Diag", e.ToString());

                try
                {
                    Write("Command Exception:" + e.Message);
                    Write(e.StackTrace);
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, "Diag", ex.ToString());
                }
            }
        }

        public void Write(string text)
        {
            text += "\n";

            StdOut.Insert(StdOut.CurrentSnapshot.Length, text);
        }

        public void WriteSeparator()
        {
            Write("------------------------------------------------------------------");
        }

        public bool AddCommand(IConsoleCommand cmd)
        {
            IConsoleCommand command = null;
            _commandsMap.TryGetValue(cmd.CommandString, out command);

            if (command == null)
            {
                _commandsMap.Add(cmd.CommandString, cmd);

                return true;
            }

            return false;
        }
    }
}
