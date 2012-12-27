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

namespace NedWp.Resources.Languages
{
    using System;
    using System.Xml.Linq;
    using System.IO.IsolatedStorage;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Globalization;
    using System.Collections.Generic;

    public class FileLanguage
    {
        private AppResources _standardResources;

        public XDocument LocalizationFile { get; set; }

        public static FileLanguage _instance;
        private const string TAG_DATA = "data";
        private const string ATTRIBUTE_NAME = "name";
        private const string TAG_VALUE = "value";

        public FileLanguage()
        {
            _standardResources = new AppResources();
            _instance = this;
            LoadCustomTranslation();
        }


        public void LoadCustomTranslation()
        {
            string currentLanguageId = App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage;
            if( currentLanguageId == null || currentLanguageId == "0" )
            {
                return;
            }
            string localizationFilePath = NedEngine.Utils.LocalizationsFilePath( currentLanguageId );

            using( IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication() )
            {
                if( file.FileExists( localizationFilePath ) )
                {
                    using( IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream( localizationFilePath, FileMode.Open, file ) )
                    {
                        LocalizationFile = XDocument.Load( isfStream );
                    }
                }
                else
                {
                    LocalizationFile = new XDocument();
                }
            }
        }

        private static string getMessage( string name )
        {
            if( _instance.LocalizationFile == null || _instance.LocalizationFile.Root == null )
            {
                return null;
            }
            var translationQuery = from translation in _instance.LocalizationFile.Root.Elements( TAG_DATA )
                                   where translation.Attribute( ATTRIBUTE_NAME ).Value == name
                                   select new
                                   {
                                       Translation = translation.Element( TAG_VALUE ).Value
                                   };
            if( translationQuery.Count() == 0 ) // Page does not have help
                return null;
            return ConvertUTFNotation( translationQuery.First().Translation );
        }

