using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using VSDebugCoreLib.Properties;

namespace VSDebugCoreLib
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CExtensionMapElement
    {
        public string Extension { get; set; }
        public string Tool { get; set; }

        public CExtensionMapElement()
        {
        }

        public CExtensionMapElement(string ext, string tool)
        {
            Extension = ext;
            Tool = tool;
        }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CExtensionsMap
    {
        public List<CExtensionMapElement> Values { get; set; }

        public CExtensionsMap()
        {
            Values = new List<CExtensionMapElement>();
        }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CAliasMapElement
    {
        public string Alias { get; set; }
        public string Value { get; set; }

        public CAliasMapElement()
        {
        }

        public CAliasMapElement(string alias, string val)
        {
            Alias = alias;
            Value = val;
        }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CAliasMap
    {
        public List<CAliasMapElement> Values { get; set; }

        public CAliasMap()
        {
            Values = new List<CAliasMapElement>();
        }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CCmdHistory
    {
        public List<string> Values { get; set; }

        public CCmdHistory()
        {
            Values = new List<string>();
        }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class CToolsSettings : INotifyPropertyChanged 
    {
        private List<string> _tools;

        private string _toolsSelected;
       
        public List<String> Values
        {
            get => _tools; set => _tools = value;
        }

       
        public string Selected
        {
            get => _toolsSelected; set => _toolsSelected = value;
        }


        public CExtensionsMap ExtensionsMap { get; set; }

        public CToolsSettings()
        {
            _tools = new List<string>();
            _tools.Add("Text Editor");
            _tools.Add("Hex Editor");
            _tools.Add("Image Editor");            
            _toolsSelected = "Hex Editor";

            ExtensionsMap = new CExtensionsMap();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }

        #endregion

    }

    public class CGeneralSettings : INotifyPropertyChanged
    {
        private string _workingDirectory = "";
        private string _textEditor       = "";
        private string _hexEditor        = "";
        private string _imgEditor        = "";
        private string _diffTool         = "";

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
            get => _textEditor; set
            {
                _textEditor = value;
                OnPropertyChanged("TextEditor");
            }
        }
        public string HexEditor
        {
            get => _hexEditor; set
            {
                _hexEditor = value;
                OnPropertyChanged("HexEditor");
            }
        }
        public string ImgEditor
        {
            get => _imgEditor; set
            {
                _imgEditor = value;
                OnPropertyChanged("ImgEditor");
            }
        }
        public string DiffTool
        {
            get => _diffTool; set
            {
                _diffTool = value;
                OnPropertyChanged("DiffTool");
            }
        }


        public CToolsSettings Tools { get; set; }
       

        public CGeneralSettings()
        {
            Tools = new CToolsSettings();
        }

        public void Import( CGeneralSettings settings )
        {
            DiffTool = settings.DiffTool;
            HexEditor = settings.HexEditor;
            ImgEditor = settings.ImgEditor;
            TextEditor = settings.TextEditor;
            WorkingDirectory = settings.WorkingDirectory;
            Tools.Values = settings.Tools.Values;
            Tools.Selected = settings.Tools.Selected;

            Tools.ExtensionsMap.Values.Clear();

            foreach (CExtensionMapElement pair in settings.Tools.ExtensionsMap.Values)
            {
                CExtensionMapElement newPair = new CExtensionMapElement();
                newPair.Extension = pair.Extension;
                newPair.Tool = pair.Tool;

                Tools.ExtensionsMap.Values.Add(newPair);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }

    public class CAliasSettings
    {
        public CAliasSettings()
        {
            AliasList = new CAliasMap();
        }

        public CAliasMap AliasList { get; set; }

        public bool   AddAlias( string alias, string value )
        {
            bool    res     = false;

            if (null == FindAlias(alias))
            {
                AliasList.Values.Add(new CAliasMapElement(alias, value));
                res = true;
            }

            return res;
        }

        public bool   DelAlias( string alias )
        {
            bool res = false;
            CAliasMapElement item = FindAlias( alias );

            if (null != item)
            {
                res = AliasList.Values.Remove(item);
            }

            return res;
        }

        public string FindAliasValue(string alias)
        {
            string res = null;
            CAliasMapElement item = FindAlias( alias );

            if (null != item)
                res = item.Value;

            return res;
        }

        public CAliasMapElement FindAlias(string alias)
        {
            CAliasMapElement res = null;

            foreach (var item in AliasList.Values)
            {
                if (item.Alias == alias)
                {
                    res = item;
                    break;
                }
            }

            return res;
        }
    }

    public class CSettings
    {
        public CGeneralSettings GeneralSettings { get; set; }
        public CAliasSettings Alias { get; set; }
        public CCmdHistory CmdHistory { get; set; }

        public CSettings() 
        {
            GeneralSettings = new CGeneralSettings();
            Alias           = new CAliasSettings();
            CmdHistory      = new CCmdHistory();
        }

        public string GetAssignedTool(string extension)
        {

            foreach (var item in GeneralSettings.Tools.ExtensionsMap.Values)
            {
                if (item.Extension == extension)
                {
                    switch (item.Tool)
                    {
                        case "Text Editor":
                            return GeneralSettings.TextEditor;
                        case "Hex Editor":
                            return GeneralSettings.HexEditor;
                        case "Image Editor":
                            return GeneralSettings.ImgEditor;
                    }
                }

            }

            return string.Empty;
        }
    }

    public class SettingsManager
    {
        public CSettings VSDSettings { get; private set; }

        public SettingsManager()
        {
            VSDSettings = new CSettings();
        }

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

            
        }

        public void SaveSettings()
        {
            Settings.Default.WorkingDirectory   = VSDSettings.GeneralSettings.WorkingDirectory;
            Settings.Default.DiffTool           = VSDSettings.GeneralSettings.DiffTool;
            Settings.Default.HexEditor          = VSDSettings.GeneralSettings.HexEditor;
            Settings.Default.TextEditor         = VSDSettings.GeneralSettings.TextEditor;
            Settings.Default.ImgEditor          = VSDSettings.GeneralSettings.ImgEditor;
            Settings.Default.ExtensionsMap      = VSDSettings.GeneralSettings.Tools.ExtensionsMap;
            Settings.Default.AliasMap           = VSDSettings.Alias.AliasList;
            Settings.Default.CmdHistory         = VSDSettings.CmdHistory;

            Settings.Default.Save();
        }
        
    }
}
