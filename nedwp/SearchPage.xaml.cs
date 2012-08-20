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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NedEngine;
using NedWp.Resources.Languages;

namespace NedWp
{
    public partial class SearchPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        bool IsDataLoaded = false; // Avoid reloding data when app is not Tombstoned
        bool IsBackNavigating = false; // To remove state data when leaving page with Back key

        private bool IsSearchListLoaded = false; // Dirty workaround for CollectionViewSource issue - it selects first item in collection by default and in WP7.0 we navigate basing on selection

        private CollectionViewSource CollectionFilter;

        private string mCurrentKeyword = String.Empty;
        public string CurrentKeyword
        {
            get { return mCurrentKeyword; }
            set
            {
                if( mCurrentKeyword != value )
                {
                    mCurrentKeyword = value;
                    NotifyPropertyChanged( "CurrentKeyword" );
                }
            }
        }
        public string RootId { get; set; }

        public SearchPage()
        {
            InitializeComponent();

            Loaded += OnPageLoaded;

            PrepareApplicationBar();
        }

        private void PrepareApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton helpButton = new ApplicationBarIconButton( new Uri( "/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative ) );
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = FileLanguage.HELP;

            ApplicationBar.PopulateWithButtons( new ApplicationBarIconButton[] {
                helpButton
            } );
        }

        private void OnPageLoaded( object sender, RoutedEventArgs args )
        {
            LoadMediaItemsUnderRootId();
            PropertyChanged += OnCurrentKeywordChanged;
            App.Engine.LibraryModel.LibraryItemRemoved += ReloadMediaItemsUnderRootId;

            SearchList.DataContext = CollectionFilter;
            SearchTextBox.Text = CurrentKeyword;
            SearchTextBox.KeyUp += OnTextBoxKeyReleased;

            IsDataLoaded = true;
        }

        private void ReloadMediaItemsUnderRootId( object sender, EventArgs args )
        {
            LoadMediaItemsUnderRootId();
        }

        private void LoadMediaItemsUnderRootId()
        {
            IsSearchListLoaded = false;
            CollectionFilter = Resources["CollectionFilter"] as CollectionViewSource;
            CollectionFilter.Source = App.Engine.LibraryModel.GetAllMediaItemsUnderId( RootId );
            CollectionFilter.View.Filter = new Predicate<Object>( FilterItemsByKeyword );
            CollectionFilter.View.MoveCurrentToPosition( -1 );
            IsSearchListLoaded = true;
        }

        public bool FilterItemsByKeyword( object item )
        {
            if( item == null )
                return false;
            MediaItemsListModelItem mediaItem = item as MediaItemsListModelItem;
            return ( mediaItem.Keywords.Contains( CurrentKeyword ) );
        }

        private void Search()
        {
            CurrentKeyword = SearchTextBox.Text.Trim();
            App.Engine.StatisticsManager.LogSearching( CurrentKeyword );
        }

        private void OnSearchButtonClicked( object sender, ManipulationCompletedEventArgs args )
        {
            Search();
        }

        private void OnTextBoxKeyReleased( object sender, KeyEventArgs args )
        {
            if( args.Key == Key.Enter )
            {
                if( sender == SearchTextBox )
                {
                    Search();
                    SearchList.Focus();
                }
            }
        }

        private void OnSelectionChanged( object sender, SelectionChangedEventArgs args )
        {
            if( SearchList.SelectedIndex < 0 || !IsSearchListLoaded )
                return;

            MediaItemRequestedCommand.GetCommand().Execute( SearchList.SelectedItem as MediaItemsListModelItem );
            SearchList.SelectedIndex = -1; // Reset selection
        }

        private void OnCurrentKeywordChanged( object sender, PropertyChangedEventArgs args )
        {
            if( args.PropertyName == "CurrentKeyword" )
            {
                CollectionFilter.View.Refresh();
            }
        }

        protected override void OnNavigatingFrom( NavigatingCancelEventArgs args )
        {
            if( args.NavigationMode == NavigationMode.Back )
                IsBackNavigating = true;
            base.OnNavigatingFrom( args );
        }

        protected override void OnNavigatedFrom( System.Windows.Navigation.NavigationEventArgs args )
        {
            if( IsBackNavigating ) // search screen clearing when exited 
            {
                ClearSearchState();
            }
            else
            {
                SaveSearchState();
            }
            IsBackNavigating = false;
            base.OnNavigatedFrom( args );
        }

        protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs args )
        {
            IDictionary<string, string> parameters = NavigationContext.QueryString;
            RootId = parameters["rootId"];

            if( !IsDataLoaded )
            {
                LoadSearchState();
            }
            base.OnNavigatedTo( args );
        }

        #region Tombstoning

        private const string CurrentSearchKeywordKey = "CurrentSearchKeyword";
        private void SaveSearchState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            state[CurrentSearchKeywordKey] = CurrentKeyword;
            App.Engine.LibraryModel.SaveLibraryState();
        }

        private void ClearSearchState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            state.Remove( CurrentSearchKeywordKey );
        }

        private void LoadSearchState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            App.Engine.LibraryModel.LoadLibraryStateIfNotLoaded();
            if( state.ContainsKey( CurrentSearchKeywordKey ) )
            {
                CurrentKeyword = (string)state[CurrentSearchKeywordKey];
            }
        }

        #endregion Tombstoning

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged( string propertyName )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        private void NavigateToHelpView( object sender, EventArgs e )
        {
            ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/HelpPage.xaml?type=" + HelpPages.ESearchPage.ToString(), UriKind.Relative ) );
        }
    }
}