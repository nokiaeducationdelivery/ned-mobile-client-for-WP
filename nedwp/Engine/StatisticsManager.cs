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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using Microsoft.Phone.Reactive;
using NedWp;
using Coding4Fun.Phone.Controls;
using NedWp.Resources.Languages;

namespace NedEngine
{
    public class StatisticsManager : LinqToXmlDatabase
    {
        protected override XElement Data
        {
            get { return new XElement(Tags.Statistics, from u in Statistics select u.GetData()); }
        }

        protected override string PathToFile { get { return "stats.xml"; } }

        private int KStatChangeBufferLimit = 5;
        private int StatChangeBuffer { get; set; }
        public ObservableCollection<StatisticItem> Statistics { get; private set; }

        private readonly ApplicationSettings ApplicationSettings;
        private readonly ILoggedUser LoggedUser;
        private readonly Transport Transport;

        public StatisticsManager(ILoggedUser LoggedUser, ApplicationSettings ApplicationSettings, Transport transport)
        {
            this.ApplicationSettings = ApplicationSettings;
            this.LoggedUser = LoggedUser;
            this.Transport = transport;

            XDocument doc = Open();

            Statistics = new ObservableCollection<StatisticItem>();
            foreach (XElement usageEvent in doc.Descendants(Tags.Statistic))
            {
                Statistics.Add(CreateItemBasedOnStatisticType(usageEvent));
            }
            Statistics.CollectionChanged += (sender, args) => { if (args.NewItems != null) StatisticAdded(); };
        }

        public StatisticItem CreateItemBasedOnStatisticType(XElement xElement)
        {
            try
            {
                StatisticType type = (StatisticType)Enum.Parse(typeof(StatisticType), xElement.Attribute(Tags.StatisticType).Value, true);
                switch (type)
                {
                    case StatisticType.BROWSE_CATALOG_BACK:
                    case StatisticType.BROWSE_CATALOG_OPEN:
                    case StatisticType.BROWSE_LIBRARY_OPEN:
                    case StatisticType.BROWSE_LIBRARY_BACK:
                    case StatisticType.BROWSE_CATEGORY_BACK:
                    case StatisticType.BROWSE_CATEGORY_OPEN:
                    case StatisticType.BROWSE_MEDIAITEM_BACK:
                    case StatisticType.DELETE_ITEM:
                        return new NavigationStatisticItem(xElement);
                    case StatisticType.DOWNLOAD_ADD:
                    case StatisticType.DOWNLOAD_COMPLETED:
                    case StatisticType.DOWNLOAD_END:
                    case StatisticType.DOWNLOAD_REMOVE:
                    case StatisticType.DOWNLOAD_START:
                        return new DownloadStatisticItem(xElement);
                    case StatisticType.USER_LOGGED:
                    case StatisticType.APP_EXIT:
                    case StatisticType.USER_DELETE:
                        return new AuthenticationStatisticItem(xElement);
                    case StatisticType.PLAY_ITEM_END:
                    case StatisticType.PLAY_ITEM_START:
                    case StatisticType.DETAILS_SHOW:
                    case StatisticType.SHOW_LINKS:
                        return new MediaItemStatisticItem(xElement);
                    case StatisticType.LIBRARY_ADD:
                    case StatisticType.LIBRARY_REMOVED:
                        return new LibraryStatisticItem(xElement);
                    case StatisticType.SEARCH_ITEM:
                    case StatisticType.LINK_OPEN:
                        return new VariousDetailsStatisticItem(xElement);
                    case StatisticType.UNKNOWN:
                    default:
                        return null;
                }
            }
            catch (ArgumentException)
            {
                Debug.Assert(false, "Can't read StatisticType.");
                return null;
            }
        }

        public bool AreThereAnyStatisticsToUpload()
        {
            return Statistics.Count() > 0;
        }

        private void StatisticAdded()
        {
            StatChangeBuffer++;
            if (StatChangeBuffer > KStatChangeBufferLimit)
            {
                StatChangeBuffer = 0;
                SaveStatistics();
            }
        }

        private void SaveStatistics()
        {
            Save(false);
        }

        public void LogMediaPlayback(MediaItemsListModelItem mediaItem)
        {
            Statistics.Add(MediaItemStatisticItem.CreateMediaPlayItemStartEvent(LoggedUser.LoggedUser, mediaItem));
        }

        public void LogMediaStop(MediaItemsListModelItem mediaItem)
        {
            Statistics.Add(MediaItemStatisticItem.CreateMediaStopItemStartEvent(LoggedUser.LoggedUser, mediaItem));
        }

        public void LogMediaItemBack(string mediaId)
        {
            Statistics.Add(NavigationStatisticItem.CreateMediaItemBackEvent(LoggedUser.LoggedUser, mediaId));
        }

        public void LogShowLinks(MediaItemsListModelItem mediaItem)
        {
            Statistics.Add(MediaItemStatisticItem.CreateMediaShowLinksEvent(LoggedUser.LoggedUser, mediaItem));
        }

