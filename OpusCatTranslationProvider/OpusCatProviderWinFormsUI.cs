using System;
using System.Windows.Forms;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace OpusCatTranslationProvider
{
    #region "Declaration"
    [TranslationProviderWinFormsUi(
        Id = "OpusCatProviderWinFormsUI",
        Name = "OpusCatProviderWinFormsUI",
        Description = "OpusCatProviderWinFormsUI")]
    #endregion
    public class OpusCatProviderWinFormsUI : ITranslationProviderWinFormsUI
    {
        #region ITranslationProviderWinFormsUI Members
         
        
        /// <summary>
        /// Show the plug-in settings form when the user is adding the translation provider plug-in
        /// through the GUI of SDL Trados Studio
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="languagePairs"></param>
        /// <param name="credentialStore"></param>
        /// <returns></returns>
        #region "Browse"
        public ITranslationProvider[] Browse(IWin32Window owner, LanguagePair[] languagePairs, ITranslationProviderCredentialStore credentialStore)
        {
            OpusCatOptionsFormWPF dialog = new OpusCatOptionsFormWPF(new OpusCatOptions(), languagePairs);
             
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                OpusCatProvider testProvider = new OpusCatProvider(dialog.Options);
                return new ITranslationProvider[] { testProvider };
            }
            return null;
        }
        #endregion


        
        /// <summary>
        /// Determines whether the plug-in settings can be changed
        /// by displaying the Settings button in SDL Trados Studio.
        /// </summary>
        #region "SupportsEditing"
        public bool SupportsEditing
        {
            get { return true; }
        }
        #endregion

        /// <summary>
        /// If the plug-in settings can be changed by the user,
        /// SDL Trados Studio will display a Settings button.
        /// By clicking this button, users raise the plug-in user interface,
        /// in which they can modify any applicable settings.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="translationProvider"></param>
        /// <param name="languagePairs"></param>
        /// <param name="credentialStore"></param>
        /// <returns></returns>
        #region "Edit"
        public bool Edit(IWin32Window owner, ITranslationProvider translationProvider, LanguagePair[] languagePairs, ITranslationProviderCredentialStore credentialStore)
        {
            OpusCatProvider editProvider = translationProvider as OpusCatProvider;
            if (editProvider == null)
            {
                return false;
            }
            
            OpusCatOptionsFormWPF dialog = new OpusCatOptionsFormWPF(editProvider.Options, languagePairs);
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Can be used in implementations in which a user login is required, e.g.
        /// for connecting to an online translation provider.
        /// In our implementation, however, this is not required, so we simply set
        /// this member to return always True.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="translationProviderUri"></param>
        /// <param name="translationProviderState"></param>
        /// <param name="credentialStore"></param>
        /// <returns></returns>
        #region "GetCredentialsFromUser"
        public bool GetCredentialsFromUser(IWin32Window owner, Uri translationProviderUri, string translationProviderState, ITranslationProviderCredentialStore credentialStore)
        {
            //return true;
            return true;
        }
        #endregion

        /// <summary>
        /// Used for displaying the plug-in info such as the plug-in name,
        /// tooltip, and icon.
        /// </summary>
        /// <param name="translationProviderUri"></param>
        /// <param name="translationProviderState"></param>
        /// <returns></returns>
        #region "GetDisplayInfo"
        public TranslationProviderDisplayInfo GetDisplayInfo(Uri translationProviderUri, string translationProviderState)
        {
            TranslationProviderDisplayInfo info = new TranslationProviderDisplayInfo();
            info.Name = PluginResources.Plugin_NiceName;            
            info.TranslationProviderIcon = PluginResources.opus;
            info.TooltipText = PluginResources.Plugin_Tooltip;

            info.SearchResultImage = PluginResources.opus1;

            return info;
        }
        #endregion



        public bool SupportsTranslationProviderUri(Uri translationProviderUri)
        {
            if (translationProviderUri == null)
            {
                throw new ArgumentNullException("URI not supported by the plug-in.");
            }
            return String.Equals(translationProviderUri.Scheme, OpusCatProvider.OpusCatTranslationProviderScheme, StringComparison.CurrentCultureIgnoreCase);
        }

        public string TypeDescription
        {
            get { return PluginResources.Plugin_Description; }
        }

        public string TypeName
        {
            get { return PluginResources.Plugin_NiceName; }
        }

        #endregion
    }
}
