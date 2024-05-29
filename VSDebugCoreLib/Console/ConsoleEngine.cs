using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;
using VSDebugCoreLib.Commands;

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

        private string _lastValidInput;

        protected VSDebugContext Context { get; }

        private ICollection<BaseCommand> _commands { get; }

        public string LastValidInput { get => _lastValidInput; }

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
                char[] sp = {' ', '\t'};
                var argv = text.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                string alias = null;

                if (argv.Length == 0)
                    return;

                // replace first argument with alias value
                if (null != (alias = Context.Settings.Alias.FindAliasValue(argv[0])))
                {
                    if (2 == argv.Length)
                        text = alias + " " + argv[1];
                    else if (1 == argv.Length) text = alias;

                    argv = text.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                }

                var command = FindCommand(argv[0]);

                if (command == null)
                {
                    var strError = "Command: " + "<" + argv[0] + ">" + " is not valid.";
                    Write(strError);
                    return;
                }

                if (ECommandStatus.CommandStatusDisabled == command.CommandStatus)
                {
                    var strError = "Command: " + "<" + argv[0] + ">" + " is only available while debugging.";
                    Write(strError);

                    return;
                }

                if (ECommandStatus.CommandStatusNaMiniDump == command.CommandStatus)
                {
                    var strError = "Command: " + "<" + argv[0] + ">" + " is not available for minidumps.";
                    Write(strError);

                    return;
                }

                if (argv.Length > 1)
                {
                    command.Execute(argv[1]);
                    _lastValidInput = text;
                }
                else
                    command.Execute("");
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

        public void ExecuteLast()
        {
            if (_lastValidInput != null)
            {
                Execute(_lastValidInput);
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