        public void LogShowMediaDetails(MediaItemsListModelItem mediaItem)
        {
            Statistics.Add(MediaItemStatisticItem.CreateMediaShowDetailsEvent(LoggedUser.LoggedUser, mediaItem));
        }

        public void LogItemDeleted(String id)
        {
            Statistics.Add(NavigationStatisticItem.CreateMediaDeleteItemEvent(LoggedUser.LoggedUser, id));
        }

        public void LogLibraryOpen(string libraryId)
        {
            Statistics.Add(NavigationStatisticItem.CreateLibraryOpenEvent(LoggedUser.LoggedUser, libraryId));
        }

        public void LogLibraryBack(string libraryId)
        {
            Statistics.Add(NavigationStatisticItem.CreateLibraryBackEvent(LoggedUser.LoggedUser, libraryId));
        }

        public void LogCatalogueOpen(string catalogueId)
        {
            Statistics.Add(NavigationStatisticItem.CreateCatalogueOpenEvent(LoggedUser.LoggedUser, catalogueId));
        }

        public void LogCatalogueBack(string catalogueId)
        {
            Statistics.Add(NavigationStatisticItem.CreateCatalogueBackEvent(LoggedUser.LoggedUser, catalogueId));
        }

        public void LogCategoryOpen(string categoryId)
        {
            Statistics.Add(NavigationStatisticItem.CreateCategoryOpenEvent(LoggedUser.LoggedUser, categoryId));
        }

        public void LogCategoryBack(string categoryId)
        {
            Statistics.Add(NavigationStatisticItem.CreateCategoryBackEvent(LoggedUser.LoggedUser, categoryId));
        }

        public void LogUserLogin(User user)
        {
            Statistics.Add(AuthenticationStatisticItem.CreateUserLoggedEvent(user));
        }

        public void LogUserDelete(User user)
        {
            Statistics.Add(AuthenticationStatisticItem.CreateUserDeleteEvent(user));
        }

        public void LogAppExit(User user)
        {
            Statistics.Add(AuthenticationStatisticItem.CreateAppExitEvent(user));
        }

        public void LogDownloadAdd(QueuedDownload download)
        {
            Statistics.Add(DownloadStatisticItem.CreateDownloadAddEven(LoggedUser.LoggedUser, ApplicationSettings.ServerUrl.DownloadPath(download), download));
        }

        public void LogDownloadRemove(QueuedDownload download)
        {
            Statistics.Add(DownloadStatisticItem.CreateDownloadRemoveEven(LoggedUser.LoggedUser, ApplicationSettings.ServerUrl.DownloadPath(download), download));
        }

        public void LogDownloadStart(QueuedDownload download)
        {
            Statistics.Add(DownloadStatisticItem.CreateDownloadStartEven(LoggedUser.LoggedUser, ApplicationSettings.ServerUrl.DownloadPath(download), download));
        }

        public void LogDownloadEnd(QueuedDownload download)
        {
            Statistics.Add(DownloadStatisticItem.CreateDownloadEndEven(LoggedUser.LoggedUser, ApplicationSettings.ServerUrl.DownloadPath(download), download));
        }

        public void LogDownloadCompleted(QueuedDownload download, string status)
        {
            Statistics.Add(DownloadStatisticItem.CreateDownloadCompletedEven(LoggedUser.LoggedUser, ApplicationSettings.ServerUrl.DownloadPath(download), download, status));
        }

        public void LogAddLibrary(Library library)
        {
            Statistics.Add(LibraryStatisticItem.CreateAddLibraryEven(LoggedUser.LoggedUser, library));
        }

        public void LogRemoveLibrary(Library library)
        {
            Statistics.Add(LibraryStatisticItem.CreateRemoveLibraryEven(LoggedUser.LoggedUser, library));
        }

        public void LogOpenLink(Uri url)
        {
            Statistics.Add(VariousDetailsStatisticItem.CreateOpenLinkEvent(LoggedUser.LoggedUser, url));
        }

        public void LogSearching(string searchFor)
        {
            Statistics.Add(VariousDetailsStatisticItem.CreateSearchEvent(LoggedUser.LoggedUser, searchFor));
        }

        public IEnumerable<StatisticItem> GetStatisticsForSending(User user, Guid updateId)
        {
            List<StatisticItem> statsToSend = new List<StatisticItem>();
            if (user == null)
                return statsToSend;

            foreach (StatisticItem stat in Statistics.Where(stat => stat.Username == user.Username).Where(stat => stat.UpdateId == Guid.Empty))
            {
                stat.UpdateId = updateId;
                statsToSend.Add(stat);

            }
            return statsToSend;
        }

        public void RemoveQueuedStatistics(bool successful, Guid updateId)
        {
            foreach (StatisticItem stat in Statistics.Where(stat => stat.UpdateId == updateId).ToList())
            {
                if (successful)
                {
                    Statistics.Remove(stat);
                }
                else
                {
                    stat.UpdateId = Guid.Empty;
                }
            }
            SaveStatistics();
        }

