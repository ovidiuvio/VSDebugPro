using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using VSDebugCoreLib.Properties;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CExtensionMapElement
    {
        public CExtensionMapElement()
        {
        }

        public CExtensionMapElement(string ext, string tool)
        {
            Extension = ext;
            Tool = tool;
        }

        public string Extension { get; set; }
        public string Tool { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CExtensionsMap
    {
        public CExtensionsMap()
        {
            Values = new List<CExtensionMapElement>();
        }

        public List<CExtensionMapElement> Values { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CAliasMapElement
    {
        public CAliasMapElement()
        {
        }

        public CAliasMapElement(string alias, string val, string[] arguments)
        {
            Alias = alias;
            Value = val;
            Arguments = arguments;
        }

        public string Alias { get; set; }
        public string Value { get; set; }
        public string[] Arguments { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CAliasMap
    {
        public CAliasMap()
        {
            Values = new List<CAliasMapElement>();
        }

        public List<CAliasMapElement> Values { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CCmdHistory
    {
        public CCmdHistory()
        {
            Values = new List<string>();
        }

        public List<string> Values { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CToolsSettings : INotifyPropertyChanged
    {
        public CToolsSettings()
        {
            Values = new List<string>();
            Values.Add("Text Editor");
            Values.Add("Hex Editor");
            Values.Add("Image Editor");
            Selected = "Hex Editor";

            ExtensionsMap = new CExtensionsMap();
        }

        public List<string> Values { get; set; }

        public string Selected { get; set; }

        public CExtensionsMap ExtensionsMap { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }

        #endregion INotifyPropertyChanged Members
    }

    public class CGeneralSettings : INotifyPropertyChanged
    {
        private string _diffTool = "";
        private string _hexEditor = "";
        private string _imgEditor = "";
        private string _textEditor = "";
        private string _workingDirectory = "";

        public CGeneralSettings()
        {
            Tools = new CToolsSettings();
        }

        public string WorkingDirectory
        {
            get
            {
                if (string.Empty == _workingDirectory)
                    return Path.GetTempPath();

                return _workingDirectory;
            }
            set
            {
                _workingDirectory = value;
                OnPropertyChanged("WorkingDirectory");
            }
        }

        public string TextEditor
        {
            get => _textEditor;
            set
            {
                _textEditor = value;
                OnPropertyChanged("TextEditor");
            }
        }

        public string HexEditor
        {
            get => _hexEditor;
            set
            {
                _hexEditor = value;
                OnPropertyChanged("HexEditor");
            }
        }

        public string ImgEditor
        {
            get => _imgEditor;
            set
            {
                _imgEditor = value;
                OnPropertyChanged("ImgEditor");
            }
        }

        public string DiffTool
        {
            get => _diffTool;
            set
            {
                _diffTool = value;
                OnPropertyChanged("DiffTool");
            }
        }

        public CToolsSettings Tools { get; set; }

        public void Import(CGeneralSettings settings)
        {
            DiffTool = settings.DiffTool;
            HexEditor = settings.HexEditor;
            ImgEditor = settings.ImgEditor;
            TextEditor = settings.TextEditor;
            WorkingDirectory = settings.WorkingDirectory;
            Tools.Values = settings.Tools.Values;
            Tools.Selected = settings.Tools.Selected;

            Tools.ExtensionsMap.Values.Clear();

            foreach (var pair in settings.Tools.ExtensionsMap.Values)
            {
                var newPair = new CExtensionMapElement();
                newPair.Extension = pair.Extension;
                newPair.Tool = pair.Tool;

                Tools.ExtensionsMap.Values.Add(newPair);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }

        #endregion INotifyPropertyChanged Members
    }

    public class CAliasSettings
    {
        public CAliasSettings()
        {
            AliasList = new CAliasMap();
        }

        public CAliasMap AliasList { get; set; }

        public bool AddAlias(string alias, string value, string[] args)
        {
            var res = false;

            if (null == FindAlias(alias))
            {
                AliasList.Values.Add(new CAliasMapElement(alias, value, args));
                res = true;
            }

            return res;
        }

        public bool DelAlias(string alias)
        {
            var res = false;
            var item = FindAlias(alias);

            if (null != item) res = AliasList.Values.Remove(item);

            return res;
        }

        public string FindAliasCommand(string alias)
        {
            string res = null;
            var item = FindAlias(alias);

            if (null != item)
                res = item.Value;

            return res;
        }

        public string[] FindAliasArguments(string alias)
        {
            string[] res = null;
            var item = FindAlias(alias);

            if (null != item)
                res = item.Arguments;

            return res;
        }

        public CAliasMapElement FindAlias(string alias)
        {
            CAliasMapElement res = null;

            foreach (var item in AliasList.Values)
                if (item.Alias == alias)
                {
                    res = item;
                    break;
                }

            return res;
        }
    }

    public class CSettings
    {
        public CSettings()
        {
            GeneralSettings = new CGeneralSettings();
            Alias = new CAliasSettings();
            CmdHistory = new CCmdHistory();
        }

        public CGeneralSettings GeneralSettings { get; set; }
        public CAliasSettings Alias { get; set; }
        public CCmdHistory CmdHistory { get; set; }

        public string GetAssignedTool(string extension)
        {
            foreach (var item in GeneralSettings.Tools.ExtensionsMap.Values)
                if (item.Extension == extension)
                    switch (item.Tool)
                    {
                        case "Text Editor":
                            return GeneralSettings.TextEditor;

                        case "Hex Editor":
                            return GeneralSettings.HexEditor;

                        case "Image Editor":
                            return GeneralSettings.ImgEditor;
                    }

            return string.Empty;
        }
    }

    public class SettingsManager
    {
        public SettingsManager()
        {
            VSDSettings = new CSettings();
        }

        public CSettings VSDSettings { get; }

        public void LoadSettings()
        {
            VSDSettings.GeneralSettings.WorkingDirectory = Settings.Default.WorkingDirectory;
            VSDSettings.GeneralSettings.DiffTool = Settings.Default.DiffTool;
            VSDSettings.GeneralSettings.HexEditor = Settings.Default.HexEditor;
            VSDSettings.GeneralSettings.TextEditor = Settings.Default.TextEditor;
            VSDSettings.GeneralSettings.ImgEditor = Settings.Default.ImgEditor;
            VSDSettings.GeneralSettings.Tools.ExtensionsMap = Settings.Default.ExtensionsMap;
            VSDSettings.Alias.AliasList = Settings.Default.AliasMap;
            VSDSettings.CmdHistory = Settings.Default.CmdHistory;

            if (0 == Settings.Default.Version)
            {
                UpgradeV0toV1();
            }
        }

        public void SaveSettings()
        {
            Settings.Default.Version = 1;
            Settings.Default.WorkingDirectory = VSDSettings.GeneralSettings.WorkingDirectory;
            Settings.Default.DiffTool = VSDSettings.GeneralSettings.DiffTool;
            Settings.Default.HexEditor = VSDSettings.GeneralSettings.HexEditor;
            Settings.Default.TextEditor = VSDSettings.GeneralSettings.TextEditor;
            Settings.Default.ImgEditor = VSDSettings.GeneralSettings.ImgEditor;
            Settings.Default.ExtensionsMap = VSDSettings.GeneralSettings.Tools.ExtensionsMap;
            Settings.Default.AliasMap = VSDSettings.Alias.AliasList;
            Settings.Default.CmdHistory = VSDSettings.CmdHistory;
            Settings.Default.Save();
        }

        private void UpgradeV0toV1()
        {
            var aliases = new CAliasMap();
            foreach (var alias in VSDSettings.Alias.AliasList.Values)
            {
                if (null == alias.Arguments)
                {
                    try
                    {
                        var args = MiscHelpers.ParseCommand(alias.Value);
                        alias.Value = args[0];
                        alias.Arguments = MiscHelpers.ShiftArray(args);
                        aliases.Values.Add(alias);
                    }
                    catch (Exception e)
                    {
                        Debugger.Log(3, "Warning", "Failed to convert alias <" + alias.Alias + ">: " + e.Message);
                    }
                }
            }
            VSDSettings.Alias.AliasList = aliases;
        }
    }
}
