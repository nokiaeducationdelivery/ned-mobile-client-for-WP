/*******************************************************************************
* Copyright (c) 2012 Nokia Corporation
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
using Microsoft.Phone.Reactive;
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
using System.Windows.Data;
using System.Globalization;
using Coding4Fun.Phone.Controls;
using System.Diagnostics;

namespace NedWp
{
    public partial class LanguageItemControl : UserControl
    {

        private const string DownloadNowTag = "DownloadNowTag";
        private const string DownloadAgainTag = "DownloadAgainTag";

        public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.Register("LanguageContextMenu", typeof(ContextMenu), typeof(LanguageItemControl), null);
        public ContextMenu ContextMenu
        {
            get { return GetValue(ContextMenuProperty) as ContextMenu; }
            set
            {
                SetValue(ContextMenuProperty, value);
            }
        }


        public static readonly DependencyProperty IsDownloadedProperty = DependencyProperty.Register("IsDownloaded", typeof(bool), typeof(LanguageItemControl), new PropertyMetadata(new PropertyChangedCallback(OnIsDownloadedChangedChanged)));
        public bool IsDownloaded
        {
            get { return (bool)GetValue(IsDownloadedProperty); }
            set
            {
                SetValue(IsDownloadedProperty, value);
                OnDownloadedChanged();
            }
        }

        public static readonly DependencyProperty AvailabilityIconProperty = DependencyProperty.Register("AvailabilityIcon", typeof(string), typeof(LanguageItemControl), new PropertyMetadata(String.Empty));
        public string AvailabilityIcon
        {
            get { return (string)GetValue(AvailabilityIconProperty); }
            set { SetValue(AvailabilityIconProperty, value); }
        }

        public LanguageItemControl()
        {
            InitializeComponent();
            ContextMenu = ContextMenuService.GetContextMenu((UIElement)FindName("LanguageItemRoot"));
            OnDownloadedChanged();
        }

        private static void OnIsDownloadedChangedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            LanguageItemControl userControl = sender as LanguageItemControl;
            userControl.IsDownloaded = (bool)args.NewValue;
        }

        private void OnContextMenuActivated(object sender, RoutedEventArgs args)
        {
            MenuItem menuItem = sender as MenuItem;
            Debug.Assert(menuItem != null);
            LanguageInfo downloadItem = menuItem.CommandParameter as LanguageInfo;
            Debug.Assert(downloadItem != null);

            switch (menuItem.Tag.ToString())
            {
                case DownloadAgainTag:
                    downloadItem.ItemState = MediaItemState.Downloading;
                    App.Engine.DownloadLocalization(downloadItem.Id)
                        .ObserveOnDispatcher()
                        .Subscribe<bool>(success => 
                        {
                            downloadItem.ItemState = MediaItemState.Local;
                        }
                        , ex => 
                        { 
                            System.Diagnostics.Debug.Assert(false, "Exception in donwloading laguage again");
                            downloadItem.ItemState = MediaItemState.Local;
                        });
                    break;
                case DownloadNowTag:
                    downloadItem.ItemState = MediaItemState.Downloading;
                    App.Engine.DownloadLocalization(downloadItem.Id)
                        .ObserveOnDispatcher()
                        .Subscribe<bool>(success =>
                        {
                            if (success)
                            {
                                downloadItem.IsLocal = true;
                            }
                            else
                            {
                                downloadItem.ItemState = MediaItemState.Remote;
                            }
                        }
                        , ex =>
                        {
                            System.Diagnostics.Debug.Assert(false, "Exception in donwloading laguage now");
                            downloadItem.ItemState = MediaItemState.Local;
                        });
                    break;
            }
        }

        private void OnDownloadedChanged()
        {
            // NOTE: Implemented this way because could not apply converter to MenuItems
            foreach (MenuItem item in ContextMenu.Items)
            {
                switch (item.Tag as string)
                {
                    case DownloadNowTag:
                        item.Visibility = IsDownloaded ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    case DownloadAgainTag:
                        item.Visibility = IsDownloaded ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }
        }
    }

}