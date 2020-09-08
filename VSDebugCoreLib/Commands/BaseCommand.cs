using System.Collections.Generic;

namespace VSDebugCoreLib.Commands
{
    public class BaseCommand : IConsoleCommand
    {
        protected readonly string CommandStringId;
        protected string CommandDescription;
        protected string CommandHelpString;
        protected ECommandStatus CommandStatusFlag;
        protected string[] CommandArguments;

        public BaseCommand(VSDebugContext context, int cmdId, string strId)
        {
            Context = context;
            CommandId = cmdId;
            CommandStringId = strId;
            CommandHelpString = string.Empty;
            CommandDescription = string.Empty;
            CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
        }

        protected VSDebugContext Context { get; }
        public int CommandId { get; }
        public string CommandString => CommandStringId;
        public string CommandHelp => CommandHelpString;
        public string CommandInfo => CommandDescription;
        public virtual ECommandStatus CommandStatus => CommandStatusFlag;

        public virtual void Execute(string[] args)
        {
        }
    }
}
