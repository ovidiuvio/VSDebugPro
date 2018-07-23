using System;
using System.ComponentModel.Design;

namespace VSDebugCoreLib.Commands
{
    public class ShellCommand : BaseCommand
    {
        public ShellCommand(VSDebugContext context, Guid guid, int cmdID, string strID = "" )
            : base(context, cmdID, strID)
        {
            try
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(guid, cmdID);
                MenuCommand menuItem = new MenuCommand(MenuCallback, menuCommandID);
                context.MenuCommandService.AddCommand(menuItem);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debugger.Log(0, "Diag", e.ToString());
            }
        }

        public virtual void MenuCallback(object sender, EventArgs e)
        {

        }

    }
}
