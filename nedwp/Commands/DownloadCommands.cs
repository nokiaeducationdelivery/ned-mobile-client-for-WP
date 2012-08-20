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
using System.Windows.Input;
using NedEngine;
using Coding4Fun.Phone.Controls;
using NedWp.Resources.Languages;
using System.Diagnostics;

namespace NedWp
{
    public abstract class DownloadCommandBase : ICommand
    {
        public abstract void Execute( object parameter );

        protected void Execute( object parameter, bool immediate )
        {
            MediaItemsListModelItem mediaItem = parameter as MediaItemsListModelItem;
            ToastPrompt toast = new ToastPrompt();
            switch( App.Engine.EnqueueMediaItem( mediaItem, immediate ) )
            {
                case Engine.AddingToQueueResult.ItemAlreadyDownloaded:
                    toast.Message = String.Format( FileLanguage.DownloadCommand_AlreadyDownloaded, mediaItem.Title == String.Empty ? FileLanguage.DownloadCommand_Item : mediaItem.Title );
                    break;
                case Engine.AddingToQueueResult.ItemAddedToQueue:
                    toast.Message = String.Format( FileLanguage.DownloadCommand_AddedToDownload, mediaItem.Title == String.Empty ? FileLanguage.DownloadCommand_Item : mediaItem.Title );
                    break;
                case Engine.AddingToQueueResult.DownloadItemStarted:
                    toast.Message = String.Format( FileLanguage.DownloadCommand_DownloadingStarted, mediaItem.Title == String.Empty ? FileLanguage.DownloadCommand_Item : mediaItem.Title );
                    break;
            }
            toast.Show();
        }

        public bool CanExecute( object parameter )
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged( EventArgs args )
        {
            if( CanExecuteChanged != null )
                CanExecuteChanged( this, args );
        }
    }

    public class AddItemToQueueCommand : DownloadCommandBase
    {
        protected static DownloadCommandBase mInstance = null;

        public static DownloadCommandBase GetCommand()
        {
            if( mInstance == null )
                mInstance = new AddItemToQueueCommand();
            return mInstance;
        }

        public override void Execute( object parameter )
        {
            bool immediate = false;
            base.Execute( parameter, immediate );
        }
    }

    public class DownloadNowCommand : DownloadCommandBase
    {
        protected static DownloadCommandBase mInstance = null;

        public static DownloadCommandBase GetCommand()
        {
            if( mInstance == null )
                mInstance = new DownloadNowCommand();
            return mInstance;
        }

        public override void Execute( object parameter )
        {
            bool immediate = true;
            base.Execute( parameter, immediate );
        }
    }

    public class DownloadAllCommand : ICommand
    {
        private static DownloadAllCommand mInstance = null;

        public static DownloadAllCommand GetCommand()
        {
            if( mInstance == null )
                mInstance = new DownloadAllCommand();
            return mInstance;
        }

        public void Execute( object parameter )
        {
            string contentId = parameter as string;
            Debug.Assert( contentId != null );
            int count = 0;
            foreach( var mediaItem in App.Engine.LibraryModel.GetAllMediaItemsUnderId( contentId ) )
            {
                if( App.Engine.EnqueueMediaItem( mediaItem, false ) == Engine.AddingToQueueResult.ItemAddedToQueue )
                {
                    ++count;
                }
            }

            ToastPrompt toast = new ToastPrompt();
            toast.Message = String.Format( FileLanguage.ITEM_ADDED_TO_QUEUE, count );
            toast.Show();

        }

        public bool CanExecute( object parameter )
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
        public void OnCanExecuteChanged( EventArgs args )
        {
            if( CanExecuteChanged != null )
                CanExecuteChanged( this, args );
        }
    }
}