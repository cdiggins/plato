using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;

namespace PlatoVSIX
{
    public class PlatoLanguageService : IVsLanguageInfo
    {
        public int GetLanguageName(out string bstrName)
        {
            bstrName = "Plato";
            return VSConstants.S_OK;
        }

        public int GetFileExtensions(out string pbstrExtensions)
        {
            pbstrExtensions = ".plato";
            return VSConstants.S_OK;
        }

        public int GetColorizer(IVsTextLines pBuffer, out IVsColorizer ppColorizer)
        {
            // We do not return because we are using a classifier
            ppColorizer = null;
            return VSConstants.E_NOTIMPL;
        }

        public int GetCodeWindowManager(IVsCodeWindow pCodeWin, out IVsCodeWindowManager ppCodeWinMgr)
        {
            ppCodeWinMgr = null;
            return VSConstants.E_NOTIMPL;
        }
    }
}