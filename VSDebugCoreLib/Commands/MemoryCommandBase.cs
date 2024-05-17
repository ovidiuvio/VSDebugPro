using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.Commands.Memory
{
    public abstract class MemoryCommandBase : BaseCommand
    {
        protected MemoryCommandBase(VSDebugContext context, int cmdId, string cmdString)
            : base(context, cmdId, cmdString)
        {
            CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
        }

        public override ECommandStatus CommandStatus
        {
            get
            {
                if (Context.IDE.Debugger != null && Context.IDE.Debugger.DebuggedProcesses != null &&
                    Context.IDE.Debugger.DebuggedProcesses.Count > 0)
                {
                    CommandStatusFlag = DebugHelpers.IsMiniDumpProcess(Context.IDE.Debugger.CurrentProcess)
                        ? ECommandStatus.CommandStatusNaMiniDump
                        : ECommandStatus.CommandStatusEnabled;
                }
                else
                {
                    CommandStatusFlag = ECommandStatus.CommandStatusDisabled;
                }

                return CommandStatusFlag;
            }
        }
    }
}
