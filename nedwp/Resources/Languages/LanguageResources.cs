using System.Globalization;
using System;

namespace NedWp.Resources.Languages 
{

    /// <summary>
    /// Class containing appropriate language strings.
    /// </summary>
    public class LanguageResources
    {
        private static FileLanguage _localizedResources = new FileLanguage();

        /// <summary>
        /// Get text in right language.
        /// </summary>
        public FileLanguage LocalizedResources 
        { 
            get 
            { 
                return _localizedResources; 
            } 
        }

        /// <summary>
        /// Selecting right application language (phone language or selected language) and set appropriate Application Culture.
        /// </summary>
        public LanguageResources()
        {
           AppResources.Culture = new CultureInfo(CultureInfo.CurrentUICulture.Name);
        }
    }
}
