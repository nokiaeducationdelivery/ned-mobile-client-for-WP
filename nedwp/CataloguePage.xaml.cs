using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using NedEngine;
using System.Diagnostics;
using System.Windows.Data;
using Microsoft.Phone.Shell;
using Coding4Fun.Phone.Controls;
using NedWp.Resources.Languages;
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
using System.Text;
using Microsoft.Phone.Reactive;

namespace NedWp
{
    public partial class CataloguePage : PhoneApplicationPage
    {
        private string ContentId { get; set; }
        private CollectionViewSource CollectionFilter;
        private NedEngine.LibraryModel.LibraryLevel PageType { get; set; }

        private const string KCataloguePageUpdateInProgress = "KCataloguePageUpdateInProgress";

        private bool IsLoaded = false; // Dirty workaround for CollectionViewSource issue - it selects first item in collection by default and in WP7.0 we navigate basing on selection

        public CataloguePage()
        {
            InitializeComponent();
            CollectionFilter = Resources["CollectionFilter"] as CollectionViewSource;
            CatalogueList.DataContext = CollectionFilter;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;
            CreateApplicationBarButtons();
        }

        private enum ApplicationBarButtons
        {
            Help,
            Search,
            DownloadAll,
            Update,
            Home
        }

        private Dictionary<ApplicationBarButtons, ApplicationBarIconButton> _applicationBarButtons = new Dictionary<ApplicationBarButtons, ApplicationBarIconButton>();
        private void CreateApplicationBarButtons()
        {
            ApplicationBarIconButton helpButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative));
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = AppResources.App_HelpButtonContent;
            ApplicationBarIconButton searchButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.feature.search.rest.png", UriKind.Relative));
            searchButton.Click += OnSearchButtonClicked;
            searchButton.Text = AppResources.CataloguePage_SearchButton;
            ApplicationBarIconButton downloadAllButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.download.rest.png", UriKind.Relative));
            downloadAllButton.Click += OnDownloadAllClicked;
            downloadAllButton.Text = AppResources.CataloguePage_DownloadAllButton;
            ApplicationBarIconButton updateButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.refresh.rest.png", UriKind.Relative));
            updateButton.Click += OnRefreshClicked;
            updateButton.Text = AppResources.CataloguePage_RefreshButton;
            ApplicationBarIconButton homeButton = new ApplicationBarIconButton(new Uri("/Resources/Icons/home.png", UriKind.Relative));
            homeButton.Click += OnHomeClicked;
            homeButton.Text = AppResources.CataloguePage_HomeButton;

