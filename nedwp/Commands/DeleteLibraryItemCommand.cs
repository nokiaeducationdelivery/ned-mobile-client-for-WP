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
using NedEngine;

namespace NedWp
{
    public class DeleteLibraryItemCommand : ICommand
    {
        private static DeleteLibraryItemCommand mInstance = null;

        public static DeleteLibraryItemCommand GetCommand()
        {
            if (mInstance == null)
                mInstance = new DeleteLibraryItemCommand();
            return mInstance;
        }

        public void Execute(object parameter)
        {
            App.Engine.LibraryModel.DeleteItem(parameter as LibraryModelItem);
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
