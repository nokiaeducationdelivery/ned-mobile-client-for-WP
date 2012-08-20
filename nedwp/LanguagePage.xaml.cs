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
using System.Collections.ObjectModel;
using NedWp.Commands;

namespace NedWp
{
    public partial class LanguagePage : PhoneApplicationPage
    {
        private string ContentId { get; set; }

        private string pageNavigatedLanguage = null;

        private const string KLanguagesPageUpdateInProgress = "KLanguagesPageUpdateInProgress";

        private bool IsLoaded = false; // Dirty workaround for CollectionViewSource issue - it selects first item in collection by default and in WP7.0 we navigate basing on selection

        public LanguagePage()
        {
            InitializeComponent();

            LanguageList.DataContext = App.Engine.ApplicationSettings.AvailableLanguages.LanguageList;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            CreateApplicationBarButtons();
        }

        private enum ApplicationBarButtons
        {
            RefreshLanguages,
            DownloadAgain,
            Help
        }

        private Dictionary<ApplicationBarButtons, IApplicationBarMenuItem> _applicationBarButtons = new Dictionary<ApplicationBarButtons, IApplicationBarMenuItem>();
        private void CreateApplicationBarButtons()
        {
            ApplicationBarIconButton helpButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.questionmark.rest.png", UriKind.Relative));
            helpButton.Click += NavigateToHelpView;
            helpButton.Text = AppResources.App_HelpButtonContent;

            ApplicationBarIconButton updateButton = new ApplicationBarIconButton(new Uri("/Resources/OriginalPlatformIcons/appbar.refresh.rest.png", UriKind.Relative));
            updateButton.Click += OnRefreshClicked;
            updateButton.Text = AppResources.CataloguePage_RefreshButton;

            _applicationBarButtons.Add(ApplicationBarButtons.Help, helpButton);
            _applicationBarButtons.Add(ApplicationBarButtons.RefreshLanguages, updateButton);
        }

        private void UpdateApplicationBarButtons()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();


            ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.Help]);
            ApplicationBar.Buttons.Add(_applicationBarButtons[ApplicationBarButtons.RefreshLanguages]);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (LanguageList.SelectedIndex < 0 || !IsLoaded)
                return;
            DownloadLocalization.GetCommand().Execute(LanguageList.SelectedItem as LanguageInfo);

           // NavigationService.GoBack();
            LanguageList.SelectedIndex = -1; // Reset selection
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

        private void OnRefreshClicked(object sender, EventArgs args)
        {
            UpdateLanguages();
        }

        private IDisposable longRunningOperation = null;
        private void UpdateLanguages()
        {
            ProgressBarOverlay.Show(AppResources.ProgressOverlay_UpdatingLibrary);

            longRunningOperation = App.Engine.RequestLanguageListUpdate()
                                             .ObserveOnDispatcher()
                                             .Finally(
                                             () =>
                                             {
                                                 ProgressBarOverlay.Close();

                                                 longRunningOperation = null;
                                                 PhoneApplicationService.Current.State.Remove(KLanguagesPageUpdateInProgress);

                                                 // leave this page if library contents were deleted (for example
                                                 // on failed update or after user interruption)
                                                 if (App.Engine.ApplicationSettings.AvailableLanguages.LanguageList.Count == -1)
                                                 {
                                                     MessageBox.Show(AppResources.LibraryUnavailableAfterFailedUpdate);
                                                     NavigationService.GoBack();
                                                 }
                                             })
                                             .Subscribe<List<LanguageInfo>>(
                                                 languageList => ReloadLanguages(languageList),
                                                 Utils.StandardErrorHandler
                                             );
        }

        private void ReloadLanguages(List<LanguageInfo> languageList)
        {
            App.Engine.ApplicationSettings.AvailableLanguages.LoadNewList(languageList);
        }

        public void NavigateToHelpView(object sender, EventArgs e)
        {
            HelpPages pageToShow = HelpPages.EUnknownPage;
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/HelpPage.xaml?type=" + pageToShow.ToString(), UriKind.Relative));
        }

        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/SearchPage.xaml?rootId=" + ContentId, UriKind.Relative));
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

            if (PhoneApplicationService.Current.State.ContainsKey(KLanguagesPageUpdateInProgress) &&
                (bool)(PhoneApplicationService.Current.State[KLanguagesPageUpdateInProgress]))
            {
                UpdateApplicationBarButtons();
                UpdateLanguages();
            }
            else
            {
                ReloadLanguages();
                UpdateApplicationBarButtons();
            }

            // workaround for flicker during recursive back
            if (!ApplicationBar.IsVisible)
                ApplicationBar.IsVisible = true;

            pageNavigatedLanguage = App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage;
        }

        private void ReloadLanguages()
        {
            IsLoaded = true;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs args)
        {
            // workaround for flicker during recursive back
            if (!(args.Content is LinksListPage))
            {
                ApplicationBar.IsVisible = false;
            }

            if (App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage != pageNavigatedLanguage)
            {
                MessageBox.Show(AppResources.RestartApplicationForLanguage);
            }

            pageNavigatedLanguage = null;

            if (longRunningOperation != null)
            {
                PhoneApplicationService.Current.State[KLanguagesPageUpdateInProgress] = true;
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
            base.OnBackKeyPress(e);
        }
    }
}
