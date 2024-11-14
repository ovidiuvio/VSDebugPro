using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VSDebugCoreLib.Commands;
using VSDebugCoreLib.Commands.Core;
using VSDebugCoreLib.Commands.Memory;
using VSDebugCoreLib.Commands.Debugging;
using VSDebugCoreLib.Commands.UI;
using VSDebugCoreLib.Console;
using VSDebugCoreLib.UI.Tools;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib
{
    public enum VsdStatus
    {
        VsdStOk = 0,
        VsdStFail = -1,
        VsdStException = -2,
        VsdStNotFound = -3,

        VsdForceDword = 0x7FFFFFFF
    }

    public class VSDebugContext
    {
        /// <summary>
        ///     VSDebugTool commands list.
        /// </summary>
        private readonly ICollection<BaseCommand> _commands = new List<BaseCommand>();

        /// <summary>
        ///     VSDebugTool console engine.
        /// </summary>
        private ConsoleEngine _consoleEngine;

        /// <summary>
        ///     VS IDE that is executing this context.
        /// </summary>
        private DTE2 _ide;

        /// <summary>
        ///     Menu command service
        /// </summary>
        private OleMenuCommandService _menu;

        /// <summary>
        ///     Settings.
        /// </summary>
        private SettingsManager _settingsManager;

        public VSDebugContext(Package package, Assembly assembly)
        {
            PACKAGE = package;
            VSDAssembly = assembly;
        }

        public ICollection<BaseCommand> Commands => _commands;

        public ConsoleEngine ConsoleEngine => _consoleEngine;

        public ConsoleWindow Console { get; set; }

        /// <summary>
        ///     VS IDE that is executing this context.
        /// </summary>
        public DTE2 IDE
        {
            get => _ide;
            set => _ide = value;
        }

        /// <summary>
        ///     Gets the menu command service.
        /// </summary>
        public OleMenuCommandService MenuCommandService
        {
            get => _menu;
            set => _menu = value;
        }

        public Package PACKAGE { get; }

        public CSettings Settings => _settingsManager.VSDSettings;
        public SettingsManager SettingsManager => _settingsManager;
        public Assembly VSDAssembly { get; }

        public void Initialize()
        {   
            // Initialize app data
            InitAppData();

            // Initialize settings manager & load settings
            InitSettings();

            // Initialize console window
            InitConsoleEngine();

            // Register commands
            InitCommands();
        }

        private void InitAppData() 
        {
            // Check if we need to create app folder
            if (!Directory.Exists(MiscHelpers.GetApplicationDataPath()))
                Directory.CreateDirectory(MiscHelpers.GetApplicationDataPath());
        }

        private void InitSettings()
        {
            _settingsManager = new SettingsManager();

            _settingsManager.LoadSettings();
        }

        private void InitConsoleEngine()
        { 
            _consoleEngine = new ConsoleEngine(this, _commands);
        }

        private void RegisterBaseCommand(BaseCommand cmd)
        {
            _commands.Add(cmd);
        }

        private void InitCommands()
        {
            var menuService = MenuCommandService;
            if (menuService != null)
            {
                RegisterConsoleCommand(new HelpCommand(this));
                RegisterConsoleCommand(new AboutCommand(this));
                RegisterConsoleCommand(new AliasCommand(this));
                RegisterConsoleCommand(new OpenConsoleCommand(this));
                RegisterConsoleCommand(new SettingsCommand(this));
                RegisterConsoleCommand(new StackWalk(this));
                RegisterConsoleCommand(new MemDump(this));
                RegisterConsoleCommand(new MemLoad(this));
                RegisterConsoleCommand(new HexDump(this));
                RegisterConsoleCommand(new MemCpy(this));
                RegisterConsoleCommand(new MemSet(this));
                RegisterConsoleCommand(new MemDiff(this));
                RegisterConsoleCommand(new MemAlloc(this));
                RegisterConsoleCommand(new MemFree(this));
                RegisterConsoleCommand(new PrintSymbol(this));
                RegisterConsoleCommand(new ExportSymbol(this));
                RegisterConsoleCommand(new ExecuteYamlCommand(this));
                RegisterBaseCommand(new ExploreWdCommand(this));
                RegisterBaseCommand(new RepeatCommand(this));
                RegisterBaseCommand(new ExportHistoryCommand(this));           
            }
        }

        private void RegisterConsoleCommand(BaseCommand cmd)
        {
            _commands.Add(cmd);
            _consoleEngine.AddCommand(cmd);
        }
    }
}