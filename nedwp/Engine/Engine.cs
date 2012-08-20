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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using NedWp;
using NedWp.Resources.Languages;

namespace NedEngine
{
    public class Engine : PropertyNotifierBase, ILoggedUser
    {

        public const string KDemoURL = "http://217.74.73.16:8083";

        public Engine()
        {
            ApplicationSettings = new ApplicationSettings();
            Transport = new Transport(this, ApplicationSettings);
            UpdateMotd();
        }

        public readonly ApplicationSettings ApplicationSettings;
        private readonly Transport Transport;

        private LibraryModel mViewModel = null;
        public LibraryModel LibraryModel
        {
            get
            {
                if (mViewModel == null)
                    mViewModel = new LibraryModel(DownloadManager, DownloadEnqueuedEvent);

                return mViewModel;
            }
        }

        private UserDatabase _userDatabase = null;
        public UserDatabase UserDatabase
        {
            get
            {
                if (_userDatabase == null)
                    _userDatabase = new UserDatabase();

                return _userDatabase;
            }
        }

        private StatisticsManager _statisticsManager = null;
        public StatisticsManager StatisticsManager
        {
            get
            {
                if (_statisticsManager == null)
                {
                    _statisticsManager = new StatisticsManager(this, ApplicationSettings, Transport);
                }

                return _statisticsManager;
            }
        }

        private DownloadManager _downloadManager = null;
        private DownloadManager DownloadManager
        {
            get
            {
                if (_downloadManager == null)
                {
                    _downloadManager = new DownloadManager(Transport);
                    SubscribeForDownloadManagerEvents();
                }

                return _downloadManager;
            }
        }

        private User _loggedUser;
        public User LoggedUser
        {
            get
            {
                return _loggedUser;
            }

            private set
            {
                if (value != _loggedUser)
                {
                    if (_loggedUser != null)
                    {
                        _loggedUser.Settings.PropertyChanged -= OnAutomaticStatisticUploadChanged;
                    }

                    _loggedUser = value;

                    processUserLogged();
                   
                    OnPropertyChanged("LoggedUser");
                }
            }
        }

        public void processUserLogged()
        {
            if (_loggedUser != null)
            {
                if (_loggedUser.Settings.AutomaticDownloads == false)
                {
                    foreach (QueuedDownload download in _loggedUser.Downloads)
                    {
                        download.State = QueuedDownload.DownloadState.Paused;
                    }
                }
                DownloadManager.InitializeQueue();

                _loggedUser.Settings.PropertyChanged += OnAutomaticStatisticUploadChanged;
                UpdateAutomaticStatisticUploadSubscription();
            }
        }

        public static bool IsDemoServerSelected()
        {
            return App.Engine != null && App.Engine.ApplicationSettings != null && App.Engine.ApplicationSettings.ServerUrl != null
                        && ( App.Engine.ApplicationSettings.ServerUrl.Equals( new Uri( NedEngine.Engine.KDemoURL ) )
#if DEBUG
 || App.Engine.ApplicationSettings.ServerUrl.Equals( new Uri( "http://10.133.105.67:8083" ) )
#endif
 );
        }

        #region ServerSelection

        public IObservable<Unit> SaveServer(string urlString)
        {
            return Observable.Return(urlString)
                             .SelectMany(url => Transport.CheckServer(new Uri(url)))
                             .ObserveOnDispatcher()
                             .Do(_ => ApplicationSettings.ServerUrl = new Uri(urlString));
        }

        #endregion ServerSelection

        #region Login
        private class Credentials
        {
            public Credentials(string username, string password)
            {
                Username = username;
                Password = password;
            }

            public string Username { get; private set; }
            public string Password { get; private set; }
        }

