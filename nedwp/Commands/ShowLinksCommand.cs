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
using System.Windows.Input;
using Microsoft.Phone.Controls;
using NedEngine;

namespace NedWp
{
    public class ShowLinksCommand : ICommand
    {
        private static ShowLinksCommand mInstance = null;

        public static ShowLinksCommand GetCommand()
        {
            if (mInstance == null)
                mInstance = new ShowLinksCommand();
            return mInstance;
        }

        public void Execute(object parameter)
        {
            MediaItemsListModelItem mediaItem = parameter as MediaItemsListModelItem;
            App.Engine.StatisticsManager.LogShowLinks(mediaItem);
            ((App.Current.RootVisual as PhoneApplicationFrame).Content as PhoneApplicationPage).NavigationService.Navigate(new Uri("/LinksListPage.xaml?id=" + mediaItem.Id, UriKind.Relative));
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