        private static string ConvertUTFNotation( String input )
        {
            if( input == null )
            {
                return null;
            }
            else
            {
                string conv1 = Regex.Replace( input,
                                             @"\\u(?<Value>[a-zA-Z0-9]{4})",
                                             m =>
                                             {
                                                 return ( (char)int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber ) ).ToString();
                                             } );//convertion of '/u' UTF16 notation to string
                string conv2 = Regex.Replace( conv1,
                                              @"\\n",
                                             "\n" );//convertion of newline ('/n') character
                return conv2;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to NOKIA EDUCATION DELIVERY.
        /// </summary>
        public static string App_ApplicationTitle
        {
            get
            {
                string customLocalization = getMessage( "MID_TITLE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_TITLE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Welcome.
        /// </summary>
        public static string MID_DEFAULTMOTD
        {
            get
            {
                string customLocalization = getMessage( "MID_DEFAULTMOTD" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_DEFAULTMOTD", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        public static string HELP
        {
            get
            {
                string customLocalization = getMessage( "HELP" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "HELP", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No help available for this screen.
        /// </summary>
        public static string App_OpeningHelpErrorUnknowScreen
        {
            get
            {
                string customLocalization = getMessage( "App_OpeningHelpErrorUnknowScreen" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "App_OpeningHelpErrorUnknowScreen", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Categories: {0}.
        /// </summary>
        public static string CATEGORIES
        {
            get
            {
                string customLocalization = getMessage( "CATEGORIES" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CATEGORIES", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        public static string DELETE
        {
            get
            {
                string customLocalization = getMessage( "DELETE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DELETE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to download all.
        /// </summary>
        public static string DOWNLOAD_ALL
        {
            get
            {
                string customLocalization = getMessage( "DOWNLOAD_ALL" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DOWNLOAD_ALL", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to back.
        /// </summary>
        public static string MID_BACK_COMMAND
        {
            get
            {
                string customLocalization = getMessage( "MID_BACK_COMMAND" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_BACK_COMMAND", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to refresh.
        /// </summary>
        public static string CataloguePage_RefreshButton
        {
            get
            {
                string customLocalization = getMessage( "CataloguePage_RefreshButton" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CataloguePage_RefreshButton", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to search.
        /// </summary>
        public static string MID_SEARCH_COMMAND
        {
            get
            {
                string customLocalization = getMessage( "MID_SEARCH_COMMAND" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_SEARCH_COMMAND", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Media items enqueued for download: {0}.
        /// </summary>
        public static string ITEM_ADDED_TO_QUEUE
        {
            get
            {
                string customLocalization = getMessage( "ITEM_ADDED_TO_QUEUE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ITEM_ADDED_TO_QUEUE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Media items: {0} (downloaded: {1}, on server: {2} ).
        /// </summary>
        public static string MEDIA_ITEMS
        {
            get
            {
                string customLocalization = getMessage( "MEDIA_ITEMS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MEDIA_ITEMS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Demo library id &apos;khan&apos;.
        /// </summary>
        public static string DEMOLIBID
        {
            get
            {
                string customLocalization = getMessage( "DEMOLIBID" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DEMOLIBID", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to For demo please use login and password &apos;guest&apos;.
        /// </summary>
        public static string DemoLoginDetails
        {
            get
            {
                string customLocalization = getMessage( "DemoLoginDetails" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DemoLoginDetails", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to For demo please use:.
        /// </summary>
        public static string DEMOURL
        {
            get
            {
                string customLocalization = getMessage( "DEMOURL" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DEMOURL", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} added to download queue.
        /// </summary>
        public static string DownloadCommand_AddedToDownload
        {
            get
            {
                string customLocalization = getMessage( "DownloadCommand_AddedToDownload" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DownloadCommand_AddedToDownload", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} is already downloaded.
        /// </summary>
        public static string DownloadCommand_AlreadyDownloaded
        {
            get
            {
                string customLocalization = getMessage( "DownloadCommand_AlreadyDownloaded" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DownloadCommand_AlreadyDownloaded", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} downloading started.
        /// </summary>
        public static string DownloadCommand_DownloadingStarted
        {
            get
            {
                string customLocalization = getMessage( "DownloadCommand_DownloadingStarted" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DownloadCommand_DownloadingStarted", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Item.
        /// </summary>
        public static string DownloadCommand_Item
        {
            get
            {
                string customLocalization = getMessage( "DownloadCommand_Item" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DownloadCommand_Item", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to cancel.
        /// </summary>
        public static string CANCEL
        {
            get
            {
                string customLocalization = getMessage( "CANCEL" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CANCEL", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Could not connect with server. Make sure your network connection is working and try again..
        /// </summary>
        public static string Error_Connection
        {
            get
            {
                string customLocalization = getMessage( "Error_Connection" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "Error_Connection", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Username and password cannot be empty.
        /// </summary>
        public static string Error_EmptyUsernameOrPassword
        {
            get
            {
                string customLocalization = getMessage( "Error_EmptyUsernameOrPassword" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "Error_EmptyUsernameOrPassword", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid username or password.
        /// </summary>
        public static string BAD_LOGIN
        {
            get
            {
                string customLocalization = getMessage( "BAD_LOGIN" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "BAD_LOGIN", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid NED server address.
        /// </summary>
        public static string NEDSERVICENOTPRESENT
        {
            get
            {
                string customLocalization = getMessage( "NEDSERVICENOTPRESENT" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "NEDSERVICENOTPRESENT", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library is already added.
        /// </summary>
        public static string LIBRARY_ALREADY_EXISTS
        {
            get
            {
                string customLocalization = getMessage( "LIBRARY_ALREADY_EXISTS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LIBRARY_ALREADY_EXISTS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library was deleted from server.
        /// </summary>
        public static string Error_LibraryDeletedFromServer
        {
            get
            {
                string customLocalization = getMessage( "Error_LibraryDeletedFromServer" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "Error_LibraryDeletedFromServer", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library doesn&apos;t exist..
        /// </summary>
        public static string LIBRARY_NOT_EXISTS
        {
            get
            {
                string customLocalization = getMessage( "LIBRARY_NOT_EXISTS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LIBRARY_NOT_EXISTS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library id cannot be empty..
        /// </summary>
        public static string Error_LibraryIdEmpty
        {
            get
            {
                string customLocalization = getMessage( "Error_LibraryIdEmpty" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "Error_LibraryIdEmpty", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unexpected error has occured..
        /// </summary>
        public static string Error_Unexpected
        {
            get
            {
                string customLocalization = getMessage( "Error_Unexpected" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "Error_Unexpected", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Help not available.
        /// </summary>
        public static string MISSING_HELP
        {
            get
            {
                string customLocalization = getMessage( "MISSING_HELP" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MISSING_HELP", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Select Language.
        /// </summary>
        public static string CHOOSE_LANGUAGE
        {
            get
            {
                string customLocalization = getMessage( "CHOOSE_LANGUAGE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CHOOSE_LANGUAGE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Tried to remove unknown item type.
        /// </summary>
        public static string LibraryModel_RemovingUnknowTypeError
        {
            get
            {
                string customLocalization = getMessage( "LibraryModel_RemovingUnknowTypeError" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LibraryModel_RemovingUnknowTypeError", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library is no longer available.
        /// </summary>
        public static string LibraryUnavailableAfterFailedUpdate
        {
            get
            {
                string customLocalization = getMessage( "LibraryUnavailableAfterFailedUpdate" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LibraryUnavailableAfterFailedUpdate", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No links are attached to this media item..
        /// </summary>
        public static string NO_LINKS
        {
            get
            {
                string customLocalization = getMessage( "NO_LINKS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "NO_LINKS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to links.
        /// </summary>
        public static string SHOW_LINKS
        {
            get
            {
                string customLocalization = getMessage( "SHOW_LINKS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SHOW_LINKS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to about.
        /// </summary>
        public static string ABOUT
        {
            get
            {
                string customLocalization = getMessage( "ABOUT" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ABOUT", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Version: {0}.{1}.
        /// </summary>
        public static string VERSION
        {
            get
            {
                string customLocalization = getMessage( "VERSION" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "VERSION", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to add.
        /// </summary>
        public static string MainPage_Add
        {
            get
            {
                string customLocalization = getMessage( "MainPage_Add" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_Add", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Adding library.
        /// </summary>
        public static string MainPage_AddingLibrary
        {
            get
            {
                string customLocalization = getMessage( "MainPage_AddingLibrary" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_AddingLibrary", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Catalogues: {0}.
        /// </summary>
        public static string CATALOGS
        {
            get
            {
                string customLocalization = getMessage( "CATALOGS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CATALOGS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to check for updates.
        /// </summary>
        public static string CHECK_FOR_UPDATE
        {
            get
            {
                string customLocalization = getMessage( "CHECK_FOR_UPDATE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CHECK_FOR_UPDATE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Application will be closed..
        /// </summary>
        public static string MainPage_ClosingApplicationMessage
        {
            get
            {
                string customLocalization = getMessage( "MainPage_ClosingApplicationMessage" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_ClosingApplicationMessage", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Are you sure?.
        /// </summary>
        public static string ARE_YOU_SURE
        {
            get
            {
                string customLocalization = getMessage( "ARE_YOU_SURE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ARE_YOU_SURE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Connecting.
        /// </summary>
        public static string CONNECTING
        {
            get
            {
                string customLocalization = getMessage( "CONNECTING" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "CONNECTING", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library will be removed from the device..
        /// </summary>
        public static string QUESTION_REMOVE_LIBRARY
        {
            get
            {
                string customLocalization = getMessage( "QUESTION_REMOVE_LIBRARY" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "QUESTION_REMOVE_LIBRARY", AppResources.Culture ) );
            }
        }


        /// <summary>
        ///   Looks up a localized string similar to Downloading.
        /// </summary>
        public static string MainPage_Downloading
        {
            get
            {
                string customLocalization = getMessage( "MainPage_Downloading" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_Downloading", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Downloading library.
        /// </summary>
        public static string MainPage_DownloadingLib
        {
            get
            {
                string customLocalization = getMessage( "MainPage_DownloadingLib" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_DownloadingLib", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to downloads.
        /// </summary>
        public static string MID_DOWNLOADS
        {
            get
            {
                string customLocalization = getMessage( "MID_DOWNLOADS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_DOWNLOADS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Enter NED server address.
        /// </summary>
        public static string ENTER_SERVER_ADDRESS
        {
            get
            {
                string customLocalization = getMessage( "ENTER_SERVER_ADDRESS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ENTER_SERVER_ADDRESS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to factory reset.
        /// </summary>
        public static string FACTORY_SETTINGS
        {
            get
            {
                string customLocalization = getMessage( "FACTORY_SETTINGS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "FACTORY_SETTINGS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to libraries.
        /// </summary>
        public static string LIBRARIES
        {
            get
            {
                string customLocalization = getMessage( "LIBRARIES" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LIBRARIES", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to library manager.
        /// </summary>
        public static string LIBRARY_MANAGER
        {
            get
            {
                string customLocalization = getMessage( "LIBRARY_MANAGER" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LIBRARY_MANAGER", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to log in.
        /// </summary>
        public static string LOGIN
        {
            get
            {
                string customLocalization = getMessage( "LOGIN" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LOGIN", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to New library ID.
        /// </summary>
        public static string LIBRARY_ID
        {
            get
            {
                string customLocalization = getMessage( "LIBRARY_ID" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LIBRARY_ID", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to New version available.
        /// </summary>
        public static string MainPage_NewVersionAvailableHeader
        {
            get
            {
                string customLocalization = getMessage( "MainPage_NewVersionAvailableHeader" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_NewVersionAvailableHeader", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Do you want to replace current version of this library with new version? (NOTE: all downloaded media will be deleted).
        /// </summary>
        public static string MainPage_NewVersionAvailableMessage
        {
            get
            {
                string customLocalization = getMessage( "MainPage_NewVersionAvailableMessage" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_NewVersionAvailableMessage", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No downloads pending.
        ///
        ///You can select media items to download while browsing the Library..
        /// </summary>
        public static string MainPage_NoDownloadPending
        {
            get
            {
                string customLocalization = getMessage( "MainPage_NoDownloadPending" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_NoDownloadPending", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No libraries to display. 
        ///
        ///Use Library Manager to download libraries and add them here..
        /// </summary>
        public static string MainPage_NoLibrariesToDisplay
        {
            get
            {
                string customLocalization = getMessage( "MainPage_NoLibrariesToDisplay" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_NoLibrariesToDisplay", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No libraries to display.
        ///
        ///Type in the library ID and click &apos;Add&apos; to check if library is available..
        /// </summary>
        public static string MainPage_NoLibrariesToDisplayTypeID
        {
            get
            {
                string customLocalization = getMessage( "MainPage_NoLibrariesToDisplayTypeID" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_NoLibrariesToDisplayTypeID", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to ok.
        /// </summary>
        public static string MainPage_OK
        {
            get
            {
                string customLocalization = getMessage( "MainPage_OK" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_OK", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Password.
        /// </summary>
        public static string PASSWORD
        {
            get
            {
                string customLocalization = getMessage( "PASSWORD" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "PASSWORD", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Paused.
        /// </summary>
        public static string MainPage_Paused
        {
            get
            {
                string customLocalization = getMessage( "MainPage_Paused" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_Paused", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Queued.
        /// </summary>
        public static string MainPage_Queued
        {
            get
            {
                string customLocalization = getMessage( "MainPage_Queued" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_Queued", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Remember Me!.
        /// </summary>
        public static string REMEMBERME
        {
            get
            {
                string customLocalization = getMessage( "REMEMBERME" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "REMEMBERME", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to server wizard.
        /// </summary>
        public static string SERVER_WIZARD
        {
            get
            {
                string customLocalization = getMessage( "SERVER_WIZARD" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SERVER_WIZARD", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to settings.
        /// </summary>
        public static string MID_OPTIONS_COMMAND
        {
            get
            {
                string customLocalization = getMessage( "MID_OPTIONS_COMMAND" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_OPTIONS_COMMAND", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Show Libraries.
        /// </summary>
        public static string SHOW_LIBRARY
        {
            get
            {
                string customLocalization = getMessage( "SHOW_LIBRARY" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SHOW_LIBRARY", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to statistics.
        /// </summary>
        public static string MID_STATISTICS_COMMAND
        {
            get
            {
                string customLocalization = getMessage( "MID_STATISTICS_COMMAND" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_STATISTICS_COMMAND", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopped.
        /// </summary>
        public static string MainPage_Stopped
        {
            get
            {
                string customLocalization = getMessage( "MainPage_Stopped" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_Stopped", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Update not necessary.
        /// </summary>
        public static string MainPage_UpdateNotNecessaryHeader
        {
            get
            {
                string customLocalization = getMessage( "MainPage_UpdateNotNecessaryHeader" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_UpdateNotNecessaryHeader", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to You have the current version of this library. Do you want to download it anyway? (NOTE: all downloaded media will be deleted).
        /// </summary>
        public static string MainPage_UpdateNotNecessaryMessage
        {
            get
            {
                string customLocalization = getMessage( "MainPage_UpdateNotNecessaryMessage" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_UpdateNotNecessaryMessage", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to user login.
        /// </summary>
        public static string USER_AUTHENTICATION
        {
            get
            {
                string customLocalization = getMessage( "USER_AUTHENTICATION" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "USER_AUTHENTICATION", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to User name.
        /// </summary>
        public static string USER_NAME
        {
            get
            {
                string customLocalization = getMessage( "USER_NAME" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "USER_NAME", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Views: {0}.
        /// </summary>
        public static string MainPage_ViewsCount
        {
            get
            {
                string customLocalization = getMessage( "MainPage_ViewsCount" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MainPage_ViewsCount", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Add to queue.
        /// </summary>
        public static string ADDTOQUEUE
        {
            get
            {
                string customLocalization = getMessage( "ADDTOQUEUE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ADDTOQUEUE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to download now.
        /// </summary>
        public static string DOWNLOAD_NOW
        {
            get
            {
                string customLocalization = getMessage( "DOWNLOAD_NOW" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DOWNLOAD_NOW", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No description available.
        /// </summary>
        public static string NO_DETAILS
        {
            get
            {
                string customLocalization = getMessage( "NO_DETAILS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "NO_DETAILS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to description.
        /// </summary>
        public static string SHOW_DETAILS
        {
            get
            {
                string customLocalization = getMessage( "SHOW_DETAILS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SHOW_DETAILS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to We are really sorry but we are unable to open this media item..
        /// </summary>
        public static string MediaItemViewerPage_CanNotOpenItem
        {
            get
            {
                string customLocalization = getMessage( "MediaItemViewerPage_CanNotOpenItem" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MediaItemViewerPage_CanNotOpenItem", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to picture.
        /// </summary>
        public static string MediaItemViewerPage_Title
        {
            get
            {
                string customLocalization = getMessage( "MediaItemViewerPage_Title" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MediaItemViewerPage_Title", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to We are really sorry but we are unable to show you this document..
        /// </summary>
        public static string MediaItemViewerPage_UnableToOpenDocument
        {
            get
            {
                string customLocalization = getMessage( "MediaItemViewerPage_UnableToOpenDocument" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MediaItemViewerPage_UnableToOpenDocument", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Updating library.
        /// </summary>
        public static string ProgressOverlay_UpdatingLibrary
        {
            get
            {
                string customLocalization = getMessage( "ProgressOverlay_UpdatingLibrary" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "ProgressOverlay_UpdatingLibrary", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No search results to display.
        ///
        ///Enter a valid keyword and press search button..
        /// </summary>
        public static string SearchPage_NoResultsToDisplay
        {
            get
            {
                string customLocalization = getMessage( "SearchPage_NoResultsToDisplay" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SearchPage_NoResultsToDisplay", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to search.
        /// </summary>
        public static string MID_SEARCH_TITLE
        {
            get
            {
                string customLocalization = getMessage( "MID_SEARCH_TITLE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_SEARCH_TITLE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Automatic downloads.
        /// </summary>
        public static string MID_DOWNLOAD_STATE_SETTINGS
        {
            get
            {
                string customLocalization = getMessage( "MID_DOWNLOAD_STATE_SETTINGS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_DOWNLOAD_STATE_SETTINGS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Automatic statistics upload.
        /// </summary>
        public static string STATISTICS_SENDING_MODE
        {
            get
            {
                string customLocalization = getMessage( "STATISTICS_SENDING_MODE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "STATISTICS_SENDING_MODE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Clearing data.
        /// </summary>
        public static string SettingsPage_ClearingData
        {
            get
            {
                string customLocalization = getMessage( "SettingsPage_ClearingData" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SettingsPage_ClearingData", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to All data will be removed..
        /// </summary>
        public static string SettingsPage_FactoryResetInfoMessage
        {
            get
            {
                string customLocalization = getMessage( "SettingsPage_FactoryResetInfoMessage" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SettingsPage_FactoryResetInfoMessage", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Logging out.
        /// </summary>
        public static string SettingsPage_LoggingOut
        {
            get
            {
                string customLocalization = getMessage( "SettingsPage_LoggingOut" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SettingsPage_LoggingOut", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to logout user.
        /// </summary>
        public static string SWITCH_USER
        {
            get
            {
                string customLocalization = getMessage( "SWITCH_USER" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SWITCH_USER", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Current user will be logged out..
        /// </summary>
        public static string QUESTION_LOGOUT_USER
        {
            get
            {
                string customLocalization = getMessage( "QUESTION_LOGOUT_USER" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "QUESTION_LOGOUT_USER", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to remove user.
        /// </summary>
        public static string REMOVE_USER
        {
            get
            {
                string customLocalization = getMessage( "REMOVE_USER" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "REMOVE_USER", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Removing user.
        /// </summary>
        public static string SettingsPage_RemovingUser
        {
            get
            {
                string customLocalization = getMessage( "SettingsPage_RemovingUser" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SettingsPage_RemovingUser", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Language.
        /// </summary>
        public static string LANGUAGE
        {
            get
            {
                string customLocalization = getMessage( "LANGUAGE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "LANGUAGE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to settings.
        /// </summary>
        public static string MID_SETTINGS_TITLE
        {
            get
            {
                string customLocalization = getMessage( "MID_SETTINGS_TITLE" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_SETTINGS_TITLE", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to All user data will be removed..
        /// </summary>
        public static string SettingsPage_UsersRemovedInfoMessage
        {
            get
            {
                string customLocalization = getMessage( "SettingsPage_UsersRemovedInfoMessage" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SettingsPage_UsersRemovedInfoMessage", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No media items opened since last statistics upload to server.
        ///
        ///Statistics are uploaded automatically when &apos;Automatic upload&apos; turned on in settings..
        /// </summary>
        public static string StatisticPage_NoMediaOpened
        {
            get
            {
                string customLocalization = getMessage( "StatisticPage_NoMediaOpened" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "StatisticPage_NoMediaOpened", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to There are no statistics to upload.
        /// </summary>
        public static string StatisticPage_NoStatisticToUpload
        {
            get
            {
                string customLocalization = getMessage( "StatisticPage_NoStatisticToUpload" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "StatisticPage_NoStatisticToUpload", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Started uploading statistics to server.
        /// </summary>
        public static string StatisticPage_StartedUploading
        {
            get
            {
                string customLocalization = getMessage( "StatisticPage_StartedUploading" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "StatisticPage_StartedUploading", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to statistics.
        /// </summary>
        public static string StatisticPage_Title
        {
            get
            {
                string customLocalization = getMessage( "StatisticPage_Title" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "StatisticPage_Title", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to upload.
        /// </summary>
        public static string MID_UPLOAD_COMMAND
        {
            get
            {
                string customLocalization = getMessage( "MID_UPLOAD_COMMAND" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_UPLOAD_COMMAND", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to upload statistics.
        /// </summary>
        public static string StatisticPage_UploadFiles
        {
            get
            {
                string customLocalization = getMessage( "StatisticPage_UploadFiles" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "StatisticPage_UploadFiles", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Statistics uploaded successfully.
        /// </summary>
        public static string DLM_SUCCESSFULLUPLOAD
        {
            get
            {
                string customLocalization = getMessage( "DLM_SUCCESSFULLUPLOAD" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DLM_SUCCESSFULLUPLOAD", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Off.
        /// </summary>
        public static string MID_OFF_SETTINGS
        {
            get
            {
                string customLocalization = getMessage( "MID_OFF_SETTINGS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_OFF_SETTINGS", AppResources.Culture ));
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to On.
        /// </summary>
        public static string MID_ON_SETTINGS
        {
            get
            {
                string customLocalization = getMessage( "MID_ON_SETTINGS" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MID_ON_SETTINGS", AppResources.Culture ) );
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Restart needed.
        /// </summary>
        public static string MSG_RESTART_NEEDED2
        {
            get
            {
                string customLocalization = getMessage( "MSG_RESTART_NEEDED2" );
                if( customLocalization != null )
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "MSG_RESTART_NEEDED2", AppResources.Culture ) );
            }
        }

        public static string DOWNLOAD_AGAIN_LANGUAGE
        {
            get
            {
                string customLocalization = getMessage( "DOWNLOAD_AGAIN_LANGUAGE" );
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "DOWNLOAD_AGAIN_LANGUAGE", AppResources.Culture ) );
            }
        }

        public static string NEXT
        {
            get
            {
                string customLocalization = getMessage( "NEXT" );
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "NEXT", AppResources.Culture ) );
            }
        }

        public static string HIDE
        {
            get
            {
                string customLocalization = getMessage( "HIDE" );
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "HIDE", AppResources.Culture ) );
            }
        }

        public static string SHOW_TIPS
        {
            get
            {
                string customLocalization = getMessage( "SHOW_TIPS" );
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "SHOW_TIPS", AppResources.Culture ) );
            }
        }

        public static string TIPS_ON_STARTUP
        {
            get
            {
                string customLocalization = getMessage( "TIPS_ON_STARTUP" );
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation( AppResources.ResourceManager.GetString( "TIPS_ON_STARTUP", AppResources.Culture ) );
            }
        }

        public static string TIPS_TRICKS
        {
            get
            {
                string customLocalization = getMessage("TIPS_TRICKS");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation(AppResources.ResourceManager.GetString("TIPS_TRICKS", AppResources.Culture));
            }
        }

        public static string MAX_DOWNLOAD_REACHED_QUESTION
        {
            get
            {
                string customLocalization = getMessage("MAX_DOWNLOAD_REACHED_QUESTION");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return ConvertUTFNotation(AppResources.ResourceManager.GetString("MAX_DOWNLOAD_REACHED_QUESTION", AppResources.Culture));
            }
        }

        public static string TIP(uint index)
        {
            string tipId = "TIP_" + index.ToString();
            string customLocalization = getMessage(tipId);
            if (customLocalization != null)
            {
                return customLocalization;
            }
            return ConvertUTFNotation(AppResources.ResourceManager.GetString(tipId, AppResources.Culture));
        }

        public static List<String> AllTips()
        {
            List<String> retval = new List<String>();
            int failCount = 0;
            int tipNumber = 1;
            while (failCount < 10)
            {
                string tipId = "TIP_" + tipNumber.ToString();
                string customLocalization = getMessage(tipId);
                if (customLocalization != null)
                {
                    retval.Add(customLocalization);
                }
                else
                {
                    customLocalization = ConvertUTFNotation(AppResources.ResourceManager.GetString(tipId, AppResources.Culture));
                    if (customLocalization != null)
                    {
                        retval.Add(customLocalization);
                    }
                    else
                    {
                        failCount++;
                    }
                }
                tipNumber++;
            }
            return retval;
        }
    }
}
