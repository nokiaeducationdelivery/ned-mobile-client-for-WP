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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NedEngine;
using Microsoft.Phone.Controls;
using Coding4Fun.Phone.Controls;
using System.Diagnostics;

namespace NedWp
{
    public class MediaItemRequestedCommand : ICommand
    {
        private static MediaItemRequestedCommand mInstance = null;

        public static MediaItemRequestedCommand GetCommand()
        {
            if (mInstance == null)
                mInstance = new MediaItemRequestedCommand();
            return mInstance;
        }

        public void Execute(object parameter)
        {
            MediaItemsListModelItem mediaItem = parameter as MediaItemsListModelItem;
            
            switch (mediaItem.ItemState)
            {
                case MediaItemState.Local:
                    // TEMPORARY: log media item playback
                    App.Engine.StatisticsManager.LogMediaPlayback(mediaItem);
                    NedEngine.Utils.NavigateTo("/MediaItemsViewerPage.xaml?id=" + mediaItem.Id);
                    break;
                case MediaItemState.Downloading:
                    ToastPrompt toast = new ToastPrompt();
                    toast.Message = String.Format("{0} is already queued for download", mediaItem.Title == String.Empty ? "Item" : mediaItem.Title);
                    toast.Show();
                    break;
                case MediaItemState.Remote:
                    AddItemToQueueCommand.GetCommand().Execute(mediaItem);
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "Unknown media item state when media item requested - unable to make decision what to do");
                    break;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }
    }
}
