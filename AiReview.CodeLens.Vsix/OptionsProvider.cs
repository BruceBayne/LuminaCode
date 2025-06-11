using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;

namespace AiReview.CodeLens.Vsix
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class VsLuminaCodeGeneralOptions : BaseOptionPage<General>
        {
        }

        [ComVisible(true)]
        public class VsBetterNamingOptions : BaseOptionPage<BetterNamingSettingsModel>
        {
        }


        [ComVisible(true)]
        public class VsCodeReviewOptions : BaseOptionPage<VsCodeReviewSettingsModel>
        {
        }
    }
}