        #region UpdatingStatistic 

        public enum StatisticsUploadStatus
        {
            Success,
            Error,
            UploadStarted,
            NothingToUpload
        }

        // From JavaME client
        public enum NedStatUploadConsts
        {
            STATSUPDATED = 0,
            NEWSTATS = 1,
            MISSINGIMEI = -1,
            SERVERINTERNALERROR = -2,
            UNKNOWN = -9999
        }

        private Subject<StatisticsUploadStatus> _updateStatisticEvent = new Subject<StatisticsUploadStatus>();
        public IObservable<StatisticsUploadStatus> UpdateStatisticEvent { get { return _updateStatisticEvent; } }

        private bool _isAutomaticStatUpdateInProgress;
        private bool _waitingForUpdateCompleted;

        private ToastPrompt mToast = new ToastPrompt();

        public void UploadStatistics()
        {
            uploadStatistics().Subscribe<StatisticsUploadStatus>
                    (
                        result => { UploadStatusUpdated(result); },
                        error => { UploadStatusUpdated(StatisticsUploadStatus.Error); }
                    );
        }
        public void AutomaticallyUpdateStatistic()
        {
            uploadStatistics().Subscribe(
                result => { AutomaticallyUploadStatusUpdated(result); },
                error => { AutomaticallyUploadStatusUpdated(StatisticsUploadStatus.Error); }
            );
        }

        public IObservable<StatisticsUploadStatus> uploadStatistics()
        {
            return Observable.Create<StatisticsUploadStatus>(o =>
            {
                Guid updateId = Guid.NewGuid();
                IEnumerable<StatisticItem> stats = GetStatisticsForSending(LoggedUser.LoggedUser, updateId);
                if (stats.Count() == 0)
                {
                    o.OnNext(StatisticsUploadStatus.NothingToUpload);
                    o.OnCompleted();
                }
                else
                {
                    o.OnNext(StatisticsUploadStatus.UploadStarted);
                    Transport.UploadStaticstics(stats)
                         .ObserveOnDispatcher()
                         .Subscribe<int>(
                         responseContent =>
                         {
                             RemoveQueuedStatistics(true, updateId);
                             NedStatUploadConsts responseCode = (NedStatUploadConsts)responseContent;
                             if (responseCode == NedStatUploadConsts.NEWSTATS || responseCode == NedStatUploadConsts.STATSUPDATED)
                                 o.OnNext(StatisticsUploadStatus.Success);
                             else
                                 o.OnNext(StatisticsUploadStatus.Error);
                         },
                         error =>
                         {
                             RemoveQueuedStatistics(false, updateId);
                             o.OnError(new Exception());
                         },
                         () => { o.OnCompleted(); }
                         );
                }
                return () => { };
            }
            );
        }
        

        private void UploadStatusUpdated(StatisticsUploadStatus result)
        {
            switch (result)
            {
                case StatisticsUploadStatus.NothingToUpload:
                    {
                        if (_isAutomaticStatUpdateInProgress && !_waitingForUpdateCompleted)
                        {
                            _waitingForUpdateCompleted = true;
                            UploadStatusUpdated(StatisticsUploadStatus.UploadStarted);
                        }
                        else
                        {
                            ShowToastMessage(AppResources.StatisticPage_NoStatisticToUpload);
                        }
                    }
                    break;
                case StatisticsUploadStatus.UploadStarted:
                    ShowToastMessage(AppResources.StatisticPage_StartedUploading);
                    break;
                case StatisticsUploadStatus.Success:
                    ShowToastMessage(AppResources.StatisticPage_UploadingSucces);
                    break;
                case StatisticsUploadStatus.Error:
                    ShowToastMessage(AppResources.StatisticPage_UploadFiles);
                    break;
                default:
                    ShowToastMessage(AppResources.StatisticPage_UploadFiles);
                    System.Diagnostics.Debug.Assert(false, "Unknown statistics upload result");
                    break;
            }
            _updateStatisticEvent.OnNext(result);
        }
        private void AutomaticallyUploadStatusUpdated(StatisticsUploadStatus result)
        {
            switch (result)
            {
                case StatisticsUploadStatus.NothingToUpload:
                case StatisticsUploadStatus.Success:
                case StatisticsUploadStatus.Error:
                    _isAutomaticStatUpdateInProgress = false;
                    if (_waitingForUpdateCompleted)
                    {
                        UploadStatusUpdated(result);
                        _waitingForUpdateCompleted = false;
                    }
                    else
                    {
                        _updateStatisticEvent.OnNext(result);
                    }
                    break;
                case StatisticsUploadStatus.UploadStarted:
                    _isAutomaticStatUpdateInProgress = true;
                    break;
                default:
                    break;
            }
        }

        private void ShowToastMessage(string message)
        {
            mToast.Hide();
            mToast = new ToastPrompt();
            mToast.Message = message;
            mToast.Show();
        }

        #endregion UpdatingStatistic
    }
}
