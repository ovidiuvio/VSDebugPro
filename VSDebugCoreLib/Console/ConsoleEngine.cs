using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

using VSDebugCoreLib.Commands;

namespace VSDebugCoreLib.Console
{
    public class ConsoleEngine
    {
        protected VSDebugContext Context { get; private set; }

        private ICollection<BaseCommand> _commands { get; set; }

        private Dictionary<string, IConsoleCommand> _commandsMap = new Dictionary<string, IConsoleCommand>();

        public ICollection<BaseCommand> Commands => _commands;

        public ConsoleEngine(VSDebugContext context, ICollection<BaseCommand> commands)
        {
            Context = context;
            _commands = commands;
        }

        public IConsoleCommand FindCommand(string CommandString)
        {
            IConsoleCommand command = null;
            _commandsMap.TryGetValue(CommandString, out command);

            if (command == null)
            {
                
            }

            return command;
        }

        public void Execute(string text)
        {
            try
            {               

                char[] sp = new char[] { ' ', '\t' };
                string[] argv = text.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);
                string alias = null;

                if (argv.Length == 0)
                    return;

                // replace first argument with alias value
                if ( null != (alias = Context.Settings.Alias.FindAliasValue(argv[0])) )
                {
                    if (2 == argv.Length)
                    {
                        text = alias + " " + argv[1];
                    }
                    else if (1 == argv.Length)
                    {
                        text = alias;                        
                    }

                    argv = text.Split(sp, 2, StringSplitOptions.RemoveEmptyEntries);

                }

                IConsoleCommand command = FindCommand(argv[0]);

                if (command == null)
                {
                    string strError = "Command: " + "<" + argv[0] + ">" + " is not valid.";
                    Write(strError);                
                    return;
                }

                if (eCommandStatus.CommandStatus_Disabled == command.CommandStatus)
                {
                    string strError = "Command: " + "<" + argv[0] + ">" + " is not available at this time.";
                    Write(strError);

                    return;
                }

                if (argv.Length > 1)
                    command.Execute(argv[1]);
                else
                    command.Execute("");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(0, "Diag", e.ToString());
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

        public ITextBuffer StdOut { get; set; }

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
