/*******************************************************************************
* Copyright (c) 2011 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
using System;
using System.IO.IsolatedStorage;

namespace NedEngine
{
    public class ApplicationSettings : PropertyNotifierBase
    {
        private const String KServerUrlKey = "server";
        private const String KRememberMeKey = "rememberMe";
        private const String KRememberedLoginKey = "rememberedLogin";
        private const String KRememberedPasswordKey = "rememberedPassword";

        private Uri _serverUrl;
        public Uri ServerUrl
        {
            get
            {
                return _serverUrl;
            }

            set
            {
                if (value != _serverUrl)
                {
                    _serverUrl = value;

                    IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
                    appSettings.Remove(KServerUrlKey);

                    if (value != null)
                    {
                        appSettings.Add(KServerUrlKey, value);
                    }

                    appSettings.Save();
                    OnPropertyChanged("ServerUrl");
                }
            }
        }

        private bool _rememberMe;
        public bool RememberMe { 
            get
            {
                return _rememberMe;
            }
            set
            {
                if (value != _rememberMe)
                {
                    _rememberMe = value;

                    IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
                    appSettings.Remove(KRememberMeKey);
                    appSettings.Add(KRememberMeKey, value);
                    appSettings.Save();

                    OnPropertyChanged("RememberMe");
                }
                if (!value)
                {
                    RememberedLogin = String.Empty;
                    RememberedPassword = String.Empty;
                }
            }
        }

        private string _rememberedLogin;
        public string RememberedLogin
        {
            get 
            {
                return _rememberedLogin;
            }
            set
            {
                if (value != _rememberedLogin)
                {
                    _rememberedLogin = value;

                    IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
                    appSettings.Remove(KRememberedLoginKey);
                    appSettings.Add(KRememberedLoginKey, value);
                    appSettings.Save();

                    OnPropertyChanged("RememberedLogin");
                }
            }

        }

        private string _rememberedPassword;
        public string RememberedPassword
        {
            get
            {
                return _rememberedPassword;
            }
            set
            {
                if (value != _rememberedPassword)
                {
                    _rememberedPassword = value;

                    IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
                    appSettings.Remove(KRememberedPasswordKey);
                    appSettings.Add(KRememberedPasswordKey, value);
                    appSettings.Save();

                    OnPropertyChanged("RememberedPassword");
                }
            }

        }
        

        public ApplicationSettings()
        {
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
            appSettings.TryGetValue(KServerUrlKey, out _serverUrl);
            appSettings.TryGetValue(KRememberMeKey, out _rememberMe);
            appSettings.TryGetValue(KRememberedLoginKey, out _rememberedLogin);
            appSettings.TryGetValue(KRememberedPasswordKey, out _rememberedPassword);
            AvailableLanguages = new Languages();
        }

        public Languages AvailableLanguages { get; set; }
    }
}
