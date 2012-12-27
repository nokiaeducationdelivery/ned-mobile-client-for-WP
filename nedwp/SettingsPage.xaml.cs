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
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Shell;
using NedWp.Resources.Languages;
using System.Windows.Data;

namespace NedWp
{
    public partial class SettingsPage : PhoneApplicationPage
    {

        enum OperationsStates
        {
            RemovingUsers,
            FactoryReseting,
            LoggingOut,
            WaitingForUserCommand,
            SelectingLanguages
        }
        private OperationsStates _operationsState = OperationsStates.WaitingForUserCommand;
        private const string _operationsStateKey = "SettingsOperationsState";
        private bool _isNewInstance = true;

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = App.Engine;
            TipsAndTricks.DataContext = App.Engine.ApplicationSettings;

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

        protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs e )
        {
            if( _isNewInstance )
            {
                _isNewInstance = false;
                IDictionary<string, object> state = PhoneApplicationService.Current.State;
                if( state.ContainsKey( _operationsStateKey ) )
                {
                    _operationsState = (OperationsStates)state[( _operationsStateKey )];
                    switch( _operationsState )
                    {
                        case OperationsStates.FactoryReseting:
                        case OperationsStates.RemovingUsers:
                        case OperationsStates.LoggingOut:
                            if( App.Engine.LoggedUser == null )
                            {
                                try
                                {
                                    state.Remove( _operationsStateKey );
                                    NavigationService.GoBack();
                                }
                                catch( InvalidOperationException ) { }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            base.OnNavigatedTo( e );
        }
        protected override void OnNavigatedFrom( System.Windows.Navigation.NavigationEventArgs e )
        {
            if( _operationsState != OperationsStates.WaitingForUserCommand )
            {
                IDictionary<string, object> state = PhoneApplicationService.Current.State;
                if( state.ContainsKey( _operationsStateKey ) )
                {
                    state[_operationsStateKey] = _operationsState;
                }
                else
                {
                    state.Add( _operationsStateKey, _operationsState );
                }
            }
            base.OnNavigatedFrom( e );
        }

        private void OnSelectLanguageClicked( object sender, RoutedEventArgs e )
        {
            _operationsState = OperationsStates.SelectingLanguages;
            NavigationService.Navigate( new Uri( "/LanguagePage.xaml", UriKind.Relative ) );
        }

        private void OnLogoutButtonClicked( object sender, RoutedEventArgs e )
        {
            _operationsState = OperationsStates.LoggingOut;
            MessageBoxResult result = MessageBox.Show( FileLanguage.QUESTION_LOGOUT_USER, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel );
            if( result == MessageBoxResult.OK )
            {
                ProgressBarOverlay.Show( FileLanguage.SettingsPage_LoggingOut );
                App.Engine.Logout()
                    .Finally( () => { try { NavigationService.GoBack(); } catch( InvalidOperationException ) { } } )
                    .Finally( ProgressBarOverlay.Close )
                    .Subscribe();
            }
        }

        private void OnRemoveUserButtonClicked( object sender, RoutedEventArgs e )
        {
            _operationsState = OperationsStates.RemovingUsers;
            MessageBoxResult result = MessageBox.Show( FileLanguage.SettingsPage_UsersRemovedInfoMessage, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel );
            if( result == MessageBoxResult.OK )
            {
                ProgressBarOverlay.Show( FileLanguage.SettingsPage_RemovingUser );
                User removedUser = App.Engine.LoggedUser;
                App.Engine.Logout()
                          .Finally(
                            () =>
                            {
                                App.Engine.RemoveUsers( removedUser );
                                ProgressBarOverlay.Close();
                                try
                                {
                                    NavigationService.GoBack();
                                }
                                catch( InvalidOperationException ) { }
                            } )
                          .Subscribe();
            }
        }

        protected override void OnBackKeyPress( System.ComponentModel.CancelEventArgs args )
        {
            if( ProgressBarOverlay.IsOpen() )
            {
                args.Cancel = true;
            }
            base.OnBackKeyPress( args );
        }

        private void OnFactoryResetButtonClicked( object sender, RoutedEventArgs e )
        {
            _operationsState = OperationsStates.FactoryReseting;
            MessageBoxResult result = MessageBox.Show( FileLanguage.SettingsPage_FactoryResetInfoMessage, FileLanguage.ARE_YOU_SURE, MessageBoxButton.OKCancel );
            if( result == MessageBoxResult.OK )
            {
                ProgressBarOverlay.Show( FileLanguage.SettingsPage_ClearingData );
                App.Engine.FactoryReset()
                    .Finally( () => { try { NavigationService.GoBack(); } catch( InvalidOperationException ) { } } )
                    .Finally( ProgressBarOverlay.Close )
                    .Subscribe();
            }
        }

        public void NavigateToHelpView( object sender, EventArgs e )
        {
            ( Application.Current.RootVisual as PhoneApplicationFrame ).Navigate( new Uri( "/HelpPage.xaml?type=" + HelpPages.ESettingsPage.ToString(), UriKind.Relative ) );
        }
    }

    public class BoolToSwitchConverter : IValueConverter
    {
        private string FalseValue = FileLanguage.MID_OFF_SETTINGS;
        private string TrueValue = FileLanguage.MID_ON_SETTINGS;

        public object Convert( object value, Type targetType, object parameter,
              System.Globalization.CultureInfo culture )
        {
            if( value == null )
                return FalseValue;
            else
                return ( "On".Equals( value ) ) ? TrueValue : FalseValue;
        }

        public object ConvertBack( object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture )
        {
            return value != null ? value.Equals( TrueValue ) : false;
        }
    }
}
