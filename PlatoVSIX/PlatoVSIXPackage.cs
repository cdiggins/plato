using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace PlatoVSIX
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// <para>
    /// MEF Composition Errors: If any of your components aren't loaded, Visual Studio may silently fail to load them.
    /// Check the "ActivityLog.xml" file in %AppData%\Microsoft\VisualStudio\<Version>\ActivityLog.xml for detailed error messages.
    /// Logging and Diagnostics: Use IVsOutputWindowPane or other logging mechanisms to output diagnostic information during development.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PlatoVSIXPackage.PackageGuidString)]
    [ProvideLanguageService(typeof(PlatoLanguageService), "Plato", 0)]
    [ProvideService(typeof(PlatoLanguageService))]
    [ProvideLanguageExtension(typeof(PlatoLanguageService), ".plato")]
    public sealed class PlatoVSIXPackage : AsyncPackage
    {
        /// <summary>
        /// PlatoVSIXPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3ebeccb7-4233-4406-9616-881968072ccb";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Register the language service
            var serviceContainer = this as IServiceContainer;
            var languageService = new PlatoLanguageService();
            serviceContainer.AddService(typeof(PlatoLanguageService), languageService, promote: true);
        }
    }
}
