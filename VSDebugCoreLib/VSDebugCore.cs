using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

using System.Reflection;

using VSDebugCoreLib.Console;
using VSDebugCoreLib.Commands;
using VSDebugCoreLib.Commands.Memory;
using VSDebugCoreLib.Commands.Core;
using VSDebugCoreLib.Commands.UI;
using System.ComponentModel.Design;

namespace VSDebugCoreLib
{
    public enum VSDStatus
    {
        VSD_ST_OK = 0,
        VSD_ST_FAIL = -1,
        VSD_ST_EXCEPTION = -2,
        VSD_ST_NOT_FOUND = -3,

        VSD_FORCE_DWORD = 0x7FFFFFFF
    };

    public class VSDebugContext
    {
        /// <summary>
        /// VSDebugTool commands list.
        /// </summary>
        private readonly ICollection<BaseCommand> _commands = new List<BaseCommand>();

        /// <summary>
        /// VS console window.
        /// </summary>
        private ConsoleWindow _console;

        /// <summary>
        /// VSDebugTool console engine.
        /// </summary>
        private ConsoleEngine _consoleEngine;


        /// <summary>
        /// VS IDE that is executing this context.
        /// </summary>
        private DTE2 _ide;
        /// <summary>
        /// Menu command service
        /// </summary>
        private OleMenuCommandService _menu;

        /// <summary>
        /// Settings.
        /// </summary>
        private SettingsManager _settingsManager;

        public VSDebugContext(Package package, Assembly assembly)
        {
            PACKAGE = package;
            VSDAssembly = assembly;
        }

        ~VSDebugContext()
        {

            _console.SaveHistory(_settingsManager.VSDSettings.CmdHistory);
            _settingsManager.SaveSettings();

        }

        public ICollection<BaseCommand> Commands => _commands;

        public ConsoleEngine CONSOLE => _consoleEngine;

        /// <summary>
        /// VS IDE that is executing this context.
        /// </summary>
        public DTE2 IDE
        {
            get => _ide; set => _ide = value;
        }

        /// <summary>
        /// Gets the menu command service.
        /// </summary>
        public OleMenuCommandService MenuCommandService
        {
            get => _menu; set => _menu = value;
        }

        public Package PACKAGE { get; private set; }

        public CSettings Settings => _settingsManager.VSDSettings;
        public Assembly VSDAssembly { get; private set; }

        public void Initialize()
        {
            // Initialize settings manager & load settings
            InitSettings();

            // Initialize console window
            InitConsoleTool();

            // Register commands
            RegisterCommands();
        }

        private void InitSettings()
        {
            _settingsManager = new SettingsManager();

            _settingsManager.LoadSettings();
        }

        private void InitConsoleTool()
        {
            ToolWindowPane consoleWnd = PACKAGE.FindToolWindow(typeof(ConsoleWindow), 0, true);
            if ((null == consoleWnd) || (null == consoleWnd.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }

            _console = (ConsoleWindow)consoleWnd;

            _console.LoadHistory(_settingsManager.VSDSettings.CmdHistory);

            _consoleEngine = new ConsoleEngine(this, _commands);

            _console.Engine = _consoleEngine;
        }


        private void RegisterBaseCommand(BaseCommand cmd)
        {
            _commands.Add(cmd);
        }
        private void RegisterCommands()
        {
            var menuService = MenuCommandService;
            if (menuService != null)
            {
                RegisterConsoleCommand(new HelpCommand(this));
                RegisterConsoleCommand(new AboutCommand(this));
                RegisterConsoleCommand(new AliasCommand(this));
                RegisterConsoleCommand(new OpenConsoleCommand(this));
                RegisterConsoleCommand(new SettingsCommand(this));
                RegisterConsoleCommand(new DumpMem(this));
                RegisterConsoleCommand(new LoadMem(this));
                RegisterConsoleCommand(new MemCpy(this));
                RegisterConsoleCommand(new MemSet(this));
                RegisterConsoleCommand(new MemDiff(this));
                RegisterConsoleCommand(new MemAlloc(this));
                RegisterConsoleCommand(new MemFree(this));
                RegisterBaseCommand(new ExploreWDCommand(this));

            }
        }

        private void RegisterConsoleCommand(BaseCommand cmd)
        {
            _commands.Add(cmd);
            _consoleEngine.AddCommand(cmd);
        }
    }
}
