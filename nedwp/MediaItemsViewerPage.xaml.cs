/*******************************************************************************
* Copyright (c) 2011-2012 Nokia Corporation
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
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using NedEngine;
using System.Windows.Media;
using Microsoft.Phone.Shell;
using NedWp.Resources.Languages;

namespace NedWp
{
    public partial class MediaItemsViewerPage : PhoneApplicationPage
    {
        private MediaItemsListModelItem mMediaItemModel = null;
        private MediaItemsListModelItem MediaItemModel
        {
            get { return mMediaItemModel; }
            set
            {
                mMediaItemModel = value;
                MediaSelector.UpdatedView( mMediaItemModel );
            }
        }

        public MediaItemsViewerPage()
        {
            InitializeComponent();
            MediaItemModel = new MediaItemsListModelItem();

            PrepareApplicationBar();
        }

        private void PrepareApplicationBar()
        {
            ApplicationBarIconButton descriptionButton = new ApplicationBarIconButton( new Uri( "/Resources/Icons/info.png", UriKind.Relative ) );
            descriptionButton.Click += OnDescriptionButtonClicked;
            descriptionButton.Text = FileLanguage.SHOW_DETAILS;

            ApplicationBarIconButton linksButton = new ApplicationBarIconButton( new Uri( "/Resources/Icons/link.png", UriKind.Relative ) );
            linksButton.Click += OnLinksButtonClicked;
            linksButton.Text = FileLanguage.SHOW_LINKS;

            ApplicationBarIconButton helpButton = new ApplicationBarIconButton( new Uri( "/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative ) );
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = FileLanguage.HELP;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.PopulateWithButtons( new ApplicationBarIconButton[] {
                descriptionButton,
                linksButton,
                helpButton
            } );
        }

        protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs args )
        {
            IDictionary<string, string> parameters = NavigationContext.QueryString;
            string id = parameters["id"];

            App.Engine.LibraryModel.LoadLibraryStateIfNotLoaded();
            MediaItemModel = App.Engine.LibraryModel.GetMediaItemForId( id );

            if( id == null || MediaItemModel == null )
            {
                MessageBox.Show( FileLanguage.MediaItemViewerPage_CanNotOpenItem );
                System.Diagnostics.Debug.Assert( false, "Navigated to MediaItemsViewerPage without or with non-existant item ID" );
            }
            else
            {
                TitlePanel.TitleText = MediaItemModel.Title;
            }
            base.OnNavigatedTo( args );
        }

        protected override void OnNavigatedFrom( System.Windows.Navigation.NavigationEventArgs args )
        {
            App.Engine.LibraryModel.SaveLibraryState();
            base.OnNavigatedFrom( args );
        }

        private void OnWebBrowserLoaded( object sender, RoutedEventArgs e )
        {
            WebBrowser webBrowser = ( sender as WebBrowser );
            if( MediaItemModel.GetMediaFileIsolatedStoragePath().ToLower().EndsWith( Constants.KPdfExt ) )
            {
                webBrowser.Visibility = Visibility.Collapsed;
                MessageBox.Show( FileLanguage.MediaItemViewerPage_UnableToOpenDocument );
            }
            else
            {
                webBrowser.Navigate( new Uri( MediaItemModel.GetMediaFileIsolatedStoragePath(), UriKind.Relative ) );
            }
        }

        private void OnLinksButtonClicked( object sender, EventArgs args )
        {
            ShowLinksCommand.GetCommand().Execute( MediaItemModel );
        }

        private void OnDescriptionButtonClicked( object sender, EventArgs args )
        {
            ShowDescriptionCommand.GetCommand().Execute( MediaItemModel );
        }

        private void OnDesiredVideoOrientationChanged( object sender, DesiredOrientationEventArgs args )
        {
            if( args.DesiredOrientation == PageOrientation.Landscape ||
                args.DesiredOrientation == PageOrientation.LandscapeLeft ||
                args.DesiredOrientation == PageOrientation.LandscapeRight )
            {
                SystemTray.IsVisible = false;
                ApplicationBar.IsVisible = false;
                TitlePanel.Visibility = Visibility.Collapsed;
                SupportedOrientations = SupportedPageOrientation.Landscape; // WARN: It should reset to original page setting (possinly PortraitOrLandscape)
                Orientation = PageOrientation.Landscape;
            }
            else
            {
                SystemTray.IsVisible = true;
                ApplicationBar.IsVisible = true;
                TitlePanel.Visibility = Visibility.Visible;
                SupportedOrientations = SupportedPageOrientation.Portrait;
                Orientation = PageOrientation.Portrait;
            }
        }

        private void OnBackKeyPressed( object sender, System.ComponentModel.CancelEventArgs e )
        {
            App.Engine.StatisticsManager.LogMediaStop( mMediaItemModel );
            App.Engine.StatisticsManager.LogMediaItemBack( mMediaItemModel.Id );
        }

        private void NavigateToHelpView( object sender, EventArgs e )
        {
            ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/HelpPage.xaml?type=" + HelpPages.EMediaItemPage.ToString(), UriKind.Relative ) );
        }
    }

    public class MediaItemViewerTemplateSelector : ContentControl
    {
        public DataTemplate PictureTemplate { get; set; }
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate UnknownTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }

        public void UpdatedView( MediaItemsListModelItem mediaItem )
        {
            switch( mediaItem.ItemType )
            {
                case MediaItemType.Picture:
                    PictureItemModel pictureModel = new PictureItemModel();
                    pictureModel.LoadImage( mediaItem.GetMediaFileIsolatedStoragePath() );
                    Content = pictureModel;
                    ContentTemplate = PictureTemplate;
                    break;
                case MediaItemType.Text:
                    ContentTemplate = TextTemplate;
                    break;
                case MediaItemType.Audio:
                    Content = new AudioVideoItemModel() { MediaSource = mediaItem.GetMediaFileIsolatedStoragePath() };
                    ContentTemplate = AudioTemplate;
                    break;
                case MediaItemType.Video:
                    Content = new AudioVideoItemModel() { MediaSource = mediaItem.GetMediaFileIsolatedStoragePath() };
                    ContentTemplate = VideoTemplate;
                    break;
                case MediaItemType.Undefined:
                    ContentTemplate = UnknownTemplate;
                    break;
                default:
                    break;
            }
        }
    }

    #region PictureItemModel
    public class PictureItemModel
    {
        public BitmapSource PictureSource { get; set; }

        public void LoadImage( string path )
        {
            byte[] data;
            using( IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication() )
            {
                using( IsolatedStorageFileStream isfs = isf.OpenFile( path, FileMode.Open, FileAccess.Read ) )
                {
                    data = new byte[isfs.Length];
                    isfs.Read( data, 0, data.Length );
                    isfs.Close();
                }
            }
            MemoryStream ms = new MemoryStream( data );
            BitmapImage bi = new BitmapImage();
            bi.SetSource( ms );
            PictureSource = bi;
        }

        public void OnFailedToLoadImage( object sender, EventArgs args )
        {
            FailedToLoadImage();
        }

        private void FailedToLoadImage()
        {
            PictureSource = new BitmapImage( new Uri( "Resources/Icons/failed_to_load_picture.png", UriKind.Relative ) );
        }
    }
    #endregion PictureItemModel

    #region AudioVideoItemModel
    public class AudioVideoItemModel
    {
        public string MediaSource { get; set; }
    }
    #endregion AudioVideoItemModel

}