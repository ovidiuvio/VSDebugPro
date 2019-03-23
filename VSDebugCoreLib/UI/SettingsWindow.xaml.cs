using System.Windows;
using System.Windows.Forms;

using VSDebugCoreLib.Properties;

namespace VSDebugCoreLib.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        protected VSDebugContext Context { get; private set; }
        protected CGeneralSettings m_generalSettings;

        public SettingsWindow(VSDebugContext package)
        {
            InitializeComponent();

            Context = package;

            m_generalSettings = new CGeneralSettings();
            m_generalSettings.Import(Context.Settings.GeneralSettings);

            groupGeneralSettings.DataContext = m_generalSettings;
            groupExtensionSettings.DataContext = m_generalSettings;
        }

        private void Browse_WorkingDirectory(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlgFolder = new FolderBrowserDialog();
            dlgFolder.Description = "Select the directory to use.";
            dlgFolder.ShowNewFolderButton = true;

            DialogResult result = dlgFolder.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_generalSettings.WorkingDirectory = dlgFolder.SelectedPath;
            }
        }

        private void Browse_TextEditor(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgFile = new OpenFileDialog();
            dlgFile.DefaultExt = "exe";
            dlgFile.Filter = "exe files (*.exe)|*.exe";

            DialogResult result = dlgFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_generalSettings.TextEditor = dlgFile.FileName;
            }
        }

        private void Browse_HexEditor(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgFile = new OpenFileDialog();
            dlgFile.DefaultExt = "exe";
            dlgFile.Filter = "exe files (*.exe)|*.exe";

            DialogResult result = dlgFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_generalSettings.HexEditor = dlgFile.FileName;
            }
        }

        private void Browse_ImgEditor(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgFile = new OpenFileDialog();
            dlgFile.DefaultExt = "exe";
            dlgFile.Filter = "exe files (*.exe)|*.exe";

            DialogResult result = dlgFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_generalSettings.ImgEditor = dlgFile.FileName;
            }
        }

        private void Browse_DiffTool(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgFile = new OpenFileDialog();
            dlgFile.DefaultExt = "exe";
            dlgFile.Filter = "exe files (*.exe)|*.exe";

            DialogResult result = dlgFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                m_generalSettings.DiffTool = dlgFile.FileName;
            }
        }

        private void add_extension_map(object sender, RoutedEventArgs e)
        {
            if (tboxExtension.Text != string.Empty && m_generalSettings.Tools.Selected != string.Empty)
            {
                bool bAdd = true;
                foreach (var item in m_generalSettings.Tools.ExtensionsMap.Values)
                {
                    if (item.Extension == tboxExtension.Text)
                    {
                        bAdd = false;
                    }
                }

                if (!System.IO.Path.HasExtension(tboxExtension.Text))
                    bAdd = false;

                if (bAdd)
                {
                    m_generalSettings.Tools.ExtensionsMap.Values.Add(new CExtensionMapElement(tboxExtension.Text, m_generalSettings.Tools.Selected));
                    m_generalSettings.Tools.OnPropertyChanged("ExtensionsMap");
                    m_generalSettings.Tools.OnPropertyChanged("Tool");
                    m_generalSettings.Tools.OnPropertyChanged("Extension");
                    datagExtensionsMap.Items.Refresh();
                }
            }
        }

        private void remove_extension_map(object sender, RoutedEventArgs e)
        {
            if (datagExtensionsMap.SelectedIndex >= 0)
            {
                m_generalSettings.Tools.ExtensionsMap.Values.Remove(datagExtensionsMap.SelectedValue as CExtensionMapElement);
                m_generalSettings.Tools.OnPropertyChanged("ExtensionsMap");
                m_generalSettings.Tools.OnPropertyChanged("Tool");
                m_generalSettings.Tools.OnPropertyChanged("Extension");
                datagExtensionsMap.Items.Refresh();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Context.Settings.GeneralSettings.Import(m_generalSettings);

            Settings.Default.Save();

            this.Close();
        }
    }
}