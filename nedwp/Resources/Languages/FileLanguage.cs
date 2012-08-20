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

namespace NedWp.Resources.Languages
{
    using System;
    using System.Xml.Linq;
    using System.IO.IsolatedStorage;
    using System.IO;
    using System.Linq;

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
            if(currentLanguageId == null || currentLanguageId == "0")
            {
                return;
            }
            string localizationFilePath = NedEngine.Utils.LocalizationsFilePath(currentLanguageId);

            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(localizationFilePath))
                {
                    using (IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(localizationFilePath, FileMode.Open, file))
                    {
                        LocalizationFile = XDocument.Load(isfStream);
                    }
                }
                else
                {
                    LocalizationFile = new XDocument();
                }
            }
        }

        private static string getMessage(string name)
        {
            if(_instance.LocalizationFile == null || _instance.LocalizationFile.Root == null)
            {
                return null;
            }
           var translationQuery = from translation in _instance.LocalizationFile.Root.Elements(TAG_DATA)
                                     where translation.Attribute(ATTRIBUTE_NAME).Value == name
                                           select new
                                           {
                                               Translation = translation.Element(TAG_VALUE).Value
                                           };
            if (translationQuery.Count() == 0) // Page does not have help
                return null;
            return translationQuery.First().Translation;
        }

        /// <summary>
        ///   Looks up a localized string similar to NOKIA EDUCATION DELIVERY.
        /// </summary>
        public static string App_ApplicationTitle
        {
            get
            {
                string customLocalization = getMessage("App_ApplicationTitle");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("App_ApplicationTitle", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Welcome.
        /// </summary>
        public static string App_DefaultMOTD
        {
            get
            {
                string customLocalization = getMessage("App_DefaultMOTD");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("App_DefaultMOTD", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        public static string App_HelpButtonContent
        {
            get
            {
                string customLocalization = getMessage("App_HelpButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("App_HelpButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No help available for this screen.
        /// </summary>
        public static string App_OpeningHelpErrorUnknowScreen
        {
            get
            {
                string customLocalization = getMessage("App_OpeningHelpErrorUnknowScreen");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("App_OpeningHelpErrorUnknowScreen", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Categories: {0}.
        /// </summary>
        public static string CatalogueModelItem_CategoriesCount
        {
            get
            {
                string customLocalization = getMessage("CatalogueModelItem_CategoriesCount");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CatalogueModelItem_CategoriesCount", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        public static string CataloguePage_DeleteButton
        {
            get
            {
                string customLocalization = getMessage("CataloguePage_DeleteButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CataloguePage_DeleteButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to download all.
        /// </summary>
        public static string CataloguePage_DownloadAllButton
        {
            get
            {
                string customLocalization = getMessage("CataloguePage_DownloadAllButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CataloguePage_DownloadAllButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to back.
        /// </summary>
        public static string CataloguePage_HomeButton
        {
            get
            {
                string customLocalization = getMessage("CataloguePage_HomeButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CataloguePage_HomeButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to refresh.
        /// </summary>
        public static string CataloguePage_RefreshButton
        {
            get
            {
                string customLocalization = getMessage("CataloguePage_RefreshButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CataloguePage_RefreshButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to search.
        /// </summary>
        public static string CataloguePage_SearchButton
        {
            get
            {
                string customLocalization = getMessage("CataloguePage_SearchButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CataloguePage_SearchButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Media items enqueued for download: {0}.
        /// </summary>
        public static string CategoryModelItem_ItemsForDownload
        {
            get
            {
                string customLocalization = getMessage("CategoryModelItem_ItemsForDownload");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CategoryModelItem_ItemsForDownload", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Media items: {0} (downloaded: {1}, on server: {2} ).
        /// </summary>
        public static string CategoryModelItem_MediaItems
        {
            get
            {
                string customLocalization = getMessage("CategoryModelItem_MediaItems");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("CategoryModelItem_MediaItems", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Demo library id &apos;khan&apos;.
        /// </summary>
        public static string DemoLibId
        {
            get
            {
                string customLocalization = getMessage("DemoLibId");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DemoLibId", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to For demo please use login and password &apos;guest&apos;.
        /// </summary>
        public static string DemoLoginDetails
        {
            get
            {
                string customLocalization = getMessage("DemoLoginDetails");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DemoLoginDetails", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to For demo please use:.
        /// </summary>
        public static string DEMOURL
        {
            get
            {
                string customLocalization = getMessage("DEMOURL");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DEMOURL", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} added to download queue.
        /// </summary>
        public static string DownloadCommand_AddedToDownload
        {
            get
            {
                string customLocalization = getMessage("DownloadCommand_AddedToDownload");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DownloadCommand_AddedToDownload", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} is already downloaded.
        /// </summary>
        public static string DownloadCommand_AlreadyDownloaded
        {
            get
            {
                string customLocalization = getMessage("DownloadCommand_AlreadyDownloaded");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DownloadCommand_AlreadyDownloaded", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to {0} downloading started.
        /// </summary>
        public static string DownloadCommand_DownloadingStarted
        {
            get
            {
                string customLocalization = getMessage("DownloadCommand_DownloadingStarted");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DownloadCommand_DownloadingStarted", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Item.
        /// </summary>
        public static string DownloadCommand_Item
        {
            get
            {
                string customLocalization = getMessage("DownloadCommand_Item");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DownloadCommand_Item", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to cancel.
        /// </summary>
        public static string DownloadListItemControl_CancelButton
        {
            get
            {
                string customLocalization = getMessage("DownloadListItemControl_CancelButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("DownloadListItemControl_CancelButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Could not connect with server. Make sure your network connection is working and try again..
        /// </summary>
        public static string Error_Connection
        {
            get
            {
                string customLocalization = getMessage("Error_Connection");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_Connection", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Username and password cannot be empty.
        /// </summary>
        public static string Error_EmptyUsernameOrPassword
        {
            get
            {
                string customLocalization = getMessage("Error_EmptyUsernameOrPassword");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_EmptyUsernameOrPassword", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid username or password.
        /// </summary>
        public static string Error_InvalidCredentials
        {
            get
            {
                string customLocalization = getMessage("Error_InvalidCredentials");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_InvalidCredentials", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid NED server address.
        /// </summary>
        public static string Error_InvalidServerAddress
        {
            get
            {
                string customLocalization = getMessage("Error_InvalidServerAddress");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_InvalidServerAddress", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library is already added.
        /// </summary>
        public static string Error_LibraryAlreadyAdded
        {
            get
            {
                string customLocalization = getMessage("Error_LibraryAlreadyAdded");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_LibraryAlreadyAdded", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library was deleted from server.
        /// </summary>
        public static string Error_LibraryDeletedFromServer
        {
            get
            {
                string customLocalization = getMessage("Error_LibraryDeletedFromServer");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_LibraryDeletedFromServer", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library doesn&apos;t exist..
        /// </summary>
        public static string Error_LibraryDoesNotExist
        {
            get
            {
                string customLocalization = getMessage("Error_LibraryDoesNotExist");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_LibraryDoesNotExist", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library id cannot be empty..
        /// </summary>
        public static string Error_LibraryIdEmpty
        {
            get
            {
                string customLocalization = getMessage("Error_LibraryIdEmpty");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_LibraryIdEmpty", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Unexpected error has occured..
        /// </summary>
        public static string Error_Unexpected
        {
            get
            {
                string customLocalization = getMessage("Error_Unexpected");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Error_Unexpected", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Help not available.
        /// </summary>
        public static string HelpPage_HelpNotAvailable
        {
            get
            {
                string customLocalization = getMessage("HelpPage_HelpNotAvailable");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("HelpPage_HelpNotAvailable", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to help.
        /// </summary>
        public static string HelpPage_Title
        {
            get
            {
                string customLocalization = getMessage("HelpPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("HelpPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Select Language.
        /// </summary>
        public static string Language_Title
        {
            get
            {
                string customLocalization = getMessage("Language_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("Language_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Tried to remove unknown item type.
        /// </summary>
        public static string LibraryModel_RemovingUnknowTypeError
        {
            get
            {
                string customLocalization = getMessage("LibraryModel_RemovingUnknowTypeError");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("LibraryModel_RemovingUnknowTypeError", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library is no longer available.
        /// </summary>
        public static string LibraryUnavailableAfterFailedUpdate
        {
            get
            {
                string customLocalization = getMessage("LibraryUnavailableAfterFailedUpdate");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("LibraryUnavailableAfterFailedUpdate", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No links are attached to this media item..
        /// </summary>
        public static string LinkPage_NoLinksInfo
        {
            get
            {
                string customLocalization = getMessage("LinkPage_NoLinksInfo");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("LinkPage_NoLinksInfo", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to links.
        /// </summary>
        public static string LinksPage_Title
        {
            get
            {
                string customLocalization = getMessage("LinksPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("LinksPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to about.
        /// </summary>
        public static string MainPage_AboutButtonText
        {
            get
            {
                string customLocalization = getMessage("MainPage_AboutButtonText");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_AboutButtonText", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Version: {0}.{1}.
        /// </summary>
        public static string MainPage_AboutVersionInfo
        {
            get
            {
                string customLocalization = getMessage("MainPage_AboutVersionInfo");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_AboutVersionInfo", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to add.
        /// </summary>
        public static string MainPage_Add
        {
            get
            {
                string customLocalization = getMessage("MainPage_Add");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Add", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Adding library.
        /// </summary>
        public static string MainPage_AddingLibrary
        {
            get
            {
                string customLocalization = getMessage("MainPage_AddingLibrary");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_AddingLibrary", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Catalogues: {0}.
        /// </summary>
        public static string MainPage_CataloguesCount
        {
            get
            {
                string customLocalization = getMessage("MainPage_CataloguesCount");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_CataloguesCount", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to check for updates.
        /// </summary>
        public static string MainPage_CheckForUpdates
        {
            get
            {
                string customLocalization = getMessage("MainPage_CheckForUpdates");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_CheckForUpdates", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Application will be closed..
        /// </summary>
        public static string MainPage_ClosingApplicationMessage
        {
            get
            {
                string customLocalization = getMessage("MainPage_ClosingApplicationMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ClosingApplicationMessage", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Are you sure?.
        /// </summary>
        public static string MainPage_ClosingApplicationTitle
        {
            get
            {
                string customLocalization = getMessage("MainPage_ClosingApplicationTitle");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ClosingApplicationTitle", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Connecting.
        /// </summary>
        public static string MainPage_Connecting
        {
            get
            {
                string customLocalization = getMessage("MainPage_Connecting");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Connecting", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Connecting to server.
        /// </summary>
        public static string MainPage_ConnectingToServer
        {
            get
            {
                string customLocalization = getMessage("MainPage_ConnectingToServer");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ConnectingToServer", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        public static string MainPage_Delete
        {
            get
            {
                string customLocalization = getMessage("MainPage_Delete");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Delete", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Library will be removed from the device..
        /// </summary>
        public static string MainPage_DeleteLibQuestionMessage
        {
            get
            {
                string customLocalization = getMessage("MainPage_DeleteLibQuestionMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_DeleteLibQuestionMessage", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Are you sure?.
        /// </summary>
        public static string MainPage_DeleteLibQuestionTitile
        {
            get
            {
                string customLocalization = getMessage("MainPage_DeleteLibQuestionTitile");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_DeleteLibQuestionTitile", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Downloading.
        /// </summary>
        public static string MainPage_Downloading
        {
            get
            {
                string customLocalization = getMessage("MainPage_Downloading");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Downloading", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Downloading library.
        /// </summary>
        public static string MainPage_DownloadingLib
        {
            get
            {
                string customLocalization = getMessage("MainPage_DownloadingLib");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_DownloadingLib", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to downloads.
        /// </summary>
        public static string MainPage_Downloads
        {
            get
            {
                string customLocalization = getMessage("MainPage_Downloads");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Downloads", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Enter NED server address.
        /// </summary>
        public static string MainPage_EnterServerAddress
        {
            get
            {
                string customLocalization = getMessage("MainPage_EnterServerAddress");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_EnterServerAddress", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to factory reset.
        /// </summary>
        public static string MainPage_FactoryResetMenuItemText
        {
            get
            {
                string customLocalization = getMessage("MainPage_FactoryResetMenuItemText");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_FactoryResetMenuItemText", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to libraries.
        /// </summary>
        public static string MainPage_Libraries
        {
            get
            {
                string customLocalization = getMessage("MainPage_Libraries");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Libraries", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to library manager.
        /// </summary>
        public static string MainPage_LibraryManager
        {
            get
            {
                string customLocalization = getMessage("MainPage_LibraryManager");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_LibraryManager", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Logging in.
        /// </summary>
        public static string MainPage_LoggingIn
        {
            get
            {
                string customLocalization = getMessage("MainPage_LoggingIn");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_LoggingIn", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to log in.
        /// </summary>
        public static string MainPage_logIn
        {
            get
            {
                string customLocalization = getMessage("MainPage_logIn");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_logIn", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to New library ID.
        /// </summary>
        public static string MainPage_NewLibraryID
        {
            get
            {
                string customLocalization = getMessage("MainPage_NewLibraryID");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NewLibraryID", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to New version available.
        /// </summary>
        public static string MainPage_NewVersionAvailableHeader
        {
            get
            {
                string customLocalization = getMessage("MainPage_NewVersionAvailableHeader");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NewVersionAvailableHeader", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Do you want to replace current version of this library with new version? (NOTE: all downloaded media will be deleted).
        /// </summary>
        public static string MainPage_NewVersionAvailableMessage
        {
            get
            {
                string customLocalization = getMessage("MainPage_NewVersionAvailableMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NewVersionAvailableMessage", AppResources.Culture);
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
                string customLocalization = getMessage("MainPage_NoDownloadPending");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NoDownloadPending", AppResources.Culture);
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
                string customLocalization = getMessage("MainPage_NoLibrariesToDisplay");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NoLibrariesToDisplay", AppResources.Culture);
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
                string customLocalization = getMessage("MainPage_NoLibrariesToDisplayTypeID");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_NoLibrariesToDisplayTypeID", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to ok.
        /// </summary>
        public static string MainPage_OK
        {
            get
            {
                string customLocalization = getMessage("MainPage_OK");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_OK", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Password.
        /// </summary>
        public static string MainPage_Password
        {
            get
            {
                string customLocalization = getMessage("MainPage_Password");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Password", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Paused.
        /// </summary>
        public static string MainPage_Paused
        {
            get
            {
                string customLocalization = getMessage("MainPage_Paused");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Paused", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Queued.
        /// </summary>
        public static string MainPage_Queued
        {
            get
            {
                string customLocalization = getMessage("MainPage_Queued");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Queued", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Remember Me!.
        /// </summary>
        public static string MainPage_RememberMe
        {
            get
            {
                string customLocalization = getMessage("MainPage_RememberMe");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_RememberMe", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to server wizard.
        /// </summary>
        public static string MainPage_ServerWizard
        {
            get
            {
                string customLocalization = getMessage("MainPage_ServerWizard");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ServerWizard", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to settings.
        /// </summary>
        public static string MainPage_SettingsButtonContent
        {
            get
            {
                string customLocalization = getMessage("MainPage_SettingsButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_SettingsButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Show Libraries.
        /// </summary>
        public static string MainPage_ShowLibraries
        {
            get
            {
                string customLocalization = getMessage("MainPage_ShowLibraries");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ShowLibraries", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to statistics.
        /// </summary>
        public static string MainPage_StatisticsButtonContent
        {
            get
            {
                string customLocalization = getMessage("MainPage_StatisticsButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_StatisticsButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Stopped.
        /// </summary>
        public static string MainPage_Stopped
        {
            get
            {
                string customLocalization = getMessage("MainPage_Stopped");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_Stopped", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Update not necessary.
        /// </summary>
        public static string MainPage_UpdateNotNecessaryHeader
        {
            get
            {
                string customLocalization = getMessage("MainPage_UpdateNotNecessaryHeader");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_UpdateNotNecessaryHeader", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to You have the current version of this library. Do you want to download it anyway? (NOTE: all downloaded media will be deleted).
        /// </summary>
        public static string MainPage_UpdateNotNecessaryMessage
        {
            get
            {
                string customLocalization = getMessage("MainPage_UpdateNotNecessaryMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_UpdateNotNecessaryMessage", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to user login.
        /// </summary>
        public static string MainPage_UserLogin
        {
            get
            {
                string customLocalization = getMessage("MainPage_UserLogin");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_UserLogin", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to User name.
        /// </summary>
        public static string MainPage_UserName
        {
            get
            {
                string customLocalization = getMessage("MainPage_UserName");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_UserName", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Views: {0}.
        /// </summary>
        public static string MainPage_ViewsCount
        {
            get
            {
                string customLocalization = getMessage("MainPage_ViewsCount");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MainPage_ViewsCount", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Add to queue.
        /// </summary>
        public static string MediaItemControl_AddToQueue
        {
            get
            {
                string customLocalization = getMessage("MediaItemControl_AddToQueue");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemControl_AddToQueue", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to delete.
        /// </summary>
        public static string MediaItemControl_DeleteButton
        {
            get
            {
                string customLocalization = getMessage("MediaItemControl_DeleteButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemControl_DeleteButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to download now.
        /// </summary>
        public static string MediaItemControl_DonwloadNowButton
        {
            get
            {
                string customLocalization = getMessage("MediaItemControl_DonwloadNowButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemControl_DonwloadNowButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No description available.
        /// </summary>
        public static string MediaItemsListModelItem_NoDescriptionAvailable
        {
            get
            {
                string customLocalization = getMessage("MediaItemsListModelItem_NoDescriptionAvailable");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemsListModelItem_NoDescriptionAvailable", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to description.
        /// </summary>
        public static string MediaItemsViewPage_DescriptionButtonText
        {
            get
            {
                string customLocalization = getMessage("MediaItemsViewPage_DescriptionButtonText");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemsViewPage_DescriptionButtonText", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to links.
        /// </summary>
        public static string MediaItemsViewPage_LinksButtonText
        {
            get
            {
                string customLocalization = getMessage("MediaItemsViewPage_LinksButtonText");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemsViewPage_LinksButtonText", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to We are really sorry but we are unable to open this media item..
        /// </summary>
        public static string MediaItemViewerPage_CanNotOpenItem
        {
            get
            {
                string customLocalization = getMessage("MediaItemViewerPage_CanNotOpenItem");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemViewerPage_CanNotOpenItem", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to picture.
        /// </summary>
        public static string MediaItemViewerPage_Title
        {
            get
            {
                string customLocalization = getMessage("MediaItemViewerPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemViewerPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to We are really sorry but we are unable to show you this document..
        /// </summary>
        public static string MediaItemViewerPage_UnableToOpenDocument
        {
            get
            {
                string customLocalization = getMessage("MediaItemViewerPage_UnableToOpenDocument");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("MediaItemViewerPage_UnableToOpenDocument", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Updating library.
        /// </summary>
        public static string ProgressOverlay_UpdatingLibrary
        {
            get
            {
                string customLocalization = getMessage("ProgressOverlay_UpdatingLibrary");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("ProgressOverlay_UpdatingLibrary", AppResources.Culture);
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
                string customLocalization = getMessage("SearchPage_NoResultsToDisplay");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SearchPage_NoResultsToDisplay", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to search.
        /// </summary>
        public static string SearchPage_Title
        {
            get
            {
                string customLocalization = getMessage("SearchPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SearchPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Automatic downloads.
        /// </summary>
        public static string SettingsPage_AutomaticDownloads
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_AutomaticDownloads");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_AutomaticDownloads", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Automatic statistics upload.
        /// </summary>
        public static string SettingsPage_AutomaticStatisticsUpload
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_AutomaticStatisticsUpload");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_AutomaticStatisticsUpload", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Clearing data.
        /// </summary>
        public static string SettingsPage_ClearingData
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_ClearingData");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_ClearingData", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to factory reset.
        /// </summary>
        public static string SettingsPage_FactoryResetButtonContent
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_FactoryResetButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_FactoryResetButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to All data will be removed..
        /// </summary>
        public static string SettingsPage_FactoryResetInfoMessage
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_FactoryResetInfoMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_FactoryResetInfoMessage", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Are you sure?.
        /// </summary>
        public static string SettingsPage_InfoHeader
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_InfoHeader");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_InfoHeader", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Logging out.
        /// </summary>
        public static string SettingsPage_LoggingOut
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_LoggingOut");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_LoggingOut", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to logout user.
        /// </summary>
        public static string SettingsPage_LogoutButtonContent
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_LogoutButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_LogoutButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Current user will be logged out..
        /// </summary>
        public static string SettingsPage_LogoutInfoMessage
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_LogoutInfoMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_LogoutInfoMessage", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to remove user.
        /// </summary>
        public static string SettingsPage_RemoveUserButtonContent
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_RemoveUserButtonContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_RemoveUserButtonContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Removing user.
        /// </summary>
        public static string SettingsPage_RemovingUser
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_RemovingUser");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_RemovingUser", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Language.
        /// </summary>
        public static string SettingsPage_SelectLanguageContent
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_SelectLanguageContent");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_SelectLanguageContent", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to settings.
        /// </summary>
        public static string SettingsPage_Title
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to All user data will be removed..
        /// </summary>
        public static string SettingsPage_UsersRemovedInfoMessage
        {
            get
            {
                string customLocalization = getMessage("SettingsPage_UsersRemovedInfoMessage");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("SettingsPage_UsersRemovedInfoMessage", AppResources.Culture);
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
                string customLocalization = getMessage("StatisticPage_NoMediaOpened");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_NoMediaOpened", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to There are no statistics to upload.
        /// </summary>
        public static string StatisticPage_NoStatisticToUpload
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_NoStatisticToUpload");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_NoStatisticToUpload", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Started uploading statistics to server.
        /// </summary>
        public static string StatisticPage_StartedUploading
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_StartedUploading");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_StartedUploading", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to statistics.
        /// </summary>
        public static string StatisticPage_Title
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_Title");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_Title", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to upload.
        /// </summary>
        public static string StatisticPage_UploadButton
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_UploadButton");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_UploadButton", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Failed to upload statistics.
        /// </summary>
        public static string StatisticPage_UploadFiles
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_UploadFiles");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_UploadFiles", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Statistics uploaded successfully.
        /// </summary>
        public static string StatisticPage_UploadingSucces
        {
            get
            {
                string customLocalization = getMessage("StatisticPage_UploadingSucces");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("StatisticPage_UploadingSucces", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Off.
        /// </summary>
        public static string ToggleSwitch_OFF
        {
            get
            {
                string customLocalization = getMessage("ToggleSwitch_OFF");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("ToggleSwitch_OFF", AppResources.Culture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to On.
        /// </summary>
        public static string ToggleSwitch_ON
        {
            get
            {
                string customLocalization = getMessage("ToggleSwitch_ON");
                if (customLocalization != null)
                {
                    return customLocalization;
                }
                return AppResources.ResourceManager.GetString("ToggleSwitch_ON", AppResources.Culture);
            }
        }
    }
}
