using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VSDebugCoreLib;
using VSDebugCoreLib.UI.Tools;

namespace VSDebugPro
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID =
        400)] // This attribute is used to register the information needed to show this context
    [ProvideAutoLoad("1336C085-E903-4333-BF29-7D4DF860DF9F")] // Force auto load
    [ProvideToolWindow(typeof(ConsoleWindow))] // console window
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.GuidVSDebugProPkgString)]
    public sealed class VSDebugProPackage : Package
    {
        /// <summary>
        ///     Default constructor of the context.
        /// </summary>
        public VSDebugProPackage()
        {
            VSDAssembly = Assembly.GetExecutingAssembly();
        }

        public static VSDebugContext Context { get; private set; }

        public Assembly VSDAssembly { get; }
        /////////////////////////////////////////////////////////////////////////////
        // Overridden Context Implementation

        #region Context Members

        /// <summary>
        ///     Initialization of the context; this method is called right after the context is sited, so this is the place
        ///     where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Initialized: {0}", ToString()));

            // Initialize base class
            base.Initialize();

            // Create extension context, this is used as global var for various stuff
            Context = new VSDebugContext(this, VSDAssembly)
            {
                IDE = (DTE2) GetService(typeof(DTE)),
                MenuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService
            };
            Context.Initialize();
        }

        #endregion Context Members
    }
}