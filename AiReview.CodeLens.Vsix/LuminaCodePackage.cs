using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AiReview.CodeLens.Vsix;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[Guid(PackageGuidString)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideOptionPage(typeof(OptionsProvider.VsLuminaCodeGeneralOptions), "LuminaCode", "General", 0, 0, true,
    SupportsProfiles = true)]
[ProvideOptionPage(typeof(OptionsProvider.VsCodeReviewOptions), "LuminaCode", "Code Review", 1, 1, true,
    SupportsProfiles = true)]
[ProvideOptionPage(typeof(OptionsProvider.VsBetterNamingOptions), "LuminaCode", "Better Naming", 1, 1, true,
    SupportsProfiles = true)]
[ProvideMenuResource("Menus.ctmenu", 1)]
public sealed class LuminaCodePackage : AsyncPackage
{
    public const string PackageGuidString = "ffe33ea4-4d07-4a48-aaca-d82ff13ec862";


    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    /// <param name="progress">A provider for progress updates.</param>
    /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    protected override async Task InitializeAsync(CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await base.InitializeAsync(cancellationToken, progress);


        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        await Command1.InitializeAsync(this);
    }
}