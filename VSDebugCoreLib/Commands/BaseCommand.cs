namespace VSDebugCoreLib.Commands
{
    public class BaseCommand : IConsoleCommand
    {
        protected string CommandStringID;
        protected string CommandHelpString;
        protected string CommandDescription;
        protected eCommandStatus CommandStatusFlag;

        protected VSDebugContext Context { get; private set; }
        public int CommandID { get; private set; }
        public string CommandString => CommandStringID;
        public string CommandHelp => CommandHelpString;
        public string CommandInfo => CommandDescription;
        public virtual eCommandStatus CommandStatus => CommandStatusFlag;

        public BaseCommand(VSDebugContext context, int cmdID, string strID)
        {
            Context = context;
            CommandID = cmdID;
            CommandStringID = strID;
            CommandHelpString = string.Empty;
            CommandDescription = string.Empty;
            CommandStatusFlag = eCommandStatus.CommandStatus_Enabled;
        }

        public virtual void Execute(string text)
        {
        }
    }
}