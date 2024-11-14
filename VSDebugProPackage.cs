using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using VSDebugCoreLib;
using VSDebugCoreLib.UI.Tools;

namespace VSDebugPro
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // This attribute is used to register the information needed to show this context
    [ProvideToolWindow(typeof(ConsoleWindow))] // console window
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.GuidVSDebugProPkgString)]
    public sealed class VSDebugProPackage : AsyncPackage
    {
        public static VSDebugContext Context { get; private set; }
        public Assembly VSDAssembly { get; }

        public VSDebugProPackage()
        {
            VSDAssembly = Assembly.GetExecutingAssembly();
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Initialized: {0}", ToString()));

            // Create extension context
            Context = new VSDebugContext(this, VSDAssembly)
            {
                IDE = (DTE2)await GetServiceAsync(typeof(DTE)),
                MenuCommandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService
            };
            Context.Initialize();
        }
    }
}