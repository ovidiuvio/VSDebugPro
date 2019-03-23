using System;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace VSDebugCoreLib.Commands.UI
{
    public class ShellCommand : BaseCommand
    {
        public ShellCommand(VSDebugContext context, Guid guid, int cmdId, string strId = "")
            : base(context, cmdId, strId)
        {
            try
            {
                // Create the command for the menu item.
                var menuCommandId = new CommandID(guid, cmdId);
                var menuItem = new MenuCommand(MenuCallback, menuCommandId);
                context.MenuCommandService.AddCommand(menuItem);
            }
            catch (Exception e)
            {
                Debugger.Log(0, "Diag", e.ToString());
            }
        }

        public virtual void MenuCallback(object sender, EventArgs e)
        {
        }
    }
}