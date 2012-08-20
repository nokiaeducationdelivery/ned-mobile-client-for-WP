/*******************************************************************************
* Copyright (c) 2012 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
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