        public IObservable<Unit> Login(string username, string password)
        {
            var credentials = Observable.Return(new Credentials(username, password));
            var dbCheck = credentials.SelectMany(cred =>
                {
                    if (String.IsNullOrEmpty(cred.Username) || String.IsNullOrEmpty(cred.Password))
                    {
                        return Observable.Throw<User>(new ArgumentException(AppResources.Error_EmptyUsernameOrPassword));
                    }

                    foreach (var user in from u in UserDatabase.Users where u.Username == cred.Username select u)
                    {
                        if (user.Password == cred.Password)
                        {
                            return Observable.Return<User>(user);
                        }
                        else
                        {
                            return Observable.Throw<User>(new ArgumentException(AppResources.Error_InvalidCredentials));
                        }
                    }

                    return Transport.CheckUser(cred.Username, cred.Password)
                                    .ObserveOnDispatcher()
                                    .Select(_ => CreateUser(username, password));
                });

            return dbCheck.ObserveOnDispatcher()
                 .Do(user => 
                     {
                         LoggedUser = user;
                         App.Engine.StatisticsManager.LogUserLogin(_loggedUser);
                     })
                 .Select(_ => new Unit());
        }

        public delegate void LogoutEventHandler(object source, EventArgs args);
        public event LogoutEventHandler OnLogoutCompleted;

        public IObservable<QueuedDownload> Logout()
        {
            return DownloadManager
                .CancelAllDownloads()
                .ObserveOnDispatcher()
                .Finally(DoLogout);
        }

        private void DoLogout()
        {
            LoggedUser = null;
            BroadcastLogoutEvent();
        }

        public delegate void FactoryResetEventHandler(object source, EventArgs args);
        public event FactoryResetEventHandler OnFactoryResetCompleted;

        public IObservable<QueuedDownload> FactoryReset()
        {
            return DownloadManager
                .CancelAllDownloads()
                .ObserveOnDispatcher()
                .Finally(
                    () =>
                    {
                        RemoveUsers(UserDatabase.Users.ToArray());
                        ApplicationSettings.ServerUrl = null;
                        StatisticsManager.Statistics.Clear();
                        DoLogout();
                        BroadcastFactoryResetEvent();
                    });
        }

        public void RememberUsernameAndPassword(string username, string password)
        {
            if (ApplicationSettings.RememberMe)
            {
                ApplicationSettings.RememberedLogin = username;
                ApplicationSettings.RememberedPassword = password;
            }
            else
            {
                ApplicationSettings.RememberedLogin = String.Empty;
                ApplicationSettings.RememberedPassword = String.Empty;
            }
        }

        private User CreateUser(string username, string password)
        {
            User newUser = new User(username, password);
            UserDatabase.Users.Add(newUser);

            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            {
                appDirectory.CreateDirectory(newUser.LocalId.ToString());
                appDirectory.CreateDirectory("shared/transfers/" + newUser.LocalId.ToString());
            }

            return newUser;
        }

        public void RemoveUsers(params User[] users)
        {
            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (User user in users)
                {
                    user.StopSaving();
                    appDirectory.RecursivelyDeleteDirectory(user.LocalId.ToString());
                }
            }

            foreach (User user in users)
            {
                UserDatabase.Users.Remove(user);
                App.Engine.StatisticsManager.LogUserDelete(user);
            }
        }

        private void BroadcastLogoutEvent()
        {
            if (OnLogoutCompleted != null)
            {
                OnLogoutCompleted(this, new EventArgs());
            }
        }

        private void BroadcastFactoryResetEvent()
        {
            if (OnFactoryResetCompleted != null)
            {
                OnFactoryResetCompleted(this, new EventArgs());
            }
        }

        #endregion Login

        #region LibraryList

        public class LibraryInfo
        {
            public LibraryInfo(string id, string title, int version)
            {
                Id = id;
                Title = title;
                Version = version;
            }

            public string Id { get; private set; }
            public string Title { get; private set; }
            public int Version { get; private set; }
        }

