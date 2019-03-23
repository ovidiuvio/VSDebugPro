using System.Windows;
using System.Windows.Input;
using VSDebugCoreLib.Utils;

namespace VSDebugCoreLib.UI
{
    /// <summary>
    ///     Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private readonly VSDebugContext Context;

        public AboutWindow(VSDebugContext context)
        {
            Context = context;

            InitializeComponent();

            var asmProduct = Context.VSDAssembly;

            _labelProduct.Content = asmProduct.GetName().Name;
            _labelVersion.Content = asmProduct.GetName().Version;
            _labelLicense.Content = VSDebugCoreLib.Resources.ProductCopyright;
            _labelWWW.Content = VSDebugCoreLib.Resources.Website + " - " + VSDebugCoreLib.Resources.ContactInfo;

            _txtHistory.Text = VSDebugCoreLib.Resources.changelog;
            _txtHistory.IsReadOnly = true;

            _txtLicense.Text = VSDebugCoreLib.Resources.license;
            _txtLicense.IsReadOnly = true;

            _labelProduct.UpdateLayout();
            _labelVersion.UpdateLayout();
            _labelLicense.UpdateLayout();
            _labelWWW.UpdateLayout();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
        }

        private void labelWWW_Click(object sender, MouseButtonEventArgs e)
        {
            MiscHelpers.LaunchLink(@"http://" + VSDebugCoreLib.Resources.Website);
        }
    }
}