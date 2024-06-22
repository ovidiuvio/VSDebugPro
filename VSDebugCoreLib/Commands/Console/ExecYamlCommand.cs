using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Stubble.Core.Builders;

namespace VSDebugCoreLib.Commands.Core
{
    public class CommandFile
    {
        public Dictionary<string, string> Variables { get; set; }
        public List<string> Commands { get; set; }
    }

    internal class ExecuteYamlCommand : BaseCommand
    {
        public ExecuteYamlCommand(VSDebugContext context)
            : base(context, (int)PkgCmdIDList.cmdIDExecuteYaml, "exec")
        {
            CommandDescription = "Executes commands from a specified YAML file with Mustache templating.";
            CommandHelpString = "Syntax: <" + CommandString + "> <yamlFilePath> <arg1> <arg2> ... <argN>\n" +
                                "\tEX: " + CommandString + " commands.yaml \"value1\" \"value2\"\n";
            CommandStatusFlag = ECommandStatus.CommandStatusEnabled;
        }

        public override void Execute(string text)
        {
            base.Execute(text);

            char[] sp = { ' ', '\t' };
            var argv = text.Split(sp, StringSplitOptions.RemoveEmptyEntries);

            if (argv.Length < 1)
            {
                Context.ConsoleEngine.Write(CommandHelpString);
                return;
            }

            var yamlFilePath = argv[0];

            string[] args;
            if (argv.Length > 1)
            {
                args = new string[argv.Length - 1];
                Array.Copy(argv, 1, args, 0, argv.Length - 1);
            }
            else
            {
                args = new string[0];
            }

            if (!File.Exists(yamlFilePath))
            {
                Context.ConsoleEngine.Write($"File not found: {yamlFilePath}");
                return;
            }

            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var commandFile = deserializer.Deserialize<CommandFile>(File.ReadAllText(yamlFilePath));

                var stubble = new StubbleBuilder().Build();
                var templateDictionary = new Dictionary<string, object>();
                foreach (var kvp in commandFile.Variables)
                {
                    templateDictionary.Add(kvp.Key, kvp.Value);
                }

                // Map command-line arguments to $1, $2, $3, etc.
                for (int i = 0; i < args.Length; i++)
                {
                    templateDictionary[$"${i + 1}"] = args[i];
                }

                // Render variables first
                var renderedVariables = new Dictionary<string, string>();
                foreach (var kvp in commandFile.Variables)
                {
                    renderedVariables[kvp.Key] = stubble.Render(kvp.Value, templateDictionary);
                }

                // Use the rendered variables for command templating
                foreach (var commandEntry in commandFile.Commands)
                {
                    var renderedArguments = stubble.Render(commandEntry, renderedVariables);
                    var commandText = $"{renderedArguments}";
                    Context.ConsoleEngine.Execute(commandText);
                }

                Context.ConsoleEngine.Write("Executed all commands from the YAML file.");
            }
            catch (Exception ex)
            {
                Context.ConsoleEngine.Write($"Error executing commands from YAML file: {ex.Message}");
            }
        }
    }
}