        public IObservable<Unit> AddLibrary(string libraryId)
        {
            return Observable.Return(libraryId)
                             .SelectMany(id =>
                                 {
                                     if (String.IsNullOrEmpty(id))
                                     {
                                         return Observable.Throw<LibraryInfo>(new ArgumentException(AppResources.Error_LibraryIdEmpty));
                                     }

                                     if (LoggedUser.Libraries.Count(library => library.ServerId == libraryId) != 0)
                                     {
                                         return Observable.Throw<LibraryInfo>(new ArgumentException(AppResources.Error_LibraryAlreadyAdded));
                                     }

                                     return Transport.GetLibraryInfo(id);
                                 })
                             .ObserveOnDispatcher()
                             .Do(libInfo =>
                                 {
                                     Library library = new Library(libInfo.Id, libInfo.Title, libInfo.Version);
                                     StatisticsManager.LogAddLibrary(library);
                                     LoggedUser.Libraries.Add(library);
                                 })
                             .Select(_ => new Unit());
        }

        public IObservable<LibraryInfo> GetLibraryInfo(Library library)
        {
            return Transport.GetLibraryInfo(library.ServerId);
        }

        public IObservable<Library> DownloadLibrary(Library library)
        {
            return Transport.GetLibraryXml(library.ServerId)
                            .ObserveOnDispatcher()
                            .Do(libUpdate =>
                               {
                                   library.Version = libUpdate.Version;
                                   library.CatalogueCount = LibraryModel.GetCatalogueCount(libUpdate.Contents);
                                   Library.SaveLibraryContents(libUpdate.Contents, library, LoggedUser);
                                   Library.PrepareDiffXml(library, LoggedUser);
                               })
                           .Select(_ => library);
        }

        public void DeleteLibrary(Library library)
        {
            LoggedUser.Libraries.Remove(library);
            CancelDownloadsFromLibrary(library).Subscribe(
                lib =>
                {
                    App.Engine.StatisticsManager.LogRemoveLibrary(lib);
                    DeleteLibraryData(lib);
                });
        }

        public IObservable<Library> CancelDownloadsFromLibrary(Library library)
        {
            var downloadsInLibrary = LoggedUser.Downloads.Where(download => download.LibraryId == library.ServerId).ToList();
            LoggedUser.Downloads.Remove(downloadsInLibrary);

            return DownloadManager.CancelDownloads(downloadsInLibrary)
                                  .FinishedToNext()
                                  .Select(_ => library);
        }

