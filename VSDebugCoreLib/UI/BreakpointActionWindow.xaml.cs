using EnvDTE;
using EnvDTE80;
using EnvDTE90a;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VSDebugCoreLib.UI
{
    /// <summary>
    /// Interaction logic for BreakpointAction.xaml
    /// </summary>
    public partial class BreakpointActionWindow : System.Windows.Window
    {
        VSDebugContext _context;

        public BreakpointActionWindow(VSDebugContext context)
        {
            InitializeComponent();

            _context = context;

            Document currentDocument = context.IDE.ActiveDocument;
            EnvDTE.TextSelection selection = (currentDocument.Object("") as TextDocument).Selection;

            var breakpoint = _context.Breakpoints.GetAssociation(currentDocument.FullName, selection.CurrentLine);

            if (breakpoint != null)
            {
                enableCommand.IsChecked = breakpoint.Enabled;
                textBoxCommandString.Text = breakpoint.Command;
                enableContinue.IsChecked = breakpoint.Continue;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Document currentDocument = _context.IDE.ActiveDocument;
            EnvDTE.TextSelection selection = (currentDocument.Object("") as TextDocument).Selection;
            var breakpoint = _context.Breakpoints.GetAssociation(currentDocument.FullName, selection.CurrentLine);

            if (breakpoint != null)
            {
                breakpoint.Enabled = (bool)enableCommand.IsChecked;
                breakpoint.Command = textBoxCommandString.Text;
                breakpoint.Continue = (bool)enableContinue.IsChecked;
            }
            else if ((bool)enableCommand.IsChecked)
            {
                _context.Breakpoints.AddAssociation(currentDocument.FullName, selection.CurrentLine, textBoxCommandString.Text, (bool)enableContinue.IsChecked);
            }
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
