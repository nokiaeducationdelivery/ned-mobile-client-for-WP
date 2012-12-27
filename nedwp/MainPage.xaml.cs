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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using NedEngine;
using Microsoft.Phone.Reactive;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NedWp.Resources.Languages;
using Coding4Fun.Phone.Controls.Data;

namespace NedWp
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        private Dictionary<String, IApplicationBarMenuItem> _applicationBarButtons = new Dictionary<string, IApplicationBarMenuItem>();

        private IDisposable longRunningOperation = null;

        public enum Screen
        {
            None = -1,
            SelectServer = 1,
            Login,
            MainMenu,
            TipsAndTricks
        }

        public string DemoUrl
        {
            get
            {
                return NedEngine.Engine.KDemoURL;
            }
        }

        public bool IsDemoServerSelected
        {
            get
            {
                return NedEngine.Engine.IsDemoServerSelected();
            }
        }

        // mNextTranistionIsForward and TemporaryVisibleScreen are used to aid a workaround for page transition issue.
        // We can't apply normal page transitions as page object is not changed (only its contents are changed) therefore we have to detect when contents change and run custom transitions.
        private bool mNextTranistionIsForward = false; // Indicates wheteher next tranistion will be 'forward like' or 'backward like'
        private Screen _tempVisibleScreen = Screen.SelectServer; // Holds a target screen for a transition, it is necessary as we have to wait for first part of traisition to finish before changing the screen
        private Screen TemporaryVisibleScreen
        {
            get
            {
                return _tempVisibleScreen;
            }
            set
            {
                _tempVisibleScreen = value;
                PageTransition( Application.Current.Resources["NavigationOutTransition"] as NavigationOutTransition );
            }
        }

        public event EventHandler VisibleScreenChanged;
        public void OnVisibleScreenChanged()
        {
            if( VisibleScreenChanged != null )
                VisibleScreenChanged( this, new EventArgs() );
        }
        private Screen _visibleScreen = Screen.SelectServer;
        public Screen VisibleScreen
        {
            get
            {
                return _visibleScreen;
            }

            set
            {
                if( value != _visibleScreen )
                {
                    if( _visibleScreen == Screen.MainMenu )
                    {
                        ClearMainPageState();
                    }
                    _visibleScreen = value;

                    NotifyPropertyChanged( "VisibleScreen" );
                    OnVisibleScreenChanged();
                }
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            InitializePasswordAndUsernameBoxes();
            ApplicationBar = new ApplicationBar();
            CreateApplicationBarButtons();
            ApplicationBar.IsMenuEnabled = true;

            ServerAddress.DataContext = App.Engine.ApplicationSettings;
            RememberMe.DataContext = App.Engine.ApplicationSettings;
            LibraryManager.DataContext = App.Engine;
            MottoOfTheDay.DataContext = App.Engine;
            AllLibrariesList.DataContext = VisibleLibraries;
            EmptyLibraryListInstruction.DataContext = VisibleLibraries;
            Downloads.DataContext = App.Engine;
            LayoutRoot.DataContext = this;
            Password.DataContext = this;
            Username.DataContext = this;
            CurrentTips.DataContext = App.Engine.TipsAndTricks;
            ShowTipsStartup.DataContext = App.Engine.ApplicationSettings;
            App.Engine.OnLogoutCompleted += OnLogoutCompleted;
            App.Engine.OnFactoryResetCompleted += OnFactoryResetCompleted;

            ServerUrl.KeyUp += OnTextBoxKeyReleased;
            Username.KeyUp += OnTextBoxKeyReleased;
            Password.KeyUp += OnTextBoxKeyReleased;
            NewLibraryId.KeyUp += OnTextBoxKeyReleased;

            MainMenuScreen.Loaded += OnMainMenuScreenLoaded;
            MainMenuScreen.Unloaded += OnMainMenuScreenUnloaded;
            VisibleScreenChanged += ( sender, args ) => { UpdateApplicationBarButtons(); };
            UpdateApplicationBarButtons();

            App.Engine.PropertyChanged += ( sender, args ) =>
                {
                    if( args.PropertyName == "LoggedUser" )
                    {
                        SubscribeForLibraryChanges();
                    }
                };
            App.Engine.ApplicationSettings.PropertyChanged += ( sender, args ) =>
                {
                    if( args.PropertyName == "ServerUrl" )
                    {
                        DemoLoginDetailsLabel.Visibility = IsDemoServerSelected ? Visibility.Visible : Visibility.Collapsed;
                    }
                };
        }

        private void CreateApplicationBarButtons()
        {
            ApplicationBarIconButton helpButton = new ApplicationBarIconButton( new Uri( "/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative ) );
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = FileLanguage.HELP;
            ApplicationBarIconButton statisticButton = new ApplicationBarIconButton( new Uri( "/Resources/Icons/statistics.png", UriKind.Relative ) );
            statisticButton.Click += NavigateToStatistics;
            statisticButton.Text = FileLanguage.MID_STATISTICS_COMMAND;
            ApplicationBarIconButton settingsButton = new ApplicationBarIconButton( new Uri( "/Resources/OriginalPlatformIcons/appbar.feature.settings.rest.png", UriKind.Relative ) );
            settingsButton.Click += NavigateToSettings;
            settingsButton.Text = FileLanguage.MID_OPTIONS_COMMAND;

            ApplicationBarMenuItem factoryResetButton = new ApplicationBarMenuItem();
            factoryResetButton.Click += OnFactoryResetButtonClicked;
            factoryResetButton.Text = FileLanguage.FACTORY_SETTINGS;

            ApplicationBarMenuItem aboutButton = new ApplicationBarMenuItem();
            aboutButton.Click += OnAboutButtonClicked;
            aboutButton.Text = FileLanguage.ABOUT;

            _applicationBarButtons.Add( "HelpButton", helpButton );
            _applicationBarButtons.Add( "StatisticsButton", statisticButton );
            _applicationBarButtons.Add( "SettingsButton", settingsButton );
            _applicationBarButtons.Add( "AboutButton", aboutButton );
            _applicationBarButtons.Add( "FactoryResetButton", factoryResetButton );

        }

        private void UpdateApplicationBarButtons()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();

            if( VisibleScreen == Screen.MainMenu )
            {

                ApplicationBar.Buttons.Add( _applicationBarButtons["HelpButton"] );
                ApplicationBar.Buttons.Add( _applicationBarButtons["StatisticsButton"] );
                ApplicationBar.Buttons.Add( _applicationBarButtons["SettingsButton"] );
            }
            else
            {
                ApplicationBar.Buttons.Add( _applicationBarButtons["HelpButton"] );
                if( VisibleScreen == Screen.Login )
                {
                    ApplicationBar.MenuItems.Add( _applicationBarButtons["FactoryResetButton"] );
                }
            }
            ApplicationBar.MenuItems.Add( _applicationBarButtons["AboutButton"] );
        }

        protected override void OnNavigatedFrom( System.Windows.Navigation.NavigationEventArgs args )
        {
            // WORKAROUND: This hides a nasty effect during Logout/RemoveUser/FactoryReset 
            // when for a very short moment icons other then expected show up
            if( args.Content is SettingsPage )
                ApplicationBar.IsVisible = false;
            SavePageState();
        }

        protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs e )
        {
            App.RecursiveBack = false;

            if( App.Engine.ApplicationSettings.ServerUrl == null )
            {
                VisibleScreen = Screen.SelectServer;
            }
            else if( App.Engine.LoggedUser == null )
            {
                VisibleScreen = Screen.Login;
            }
            else
            {
                VisibleScreen = Screen.MainMenu;
            }
            if( !ApplicationBar.IsVisible ) // Do not remove unless Settings Page workaround is removed
                ApplicationBar.IsVisible = true;
            ProgressBarOverlay.Close();

            bool pivot = this.NavigationContext.QueryString.ContainsKey( "MainMenuScreen.SelectedIndex" );

            int index = 0;

            if( pivot && int.TryParse( this.NavigationContext.QueryString["MainMenuScreen.SelectedIndex"], out index ) )
            {
                backToContentLists = true;
                MainMenuScreen.SelectedIndex = index;
                if( e.NavigationMode == System.Windows.Navigation.NavigationMode.New )
                {
                    if( PhoneApplicationService.Current.State.ContainsKey( SelectedPivotItemIndexKey ) )
                    {
                        PhoneApplicationService.Current.State[SelectedPivotItemIndexKey] = index;
                    }
                }
                else
                {
                    if( PhoneApplicationService.Current.State.ContainsKey( SelectedPivotItemIndexKey ) )
                    {
                        PhoneApplicationService.Current.State[SelectedPivotItemIndexKey] = 1;
                    }
                }
            }
            base.OnNavigatedTo( e );
        }

        private void OnMainMenuScreenLoaded( object sender, RoutedEventArgs args )
        {
            LoadPageState();
            MainMenuScreen.SelectionChanged += UpdateMOTDAnimation;
            VisibleScreenChanged += UpdateMOTDAnimation;
            UpdateMOTDAnimation( this, new EventArgs() );
        }

        protected void OnMainMenuScreenUnloaded( object sender, EventArgs args )
        {
            SetMOTDActive( false );
        }

        private void InitializePasswordAndUsernameBoxes()
        {
            if( App.Engine.ApplicationSettings.RememberedLogin != null && App.Engine.ApplicationSettings.RememberMe )
                UsernameBoxText = App.Engine.ApplicationSettings.RememberedLogin;
            if( App.Engine.ApplicationSettings.RememberedPassword != null && App.Engine.ApplicationSettings.RememberMe )
                PasswordBoxText = App.Engine.ApplicationSettings.RememberedPassword;
        }

        // from INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged( String propertyName )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if( null != handler )
            {
                handler( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #region ManualPageTranistion

        private void GoForwardToScreen( Screen newScreen )
        {
            mNextTranistionIsForward = true;
            TemporaryVisibleScreen = newScreen;
        }

        private void GoBackwardToScreen( Screen newScreen )
        {
            mNextTranistionIsForward = false;
            TemporaryVisibleScreen = newScreen;
        }

        private void PageTransition( NavigationTransition navigationTransition )
        {
            if( Application.Current.RootVisual == null )
                return;

            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)( ( (PhoneApplicationFrame)Application.Current.RootVisual ) ).Content;
            ITransition transition = null;
            if( mNextTranistionIsForward )
                transition = navigationTransition.Forward.GetTransition( phoneApplicationPage );
            else
                transition = navigationTransition.Backward.GetTransition( phoneApplicationPage );
            transition.Completed += delegate
            {
                transition.Stop();
                if( TemporaryVisibleScreen != Screen.None )
                {
                    VisibleScreen = TemporaryVisibleScreen;
                    TemporaryVisibleScreen = Screen.None;
                    PageTransition( Application.Current.Resources["NavigationInTransition"] as NavigationInTransition );
                }
            };
            transition.Begin();
        }

        #endregion ManualPageTransition

        #region ServerSelection

        private void SaveServer()
        {
            ProgressBarOverlay.Show( FileLanguage.CONNECTING );
            longRunningOperation = App.Engine.SaveServer( ServerUrl.Text )
                                             .Finally( () => ProgressBarOverlay.Close() )
                                             .Subscribe<Unit>(
                                                 result => GoForwardToScreen( Screen.Login ),
                                                 Utils.StandardErrorHandler
                                             );
        }

        private void OnServerAddressSelected( object sender, RoutedEventArgs e )
        {
            SaveServer();
        }

        #endregion ServerSelection

        #region Login

        // Values below should not be saved between application run, they are only backing store for current Username/Password boxes
        private string mUsernameBoxText;
        public string UsernameBoxText
        {
            get { return mUsernameBoxText; }
            set
            {
                mUsernameBoxText = value;
                NotifyPropertyChanged( "UsernameBoxText" );
            }
        }
        private string mPasswordBoxText;
        public string PasswordBoxText
        {
            get { return mPasswordBoxText; }
            set
            {
                mPasswordBoxText = value;
                NotifyPropertyChanged( "PasswordBoxText" );
            }
        }

        private void Login( object sender, RoutedEventArgs e )
        {
            ProgressBarOverlay.Show( FileLanguage.CONNECTING );

            longRunningOperation = App.Engine.Login( UsernameBoxText, PasswordBoxText )
                .Subscribe<Unit>(
                    result =>
                    {
                        Dispatcher.BeginInvoke( () =>
                        {
                            ProgressBarOverlay.Close();
                            App.Engine.RememberUsernameAndPassword( UsernameBoxText, PasswordBoxText );
                            App.Engine.RequestMotdUpdate();
                            MainMenuScreen.SelectedItem = Libraries;
                            if (App.Engine.ApplicationSettings.ShowTipsStartup)
                            {
                                GoForwardToScreen(Screen.TipsAndTricks);
                            }
                            else
                            {
                                GoForwardToScreen(Screen.MainMenu);
                            }
                        } );
                    },
                    error =>
                    {
                        ProgressBarOverlay.Close();
                        Utils.StandardErrorHandler( error );
                    }
                );
        }

        /**
         *  Due to MainPage consists of 3 pages that are unloaded only on application close all input fields etc. should be reset when going back (Logout, Factory Reset etc.)
         */
        private void OnLogoutCompleted( object source, EventArgs args )
        {
            // Clear Login page
            // Username not cleared on purpose
            PasswordBoxText = String.Empty;
            RememberMe.IsChecked = false;
            // Clear Library page
            NewLibraryId.Text = String.Empty;
        }
        private void OnFactoryResetCompleted( object source, EventArgs args )
        {
            // Clear Login page
            UsernameBoxText = String.Empty;
            // Clear ServerSelect page
            ServerUrl.Text = "http://";
        }

        #endregion Login

        #region LibraryManager

        private IDisposable librariesObserver = null;
        public ObservableCollection<Library> VisibleLibraries = new ObservableCollection<Library>();

        private void SubscribeForLibraryChanges()
        {
            if( librariesObserver != null )
            {
                librariesObserver.Dispose();
                librariesObserver = null;
            }

            if( App.Engine.LoggedUser != null )
            {
                librariesObserver = Observable.FromEvent<PropertyChangedEventArgs>( App.Engine.LoggedUser, "PropertyChanged" )
                                              .Where( userEvent =>
                                              {
                                                  string propertyName = userEvent.EventArgs.PropertyName;
                                                  return propertyName == "Libraries" || propertyName == "LibraryElement";
                                              } )
                                              .Subscribe<IEvent<PropertyChangedEventArgs>>( _ => RefreshVisibleLibrariesList() );
            }

            RefreshVisibleLibrariesList();
        }

        void RefreshVisibleLibrariesList()
        {
            VisibleLibraries.Clear();
            if( App.Engine.LoggedUser != null )
            {
                foreach( Library library in from lib in App.Engine.LoggedUser.Libraries where lib.Visible select lib )
                {
                    VisibleLibraries.Add( library );
                }
            }
        }

        private void OnAddLibraryButtonClicked( object sender, RoutedEventArgs e )
        {
            RequestAddingLibrary();
        }

        private void RequestAddingLibrary()
        {
            ProgressBarOverlay.Show( FileLanguage.MainPage_AddingLibrary );

            longRunningOperation = App.Engine.AddLibrary( NewLibraryId.Text )
                                             .Finally( () => ProgressBarOverlay.Close() )
                                             .Subscribe<Unit>(
                                                 result => { /*no implementation needed*/ },
                                                 Utils.StandardErrorHandler
                                             );
        }

        private void OnLibraryClicked( object sender, SelectionChangedEventArgs args )
        {
            if( AllLibrariesList.SelectedIndex != -1 )
            {
                Library clickedLib = AllLibrariesList.SelectedItem as Library;
                Debug.Assert( clickedLib != null );

                if( clickedLib.CatalogueCount == -1 )
                {
                    ProgressBarOverlay.Show( FileLanguage.MainPage_DownloadingLib );

                    longRunningOperation = App.Engine.DownloadLibrary( clickedLib )
                                                     .Finally( ProgressBarOverlay.Close )
                                                     .Subscribe<Library>(
                                                         library =>
                                                         {
                                                             // keep ProgressBarOverlay.Close() call here despite it's also in Finally,
                                                             // because it has to be called before NavigateToLibrary, i.e. before
                                                             // ApplicationBar for new page is loaded.
                                                             ProgressBarOverlay.Close();
                                                             NavigateToLibrary( library );
                                                         },
                                                         Utils.StandardErrorHandler
                                                     );
                }
                else
                {
                    NavigateToLibrary( clickedLib );
                }

                // set selected index to -1 to enable repeated clicks on item
                AllLibrariesList.SelectedIndex = -1;
            }
        }

        private void NavigateToLibrary( Library library )
        {
            NedEngine.Utils.NavigateTo( "/CataloguePage.xaml?lib=true&id=" + library.ServerId );
        }

        private void OnContextMenuActivated( object sender, RoutedEventArgs args )
        {
            MenuItem menuItem = sender as MenuItem;
            Debug.Assert( menuItem != null );
            Library library = menuItem.CommandParameter as Library;
            Debug.Assert( library != null );

            switch( menuItem.Tag.ToString() )
            {
                case "DeleteTag":
                    if( MessageBox.Show( FileLanguage.QUESTION_REMOVE_LIBRARY, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel ) == MessageBoxResult.OK )
                    {
                        App.Engine.DeleteLibrary( library );
                    }
                    break;
                case "CheckForUpdatesTag":
                    {
                        ProgressBarOverlay.Show( FileLanguage.ProgressOverlay_UpdatingLibrary );

                        longRunningOperation = App.Engine.UpdateLibrary( library )
                                                         .Finally( ProgressBarOverlay.Close )
                                                         .Subscribe<Library>(
                                                             result => { /*no implementation needed*/ },
                                                             Utils.StandardErrorHandler
                                                         );
                    }
                    break;
                case "DownloadAllTag":
                    {
                        string libraryId = library.ServerId;
                        if( library.CatalogueCount == -1 )
                        {
                            ProgressBarOverlay.Show( FileLanguage.MainPage_DownloadingLib );

                            longRunningOperation = App.Engine.DownloadLibrary( library )
                                                             .Finally( ProgressBarOverlay.Close )
                                                             .Subscribe<Library>(
                                                                 libraryParam =>
                                                                 {
                                                                     ProgressBarOverlay.Close();
                                                                     if( !App.Engine.LibraryModel.IsLoaded() || App.Engine.LibraryModel.LibraryId != libraryId )
                                                                     {
                                                                         App.Engine.LibraryModel.LoadLibrary( libraryId );
                                                                     }
                                                                     DownloadAllCommand.GetCommand().Execute( libraryId );
                                                                 },
                                                                 Utils.StandardErrorHandler
                                                             );
                        }
                        else
                        {
                            if( !App.Engine.LibraryModel.IsLoaded() || App.Engine.LibraryModel.LibraryId != libraryId )
                            {
                                App.Engine.LibraryModel.LoadLibrary( libraryId );
                            }
                            DownloadAllCommand.GetCommand().Execute( libraryId );
                        }

                    }
                    break;
                case "SearchTag":
                    {
                        string libraryId = library.ServerId;
                        if( library.CatalogueCount == -1 )
                        {
                            ProgressBarOverlay.Show( FileLanguage.MainPage_DownloadingLib );

                            longRunningOperation = App.Engine.DownloadLibrary( library )
                                                             .Finally( ProgressBarOverlay.Close )
                                                             .Subscribe<Library>(
                                                                 libraryParam =>
                                                                 {
                                                                     ProgressBarOverlay.Close();
                                                                     if( !App.Engine.LibraryModel.IsLoaded() || App.Engine.LibraryModel.LibraryId != libraryId )
                                                                     {
                                                                         App.Engine.LibraryModel.LoadLibrary( libraryId );
                                                                     }
                                                                     ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/SearchPage.xaml?rootId=" + libraryId, UriKind.Relative ) );
                                                                 },
                                                                 Utils.StandardErrorHandler
                                                             );
                        }
                        else
                        {
                            if( !App.Engine.LibraryModel.IsLoaded() || App.Engine.LibraryModel.LibraryId != libraryId )
                            {
                                App.Engine.LibraryModel.LoadLibrary( libraryId );
                            }
                            ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/SearchPage.xaml?rootId=" + libraryId, UriKind.Relative ) );
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Tips And Tricks

        private void OnHideButtonClicked(object sender, RoutedEventArgs e)
        {
            GoForwardToScreen(Screen.MainMenu);
        }

        private void OnNextButtonClicked(object sender, RoutedEventArgs e)
        {
            App.Engine.TipsAndTricks.RollNext();
        }

        #endregion Tips And Tricks

        #region Downloads

        private void OnDownloadClicked( object sender, SelectionChangedEventArgs args )
        {
            if( DownloadsList.SelectedIndex != -1 )
            {
                QueuedDownload clickedDownload = DownloadsList.SelectedItem as QueuedDownload;
                Debug.Assert( clickedDownload != null );

                // pause/resume download
                if( clickedDownload.State == QueuedDownload.DownloadState.Paused || clickedDownload.State == QueuedDownload.DownloadState.Stopped )
                {
                    App.Engine.StartDownload( clickedDownload );
                }
                else
                {
                    App.Engine.StatisticsManager.LogDownloadEnd( clickedDownload );
                    App.Engine.StopDownload( clickedDownload );
                }

                // set selected index to -1 to enable repeated clicks on item
                DownloadsList.SelectedIndex = -1;
            }
        }

        #endregion

        #region Help

        public void NavigateToHelpView( object sender, EventArgs e )
        {
            HelpPages pageToShow = HelpPages.EUnknownPage;
            try
            {
                if( VisibleScreen == Screen.SelectServer ) pageToShow = HelpPages.ESelectServerPage;
                else if( VisibleScreen == Screen.Login ) pageToShow = HelpPages.EUserLoginPage;
                else if( VisibleScreen == Screen.MainMenu )
                {
                    PivotItem pivotItem = ( MainMenuScreen.SelectedItem as PivotItem );
                    if( pivotItem.Name == "Libraries" ) pageToShow = HelpPages.ELibraryListPage;
                    else if( pivotItem.Name == "LibraryManager" ) pageToShow = HelpPages.ELibraryManagerPage;
                    else if( pivotItem.Name == "Downloads" ) pageToShow = HelpPages.EDownloadsPage;
                    else throw new InvalidOperationException();
                }
                else throw new InvalidOperationException();
            }
            catch( InvalidOperationException )
            {
                System.Diagnostics.Debug.Assert( false, "Tried to show help for unknown screen, either there is a new screen or sceen/pivot names has changed" );
                MessageBox.Show( FileLanguage.App_OpeningHelpErrorUnknowScreen );
                return;
            }

            ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/HelpPage.xaml?type=" + pageToShow.ToString(), UriKind.Relative ) );
        }

        #endregion

        #region Tombstoning

        private const string SelectedPivotItemIndexKey = "SelectedPivotItemIndex";
        private const string LibraryIdInLibraryManagerKey = "LibraryIdInLibraryManager";

        private void SavePageState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            if( MainMenuScreen != null )
            {
                state[SelectedPivotItemIndexKey] = MainMenuScreen.SelectedIndex;
                state[LibraryIdInLibraryManagerKey] = NewLibraryId.Text;
            }
        }
        private void LoadPageState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            if( state.ContainsKey( SelectedPivotItemIndexKey ) )
            {
                MainMenuScreen.SelectedIndex = (int)state[SelectedPivotItemIndexKey];
            }
            if( state.ContainsKey( LibraryIdInLibraryManagerKey ) )
            {
                NewLibraryId.Text = (string)state[LibraryIdInLibraryManagerKey];
            }

            SubscribeForLibraryChanges();
        }

        private void ClearMainPageState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            state.Remove( SelectedPivotItemIndexKey );
            state.Remove( LibraryIdInLibraryManagerKey );
        }

        #endregion Tombstoning


        private bool _aboutMsgBoxLock;
        private void OnAboutButtonClicked( object sender, EventArgs e )
        {
            if( !_aboutMsgBoxLock )
            {
                _aboutMsgBoxLock = true;
                Version version = new Version( PhoneHelper.GetAppAttribute( "Version" ) );
                MessageBox.Show( String.Format( FileLanguage.VERSION, version.Major, version.Minor, version.Revision ) );
                _aboutMsgBoxLock = false;
            }
        }

        private void OnFactoryResetButtonClicked( object sender, EventArgs e )
        {
            MessageBoxResult result = MessageBox.Show( FileLanguage.SettingsPage_FactoryResetInfoMessage, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel );
            if( result == MessageBoxResult.OK )
            {
                ProgressBarOverlay.Show( FileLanguage.SettingsPage_ClearingData );
                App.Engine.FactoryReset()
                    .Finally( () =>
                    {
                        VisibleScreen = Screen.SelectServer;
                    } )
                    .Finally( ProgressBarOverlay.Close )
                    .Subscribe();
            }
        }

        public void NavigateToSettings( object sender, EventArgs e )
        {
            NavigationService.Navigate( new Uri( "/SettingsPage.xaml", UriKind.Relative ) );
        }

        public void NavigateToStatistics( object sender, EventArgs e )
        {
            NavigationService.Navigate( new Uri( "/StatisticsPage.xaml", UriKind.Relative ) );
        }

        private void RemoveApplicationBarButton( string buttonText )
        {
            foreach( object button in ApplicationBar.Buttons )
            {
                ApplicationBarIconButton appBarButton = button as ApplicationBarIconButton;
                if( appBarButton.Text == buttonText )
                {
                    ApplicationBar.Buttons.Remove( button );
                    return;
                }
            }
        }

        private void OnTextBoxKeyReleased( object sender, KeyEventArgs args )
        {
            if( args.Key == Key.Enter )
            {
                if( sender == Username )
                    Password.Focus(); // Show move keyboard to password input
                else if( sender == Password )
                    LoginButton.Focus(); // Hide keyboard
                else if( sender == ServerUrl )
                {
                    ServerUrlButton.Focus(); // Hide keyboard
                    SaveServer();
                }
                else if( sender == NewLibraryId )
                {
                    NewLibraryAddButton.Focus(); // Hide keyboard
                    RequestAddingLibrary();
                }
            }
        }

        private bool backToContentLists = false;//pressing back returns to mediaitem list or catalog list from DownloadManager, supressiong exit app question
        private bool _closeQuestionMsgBoxLock;
        protected override void OnBackKeyPress( System.ComponentModel.CancelEventArgs args )
        {
            if( ProgressBarOverlay.IsOpen() && longRunningOperation != null )
            {
                longRunningOperation.Dispose();
                longRunningOperation = null;
                args.Cancel = true;
            }
            else
            {
                if( !_closeQuestionMsgBoxLock && !backToContentLists )
                {
                    _closeQuestionMsgBoxLock = true;
                    if( MessageBox.Show( FileLanguage.MainPage_ClosingApplicationMessage, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel ) == MessageBoxResult.Cancel )
                    {
                        args.Cancel = true;
                    }
                    _closeQuestionMsgBoxLock = false;
                }
            }
            backToContentLists = false;
            base.OnBackKeyPress( args );
        }

        private void UpdateMOTDAnimation( object sender, EventArgs args )
        {
            if( VisibleScreen == Screen.MainMenu )
                SetMOTDActive( MainMenuScreen.SelectedItem == Libraries );
            else
                SetMOTDActive( false );
        }

        private void SetMOTDActive( bool active )
        {
            MottoOfTheDay.IsMarqueeAnimationRunning = active;
        }
    }

    public class ScreenVisibilityCoverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            object parameterValue = Enum.Parse( value.GetType(), parameter as string, true );
            return parameterValue.Equals( value ) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }

    public class CatalogueCountConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            int catalogueCount = ( (int)value );
            String catalogueCountString = catalogueCount == -1 ? "?" : catalogueCount.ToString();
            return String.Format( FileLanguage.CATALOGS, catalogueCountString );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }


    public class MediaViewsCountConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            int viewCount = ( (int)value );
            return String.Format( FileLanguage.MainPage_ViewsCount, viewCount );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }

    public class DownloadStateTextConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            switch( (NedEngine.QueuedDownload.DownloadState)value )
            {
                case QueuedDownload.DownloadState.Downloading: return FileLanguage.MainPage_Downloading;
                case QueuedDownload.DownloadState.Paused: return FileLanguage.MainPage_Paused;
                case QueuedDownload.DownloadState.Queued: return FileLanguage.MainPage_Queued;
                case QueuedDownload.DownloadState.Stopped: return FileLanguage.MainPage_Stopped;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }

    public class DemoInfoToVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            bool found = false;
            try
            {
                Library o = ( (NedEngine.ObservableCollectionEx<NedEngine.Library>)value ).First<Library>( ( Library inLib ) =>
                {
                    return inLib.ServerId == "khan";
                } );
                found = true;
            }
            catch( InvalidOperationException )
            {
            }

            return !found && Engine.IsDemoServerSelected() && App.Engine != null && App.Engine.LoggedUser != null && App.Engine.LoggedUser.Username == "guest" ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}