        public void DeleteMediaItem(MediaItemsListModelItem item)
        {
            var downloads = LoggedUser.Downloads.Where(dl => dl.Id == item.Id).ToList();

            if (downloads.Count > 0)
            {
                DownloadManager.CancelDownloads(downloads)
                    .ObserveOnDispatcher()
                    .Finally(() =>
                    {
                        using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            foreach (var download in downloads)
                            {
                                string path = Utils.MediaFilePath(LoggedUser, download);
                                if (isf.FileExists(path))
                                {
                                    isf.DeleteFile(path);
                                }
                            }
                        }
                    })
                    .Subscribe();

                LoggedUser.Downloads.Remove(downloads);
            }
            else
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string path = Utils.MediaFilePath(LoggedUser, item);
                    if (isf.FileExists(path))
                    {
                        isf.DeleteFile(path);
                    }
                }
            }
        }

        public void DeleteLibraryData(Library library)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string libraryDirectory = Utils.LibraryDirPath(LoggedUser, library);
                if (isf.DirectoryExists(libraryDirectory))
                {
                    isf.RecursivelyDeleteDirectory(libraryDirectory);
                }
            }
        }

        public void PrepareToUpdateLibraryData(Library library)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string libraryDirectory = Utils.LibraryDirPath(LoggedUser, library);
                if (isf.DirectoryExists(libraryDirectory))
                {
                    if (isf.FileExists(Utils.LibraryXmlPath(LoggedUser, library)) && !isf.FileExists(Utils.LibraryXmlPreviousPath(LoggedUser, library)))
                    {
                        isf.MoveFile(Utils.LibraryXmlPath(LoggedUser, library), Utils.LibraryXmlPreviousPath(LoggedUser, library));
                    }
                    else
                    {
                        //shouldnt happen, will lose recent history
                        isf.DeleteFile(Utils.LibraryXmlPath(LoggedUser, library));
                    }
                }
            }
        }

        public IObservable<Library> UpdateLibrary(Library library)
        {
            return CheckForUpdates(library)
                   .SelectMany(CancelDownloadsFromLibrary)
                   .Do(
                   lib =>
                   {
                       PrepareToUpdateLibraryData(lib);
                       lib.CatalogueCount = -1;
                   })
                   .SelectMany(DownloadLibrary);
        }

        private IObservable<Library> CheckForUpdates(Library library)
        {
            if (library.CatalogueCount == -1)
            {
                return Observable.Return<Library>(library);
            }
            else
            {
                return App.Engine.GetLibraryInfo(library)
                                 .ObserveOnDispatcher()
                                 .SelectMany(libraryInfo => AskUserAboutLibraryUpdate(library, libraryInfo));
            }
        }

        private IObservable<Library> AskUserAboutLibraryUpdate(Library library, NedEngine.Engine.LibraryInfo updatedLibraryInfo)
        {
            string dialogMessage;
            string dialogHeader;
            if (updatedLibraryInfo.Version == library.Version)
            {
                dialogHeader = AppResources.MainPage_UpdateNotNecessaryHeader;
                dialogMessage = AppResources.MainPage_UpdateNotNecessaryMessage;
            }
            else
            {
                dialogHeader = AppResources.MainPage_NewVersionAvailableHeader;
                dialogMessage = AppResources.MainPage_NewVersionAvailableMessage;
            }

            if (MessageBox.Show(dialogMessage, dialogHeader, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                return Observable.Return(library);
            }
            else
            {
                return Observable.Empty<Library>();
            }
        }

        #endregion LibraryList

        #region Statistics
     

        private void UploadStatistics(object sender, EventArgs args)
        {
            StatisticsManager.AutomaticallyUpdateStatistic();
        }

        private void OnAutomaticStatisticUploadChanged(object sender, EventArgs args)
        {
            UpdateAutomaticStatisticUploadSubscription();
        }

        private void UpdateAutomaticStatisticUploadSubscription()
        {
            Transport.NetworkRequestStarted -= UploadStatistics;
            if (LoggedUser != null & LoggedUser.Settings.AutomaticStatisticsUpload)
            {
                Transport.NetworkRequestStarted += UploadStatistics;
            }
        }

        #endregion Statistics

        #region MOTD

        public string _motd;
        public string MOTD
        {
            get { return _motd; }
            set
            {
                _motd = value;
                OnPropertyChanged("MOTD");
            }
        }

        public void RequestMotdUpdate()
        {
            Transport.GetMotd()
                     .ObserveOnDispatcher()
                     .Finally(UpdateMotd)
                     .Subscribe<string>(
                     motd => SaveMotd(motd),
                     error => SaveMotd(null));
        }

        private void UpdateMotd()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(Constants.KMotdSetting))
            {
                // Set default MOTD
                SaveMotd(AppResources.App_DefaultMOTD);
            }
            MOTD = IsolatedStorageSettings.ApplicationSettings[Constants.KMotdSetting] as string;
        }

        private void SaveMotd(string motd)
        {
            IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
            appSettings.Remove(Constants.KMotdSetting);
            if (!String.IsNullOrEmpty(motd))
            {
                appSettings.Add(Constants.KMotdSetting, motd);
            }
            appSettings.Save();
        }

        #endregion MOTD

        #region Languages

        public IObservable<List<LanguageInfo>> RequestLanguageListUpdate()
        {
            return Transport.GetLanguges();
        }

        public IObservable<bool> DownloadLocalization(string remoteFileName)
        {
            return Transport.DownloadLocalization(remoteFileName);
        }

     //   private object updateLanguageSettings(List<LanguageInfoRemote> languages)
      //  {
       //     throw new NotImplementedException();
        //}

     //   private object DisplayError()
       // {
         //   throw new NotImplementedException();
        //}

        //private object ParseLanguages(string xml)
       // {
       //     throw new NotImplementedException();
       // }

        #endregion Languages

        #region Downloads

        private void SubscribeForDownloadManagerEvents()
        {
            DownloadManager.DownloadEnqueuedEvent.Subscribe(download => download.State = QueuedDownload.DownloadState.Queued);
            DownloadManager.DownloadStartedEvent.Subscribe(download => download.State = QueuedDownload.DownloadState.Downloading);
            DownloadManager.DownloadStopPendingEvent.Subscribe(download => download.State = QueuedDownload.DownloadState.Paused);
            DownloadManager.DownloadStoppedEvent.Subscribe(download =>
            {
                if (download.State != QueuedDownload.DownloadState.Stopped)
                    download.State = QueuedDownload.DownloadState.Paused;
            });

            DownloadManager.DownloadCompletedEvent
                           .Merge(DownloadManager.DownloadErrorEvent)
                           .Subscribe(download => LoggedUser.Downloads.Remove(download));
        }

        private Subject<QueuedDownload> _downloadEnqueuedEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadEnqueuedEvent { get { return _downloadEnqueuedEvent; } }

        public enum AddingToQueueResult
        {
            ItemAddedToQueue,
            ItemAlreadyDownloaded,
            DownloadItemStarted
        }

        public AddingToQueueResult EnqueueMediaItem(MediaItemsListModelItem mediaItem, bool immediate)
        {
            if (LoggedUser.Downloads.Count(queuedDownload => queuedDownload.Id == mediaItem.Id) == 0)
            {
                QueuedDownload.DownloadState initialState =
                    App.Engine.LoggedUser.Settings.AutomaticDownloads ?
                    QueuedDownload.DownloadState.Queued :
                    (immediate ? QueuedDownload.DownloadState.Queued : QueuedDownload.DownloadState.Paused);

                QueuedDownload queuedDownload = new QueuedDownload(mediaItem) { State = initialState };

                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isf.FileExists(Utils.MediaFilePath(LoggedUser, queuedDownload)))
                    {
                        return AddingToQueueResult.ItemAlreadyDownloaded;
                    }
                }

                StatisticsManager.LogDownloadAdd(queuedDownload);
                LoggedUser.Downloads.Add(queuedDownload);
                _downloadEnqueuedEvent.OnNext(queuedDownload);

                if (initialState != QueuedDownload.DownloadState.Stopped)
                {
                    DownloadManager.StartDownload(queuedDownload);
                }
                return AddingToQueueResult.ItemAddedToQueue;
            }
            else if (immediate)
            {
                QueuedDownload queuedDownload = (from download in LoggedUser.Downloads where download.Id == mediaItem.Id select download).First();
                queuedDownload.State = QueuedDownload.DownloadState.Queued;
                DownloadManager.StartDownload(queuedDownload);
                return AddingToQueueResult.DownloadItemStarted;
            }
            else
            {
                return AddingToQueueResult.ItemAlreadyDownloaded;
            }
        }

        public void StopDownload(QueuedDownload download)
        {
            DownloadManager.StopDownload(download);
        }

        public void StartDownload(QueuedDownload download)
        {
            DownloadManager.StartDownload(download);
        }

        public void CancelDownload(QueuedDownload download)
        {
            DownloadManager.StopDownload(download).Subscribe(
                dl =>
                {
                    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string path = Utils.MediaFilePath(LoggedUser, dl);
                        if (isf.FileExists(path))
                        {
                            isf.DeleteFile(path);
                        }
                    }
                });
            LoggedUser.Downloads.Remove(download);
            App.Engine.StatisticsManager.LogDownloadRemove(download); 
        }

        #endregion Downloads

        #region Tombstoning and Exiting

        public bool IsLoaded { get; private set; }

        private const string LoggedUserTag = "LoggedUser";

        public void LoadSessionData()
        {
            if (IsLoaded)
            {
                processUserLogged();
                return;
            }

            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            if (state.ContainsKey(LoggedUserTag))
            {
                LoggedUser = UserDatabase.GetUser((string)state[LoggedUserTag]);
            }
            IsLoaded = true;
        }

        public void SaveSessionData()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            if (LoggedUser != null)
                state[LoggedUserTag] = LoggedUser.Username;
            else
                state.Remove(LoggedUserTag);
        }

        public void SavePersistantData()
        {
            StatisticsManager.Save(true);
        }

        #endregion Tombstoning and Exiting

    }

}
