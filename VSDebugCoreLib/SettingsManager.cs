using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib
{
    public class CExtensionsMap
    {
        public CExtensionsMap()
        {
            Values = new List<Tuple<string,string>>();
        }

        public List<Tuple<string, string>> Values { get; set; }
    }

    public class CAliasMap
    {
        public CAliasMap()
        {
            Values = new List<Tuple<string, string>>();
        }

        public List<Tuple<string, string>> Values { get; set; }
    }

    public class CCmdHistory
    {
        public CCmdHistory()
        {
            Values = new List<string>();
        }

        public List<string> Values { get; set; }
    }

    public class CToolsSettings : INotifyPropertyChanged
    {
        public CToolsSettings()
        {
            Values = new List<string>
            {
                "Text Editor",
                "Hex Editor",
                "Image Editor"
            };
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
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
                var newPair = new Tuple<string, string>(pair.Item1, pair.Item2);

                Tools.ExtensionsMap.Values.Add(newPair);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
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

        public bool AddAlias(string alias, string value)
        {
            var res = false;

            if (null == FindAlias(alias))
            {
                AliasList.Values.Add(new Tuple<string, string>(alias, value));
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

        public string FindAliasValue(string alias)
        {
            string res = null;
            var item = FindAlias(alias);

            if (null != item)
                res = item.Item2;

            return res;
        }

        public Tuple<string, string> FindAlias(string alias)
        {
            Tuple<string, string> res = null;

            foreach (var item in AliasList.Values)
                if (item.Item1 == alias)
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
                if (item.Item1 == extension)
                    switch (item.Item2)
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

        private static string GetKey(Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key].ToString() : "";
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(MiscHelpers.GetApplicationDataPath() + "settings.json"))
                {
                    Dictionary<string, object> settings;
                    JsonSerializer serializer = new JsonSerializer();
                    using (StreamReader sr = new StreamReader(MiscHelpers.GetApplicationDataPath() + "settings.json"))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        settings = serializer.Deserialize<Dictionary<string, object>>(reader);
                    }

                    VSDSettings.GeneralSettings.WorkingDirectory = GetKey(settings, "WorkingDirectory");
                    VSDSettings.GeneralSettings.DiffTool = GetKey(settings, "DiffTool");
                    VSDSettings.GeneralSettings.HexEditor = GetKey(settings, "HexEditor");
                    VSDSettings.GeneralSettings.TextEditor = GetKey(settings, "TextEditor");
                    VSDSettings.GeneralSettings.ImgEditor = GetKey(settings, "ImageEditor");
                    string extJson = GetKey(settings, "ExtensionsMap");
                    VSDSettings.GeneralSettings.Tools.ExtensionsMap.Values = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(extJson);
                    string aliasJson = GetKey(settings, "AliasMap");
                    VSDSettings.Alias.AliasList.Values = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(aliasJson);
                    string historyJson = GetKey(settings, "CmdHistory");
                    VSDSettings.CmdHistory.Values = JsonConvert.DeserializeObject<List<string>>(historyJson);
                }
                // fill default ext map
                else
                {
                    VSDSettings.GeneralSettings.Tools.ExtensionsMap.Values = new List<Tuple<string, string>>
                    {
                        new Tuple<string, string>(".bin", "Hex Editor"),
                        new Tuple<string, string>(".dmp", "Hex Editor"),
                        new Tuple<string, string>(".hex", "Hex Editor"),
                        new Tuple<string, string>(".txt", "Text Editor")
                    };
                }
            }
            catch (Exception)
            {
                // settings file corrupted, delete it
                if (File.Exists(MiscHelpers.GetApplicationDataPath() + "settings.json"))
                    File.Delete(MiscHelpers.GetApplicationDataPath() + "settings.json");
            }
        }

        public void SaveSettings()
        {
            var settings = new Dictionary<string, object>();
            settings["WorkingDirectory"] = VSDSettings.GeneralSettings.WorkingDirectory;
            settings["DiffTool"] = VSDSettings.GeneralSettings.DiffTool;
            settings["HexEditor"] = VSDSettings.GeneralSettings.HexEditor;
            settings["TextEditor"] = VSDSettings.GeneralSettings.TextEditor;
            settings["ImageEditor"] = VSDSettings.GeneralSettings.ImgEditor;
            settings["ExtensionsMap"] = VSDSettings.GeneralSettings.Tools.ExtensionsMap.Values;
            settings["AliasMap"] = VSDSettings.Alias.AliasList.Values;
            settings["CmdHistory"] = VSDSettings.CmdHistory.Values;

            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(MiscHelpers.GetApplicationDataPath() + "settings.json"))
            using (JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
            {
                serializer.Serialize(writer, settings);
            }
        }
    }
}