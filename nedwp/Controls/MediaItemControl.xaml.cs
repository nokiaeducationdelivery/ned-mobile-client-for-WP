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
using System.Windows.Data;
using System.Globalization;
using Coding4Fun.Phone.Controls;

namespace NedWp
{
    public partial class MediaItemControl : UserControl
    {
        private const string DeleteTag = "DeleteTag";
        private const string DownloadNowTag = "DownloadNowTag";
        private const string AddToQueueTag = "AddToQueueTag";
        private const string ShowLinksTag = "ShowLinksTag";
        private const string ShowDescriptionTag = "ShowDescriptionTag";

        public static readonly DependencyProperty ContextMenuProperty = DependencyProperty.Register("MediaContextMenu", typeof(ContextMenu), typeof(MediaItemControl), null);
        public ContextMenu ContextMenu
        {
            get { return GetValue(ContextMenuProperty) as ContextMenu; }
            set
            {
                SetValue(ContextMenuProperty, value);
            }
        }
        
        public static readonly DependencyProperty IsDownloadedProperty = DependencyProperty.Register("IsDownloaded", typeof(bool), typeof(MediaItemControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsDownloadedChangedChanged)));
        public bool IsDownloaded
        {
            get { return (bool)GetValue(IsDownloadedProperty); }
            set
            {
                SetValue(IsDownloadedProperty, value);
                OnDownloadedChanged();
            }
        }

        public static readonly DependencyProperty AvailabilityIconProperty = DependencyProperty.Register("AvailabilityIcon", typeof(string), typeof(MediaItemControl), new PropertyMetadata(String.Empty));
        public string AvailabilityIcon
        {
            get { return (string)GetValue(AvailabilityIconProperty); }
            set { SetValue(AvailabilityIconProperty, value); }
        }
           

        public MediaItemControl()
        {
            InitializeComponent();
            ContextMenu = ContextMenuService.GetContextMenu((UIElement)FindName("MediaItemRoot"));
        }

        private void OnMenuItemClicked(object sender, RoutedEventArgs args)
        {
            var menuItem = (MenuItem)sender;
            MediaItemsListModelItem mediaItem = (MediaItemsListModelItem)menuItem.CommandParameter;
            var tag = menuItem.Tag.ToString();
            switch (tag)
            {
                case DeleteTag:
                    DeleteLibraryItemCommand.GetCommand().Execute(mediaItem);
                    break;
                case AddToQueueTag:
                    AddItemToQueueCommand.GetCommand().Execute(mediaItem);
                    break;
                case DownloadNowTag:
                    DownloadNowCommand.GetCommand().Execute(mediaItem);
                    break;
                case ShowLinksTag:
                    ShowLinksCommand.GetCommand().Execute(mediaItem);
                    break;
                case ShowDescriptionTag:
                    ShowDescriptionCommand.GetCommand().Execute(mediaItem);
                    break;
                default:
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
                    case AddToQueueTag:
                        item.Visibility = IsDownloaded ? Visibility.Collapsed : Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnIsDownloadedChangedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            MediaItemControl userControl = sender as MediaItemControl;
            userControl.IsDownloaded = (bool)args.NewValue;
        }
    }

    public class AvailabilityIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MediaItemState state = (MediaItemState)value;
            string iconPath = String.Empty;
            switch(state) {
                case MediaItemState.Remote:
                    iconPath = "../Resources/MediaItemIcons/small_remote_content_icon.png";
                    break;
                case MediaItemState.Downloading:
                    iconPath = "../Resources/MediaItemIcons/download_in_progress.png";
                    break;
                case MediaItemState.Local:
                    iconPath = "../Resources/MediaItemIcons/small_local_content_icon.png";
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "Could not determine MediaItemControl icon due to unknown item state");
                    break;
            }
            return iconPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}