            _applicationBarButtons.Add(ApplicationBarButtons.Update, updateButton);
            _applicationBarButtons.Add(ApplicationBarButtons.DownloadAll, downloadAllButton);
            _applicationBarButtons.Add(ApplicationBarButtons.Search, searchButton);
            _applicationBarButtons.Add(ApplicationBarButtons.Help, helpButton);
            _applicationBarButtons.Add(ApplicationBarButtons.Home, homeButton);
        }

        private void UpdateApplicationBarButtons()
        {
            ApplicationBar.Buttons.Clear();

            if (PageType == LibraryModel.LibraryLevel.Catalogue)
            {
                ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.Update]);
            }
            else
            {
                ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.Home]);
            }

            ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.DownloadAll]);
            ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.Search]);
            ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.Help]);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (CatalogueList.SelectedIndex < 0 || !IsLoaded)
                return;
            switch (PageType)
            {
                case LibraryModel.LibraryLevel.Catalogue:
                    {
                        string nextPageId = (CatalogueList.SelectedItem as LibraryModelItem).Id;
                        App.Engine.StatisticsManager.LogCatalogueOpen(nextPageId);
                        NavigationService.Navigate(new Uri("/CataloguePage.xaml?id=" + nextPageId, UriKind.Relative));
                        break;
                    }
                case LibraryModel.LibraryLevel.Category:
                    {
                        string nextPageId = (CatalogueList.SelectedItem as LibraryModelItem).Id;
                        App.Engine.StatisticsManager.LogCategoryOpen(nextPageId);
                        NavigationService.Navigate(new Uri("/CataloguePage.xaml?id=" + nextPageId, UriKind.Relative));
                        break;
                    }
                case LibraryModel.LibraryLevel.MediaItemsList:
                    MediaItemRequestedCommand.GetCommand().Execute(CatalogueList.SelectedItem as MediaItemsListModelItem);
                    break;
                default:
                    break;
            }
            CatalogueList.SelectedIndex = -1; // Reset selection
        }

        private void OnContextMenuActivated(object sender, RoutedEventArgs args)
        {
            var menuItem = (MenuItem)sender;
            var tag = menuItem.Tag.ToString();
            switch (tag)
            {
                case "DeleteTag":
                    DeleteLibraryItemCommand.GetCommand().Execute((sender as MenuItem).CommandParameter as LibraryModelItem);
                    break;
                default:
                    break;
            }
        }

        private void OnDownloadAllClicked(object sender, EventArgs args)
        {
            DownloadAllCommand.GetCommand().Execute(ContentId);
        }

        private void OnHomeClicked(object sender, EventArgs args)
        {
            App.RecursiveBack = true;
            NavigationService.GoBack();
        }

        private void OnRefreshClicked(object sender, EventArgs args)
        {
            UpdateLibrary(App.Engine.LibraryModel.ActiveLibrary);
        }

        private IDisposable longRunningOperation = null;
        private void UpdateLibrary(Library library)
        {
            ProgressBarOverlay.Show(AppResources.ProgressOverlay_UpdatingLibrary);

            longRunningOperation = App.Engine.UpdateLibrary(library)
                                             .Finally(
                                             () =>
                                             {
                                                 ProgressBarOverlay.Close();

                                                 longRunningOperation = null;
                                                 PhoneApplicationService.Current.State.Remove(KCataloguePageUpdateInProgress);

                                                 // leave this page if library contents were deleted (for example
                                                 // on failed update or after user interruption)
                                                 if (library.CatalogueCount == -1)
                                                 {
                                                     MessageBox.Show(AppResources.LibraryUnavailableAfterFailedUpdate);
                                                     NavigationService.GoBack();
                                                 }
                                             })
                                             .Subscribe<Library>(
                                                 _ => ReloadLibrary(true),
                                                 Utils.StandardErrorHandler
                                             );
        }

        public void NavigateToHelpView(object sender, EventArgs e)
        {
            HelpPages pageToShow = HelpPages.EUnknownPage;
            switch(PageType)
            {
                case LibraryModel.LibraryLevel.Catalogue:
                    pageToShow = HelpPages.ECataloguePage;
                    break;
                case LibraryModel.LibraryLevel.Category:
                    pageToShow = HelpPages.ECategoryPage;
                    break;
                case LibraryModel.LibraryLevel.MediaItemsList:
                    pageToShow = HelpPages.EMediaItemsPage;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "Tried to show help for unknown screen, either there is a new screen or sceen/pivot names has changed");
                    MessageBox.Show(AppResources.App_OpeningHelpErrorUnknowScreen);
                    return;
            }
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/HelpPage.xaml?type=" + pageToShow.ToString(), UriKind.Relative));
        }

        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/SearchPage.xaml?rootId=" + ContentId, UriKind.Relative));
        }

        private Predicate<object> GetFilterPredicateForPageType(LibraryModel.LibraryLevel level)
        {
            switch (level)
            {
                case LibraryModel.LibraryLevel.Catalogue:
                case LibraryModel.LibraryLevel.Category:
                    return new Predicate<Object>(FilterLibraryItemsPage);
                case LibraryModel.LibraryLevel.MediaItemsList:
                    return new Predicate<Object>(FilterLibraryItemsPage);
                default:
                    return null;
            }
        }

        private bool FilterLibraryItemsPage(object item)
        {
            if (item == null)
                return false;
            LibraryModelItem libraryItem = item as LibraryModelItem;
            return (libraryItem.ParentId == ContentId);
        }

        private void ReloadLibrary(bool forceReload)
        {
            IsLoaded = false;

            if (forceReload)
            {
                App.Engine.LibraryModel.LoadLibrary(ContentId);
            }
            else
            {
                App.Engine.LibraryModel.LoadLibraryStateIfNotLoaded();
            }
            PageType = App.Engine.LibraryModel.GetNodeType(ContentId);

            CollectionFilter.Source = App.Engine.LibraryModel.GetDataSourceForId(ContentId);
            CollectionFilter.View.Filter = GetFilterPredicateForPageType(PageType);
            CollectionFilter.View.MoveCurrentToPosition(-1);
            TitlePanel.TitleText = App.Engine.LibraryModel.GetNodeTitle(ContentId);
            IsLoaded = true;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            if (App.RecursiveBack)
            {
                TransitionService.SetNavigationInTransition(this, null);
                TransitionService.SetNavigationOutTransition(this, null);
                NavigationService.GoBack();
                return;
            }

            IDictionary<string, string> parameters = NavigationContext.QueryString;
            Debug.Assert(parameters.ContainsKey("id"));
            ContentId = parameters["id"];

            if (PhoneApplicationService.Current.State.ContainsKey(KCataloguePageUpdateInProgress) &&
                (bool)(PhoneApplicationService.Current.State[KCataloguePageUpdateInProgress]))
            {
                PageType = LibraryModel.LibraryLevel.Catalogue;
                UpdateApplicationBarButtons();
                UpdateLibrary(App.Engine.LoggedUser.Libraries.First(library => library.ServerId == ContentId));
            }
            else
            {
                ReloadLibrary(parameters.ContainsKey("lib"));
                UpdateApplicationBarButtons();
            }

            // workaround for flicker during recursive back
            if (!ApplicationBar.IsVisible)
                ApplicationBar.IsVisible = true;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs args)
        {
            // workaround for flicker during recursive back
            if (!(args.Content is LinksListPage))
            {
                ApplicationBar.IsVisible = false;
            }

            if (App.Engine.LibraryModel.ActiveLibrary != null)
            {
                App.Engine.LibraryModel.SaveLibraryState();
            }

            if (longRunningOperation != null)
            {
                PhoneApplicationService.Current.State[KCataloguePageUpdateInProgress] = true;
            }

            base.OnNavigatedFrom(args);
        }

        private void OnBackKeyPressed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ProgressBarOverlay.IsOpen() && longRunningOperation != null)
            {
                longRunningOperation.Dispose();
                longRunningOperation = null;
                e.Cancel = true;
            }
            else
            {
                switch (PageType)
                {
                    case LibraryModel.LibraryLevel.Catalogue:
                        //list of cataloue's, means that we are in library
                        App.Engine.StatisticsManager.LogLibraryBack(ContentId);
                        break;
                    case LibraryModel.LibraryLevel.Category:
                        // list of category means that we are in catalogue
                        App.Engine.StatisticsManager.LogCatalogueBack(ContentId);
                        break;
                    case LibraryModel.LibraryLevel.MediaItemsList:
                        // list of media items, means that we are in category
                        App.Engine.StatisticsManager.LogCategoryBack(ContentId);
                        break;
                }
            }
            base.OnBackKeyPress(e);
        }
    }
}
