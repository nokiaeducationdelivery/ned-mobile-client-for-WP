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
using System.Linq;
using System.Windows;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Reactive;
using NedEngine;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;
using NedWp.Resources.Languages;
using System.Threading;

namespace NedWp
{
    public partial class StatisticsPage : PhoneApplicationPage
    {
        public class MediaItemStatistic
        {
            public MediaItemStatistic(int viewCount, string mediaName, string mediaType)
            {
                ViewCount = viewCount;
                MediaName = mediaName;
                MediaIcon = NedEngine.Utils.GetMediaIcon((MediaItemType)Enum.Parse(typeof(MediaItemType), mediaType, true));
            }

            public int ViewCount { get; private set; }
            public string MediaName { get; private set; }
            public string MediaIcon { get; private set; }
        }

        public ObservableCollection<MediaItemStatistic> MediaStatistics = new ObservableCollection<MediaItemStatistic>();

        public StatisticsPage()
        {
            InitializeComponent();
            StatisticsList.DataContext = MediaStatistics;
            EmptyLibraryListInstruction.DataContext = MediaStatistics;

            PrepareApplicationBar();
        }

        private void PrepareApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton helpButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative));
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = AppResources.App_HelpButtonContent;

            ApplicationBarIconButton uploadButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.upload.rest.png", UriKind.Relative));
            uploadButton.Click += UploadStatistics;
            uploadButton.Text = AppResources.StatisticPage_UploadButton;


            ApplicationBar.PopulateWithButtons(new ApplicationBarIconButton[] {
                uploadButton,
                helpButton
            });
        }
        private IDisposable _disposeUpdateStatisticEvent;
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs navEvent)
        {
            _disposeUpdateStatisticEvent = App.Engine.StatisticsManager.UpdateStatisticEvent.Subscribe<StatisticsManager.StatisticsUploadStatus>
            (
                result => { UploadStatusUpdated(result); },
                error => { UploadStatusUpdated(StatisticsManager.StatisticsUploadStatus.Error); }
            );
            RefreshData();
            base.OnNavigatedTo(navEvent);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_disposeUpdateStatisticEvent != null)
                _disposeUpdateStatisticEvent.Dispose();

            base.OnNavigatedFrom(e);
        }

        private void RefreshData()
        {
            MediaStatistics.Clear();
            var events =
                from e in App.Engine.StatisticsManager.Statistics
                where e.Type == StatisticType.PLAY_ITEM_START && e.Username == App.Engine.LoggedUser.Username
                group e by new { MediaName = e.Details[MediaItemStatisticItem.DetailKeys.MediaTitle], MediaType = e.Details[MediaItemStatisticItem.DetailKeys.MediaType] } into grp
                select new MediaItemStatistic(grp.Count(), grp.Key.MediaName, grp.Key.MediaType);
            foreach (MediaItemStatistic item in events)
            {
                MediaStatistics.Add(item);
            }
        }


        private void UploadStatistics(object sender, EventArgs e)
        {
            App.Engine.StatisticsManager.UploadStatistics();
        }

        private void UploadStatusUpdated(StatisticsManager.StatisticsUploadStatus result)
        {
            switch (result)
            {
                case StatisticsManager.StatisticsUploadStatus.Success:
                case StatisticsManager.StatisticsUploadStatus.Error:
                    RefreshData();
                    break;
                case StatisticsManager.StatisticsUploadStatus.NothingToUpload:
                case StatisticsManager.StatisticsUploadStatus.UploadStarted:
                default:
                    break;
            }
        }

        public void NavigateToHelpView(object sender, EventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/HelpPage.xaml?type=" + HelpPages.EStatisticsPage.ToString(), UriKind.Relative));
        }
    }
}
