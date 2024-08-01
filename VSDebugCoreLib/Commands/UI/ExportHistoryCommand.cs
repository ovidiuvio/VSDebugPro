using System;
using System.Diagnostics;
using System.IO;
using VSDebugCoreLib.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VSDebugCoreLib.Commands.UI
{
    public class ExportHistoryCommand : ShellCommand
    {
        public ExportHistoryCommand(VSDebugContext context)
            : base(context, GuidList.GuidVSDebugProExportHistory, (int) PkgCmdIDList.CmdIDExportHistory)
        {
            CommandDescription = "Export command history as a YAML file.";
        }

        public override void MenuCallback(object sender, EventArgs e)
        {
            try
            {
                var cmdHistory = Context.Settings.CmdHistory;
                var workingDirectory = Context.Settings.GeneralSettings.WorkingDirectory;

                // Create a Serializer with naming convention for YAML
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                // Serialize the command history to YAML
                var yaml = serializer.Serialize(cmdHistory.Values);

                // define the filename
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"command_history_{timestamp}.yaml";
                var filePath = Path.Combine(workingDirectory, fileName);

                // WriteInfo the YAML to the file
                File.WriteAllText(filePath, yaml);

                Context.Console.WriteInfo($"Command history exported to: {MiscHelpers.GetClickableFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Context.Console.WriteInfo($"Error exporting history: {ex.Message}");
            }
        }
    }
}