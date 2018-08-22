namespace VSDebugCoreLib.Commands
{
    /// <summary>
    /// Command status
    /// </summary>
    public enum eCommandStatus
    {
        CommandStatus_Disabled  = 0,
        CommandStatus_Enabled,
        CommandStatus_NA_MiniDump,
    }

    /// <summary>
    /// Console Command interface
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        /// Unique string identifier for the command.
        /// </summary>
        string CommandString { get; }

        /// <summary>
        /// Command help text
        /// </summary>
        string CommandHelp   { get; }

        // <summary>
        /// Command description
        /// </summary>
        string CommandInfo   { get; }

        /// <summary>
        /// Command help text
        /// </summary>
        eCommandStatus CommandStatus { get; }

        /// <summary>
        /// Execute function
        /// </summary>
        void Execute(string text);


        

    }
}
