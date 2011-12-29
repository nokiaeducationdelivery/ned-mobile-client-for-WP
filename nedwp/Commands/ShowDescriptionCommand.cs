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
using System.Windows;
using System.Windows.Input;
using NedEngine;

namespace NedWp
{
    public class ShowDescriptionCommand : ICommand
    {
        private static ShowDescriptionCommand mInstance = null;

        public static ShowDescriptionCommand GetCommand()
        {
            if (mInstance == null)
                mInstance = new ShowDescriptionCommand();
            return mInstance;
        }

        // MessageBox.Show is theoretically a blocking call, but internally it
        // processes the UI thread event loop. It means that if user manages to
        // enqueue multiple click events, all those events will be processed
        // simultaneously. The callstack looks like this:
        //
        // ---TOP---
        // Exception
        // MessageBox.Show
        // UserEventHandler
        // (some internal event handler calls)
        // MessageBox.Show
        // UserEventHandler
        // (some internal event handler calls)
        //
        // Showing the second Message Box throws an exception. The only way to
        // prevent this is to use some kind of a flag preventing nested event
        // handling (lock C# statement cannot be used, because it protects only
        // from accessing locked object from other thread and here everything
        // happens in the same thread).
        private bool _msgBoxLock = false;
        public void Execute(object parameter)
        {
            if (!_msgBoxLock)
            {
                _msgBoxLock = true;
                MediaItemsListModelItem mediaItem = parameter as MediaItemsListModelItem;
                App.Engine.StatisticsManager.LogShowMediaDetails(mediaItem);
                MessageBox.Show(mediaItem.Description);
                _msgBoxLock = false;
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
