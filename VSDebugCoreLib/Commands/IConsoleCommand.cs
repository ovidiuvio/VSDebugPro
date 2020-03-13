namespace VSDebugCoreLib.Commands
{
    /// <summary>
    ///     Command status
    /// </summary>
    public enum ECommandStatus
    {
        CommandStatusDisabled = 0,
        CommandStatusEnabled,
        CommandStatusNaMiniDump
    }

    /// <summary>
    ///     Console Command interface
    /// </summary>
    public interface IConsoleCommand
    {
        /// <summary>
        ///     Unique string identifier for the command.
        /// </summary>
        string CommandString { get; }

        /// <summary>
        ///     Command help text
        /// </summary>
        string CommandHelp { get; }

        /// <summary>
        ///     Command description
        /// </summary>
        string CommandInfo { get; }

        /// <summary>
        ///     Command help text
        /// </summary>
        ECommandStatus CommandStatus { get; }

        /// <summary>
        ///     Execute function
        /// </summary>
        void Execute(string[] args);
    }
}
