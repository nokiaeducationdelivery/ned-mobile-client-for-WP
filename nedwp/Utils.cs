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
using Microsoft.Phone.Shell;
using System;
using System.Linq;
using NedWp.Resources.Languages;
using System.Windows;
using System.Net;

namespace NedWp
{
    public static class Utils
    {
        public static void PopulateWithButtons(this IApplicationBar applicationBar, ApplicationBarIconButton[] buttons)
        {
            // It seems that the list of buttons is 'specific' and calling Contans(x) ? Remove(x) won't work so it has to be cleared before"
            applicationBar.Buttons.Clear();
            for (int i = 0; i < buttons.Length; i++ )
                applicationBar.Buttons.Add(buttons[i]);
        }

        public static void RemoveAppBarButtonByText(this IApplicationBar applicationBar, string buttonText)
        {
            foreach (object button in applicationBar.Buttons)
            {
                ApplicationBarIconButton appBarButton = button as ApplicationBarIconButton;
                if (appBarButton.Text == buttonText)
                {
                    applicationBar.Buttons.Remove(button);
                    return;
                }
            }
        }

        #region Errors

        public static void StandardErrorHandler(Exception error)
        {
            HandleError(error,
                        BuildErrorHandler(IsConnectionError, AppResources.Error_Connection),
                        BuildErrorHandler(e => e is ArgumentException, error.Message));
        }

        private static void HandleError(Exception error, params Func<Exception, bool>[] errorHandlers)
        {
            if (!errorHandlers.Any(handler => handler(error)))
            {
                MessageBox.Show(AppResources.Error_Unexpected);
            }
        }

        private static Func<Exception, bool> BuildErrorHandler(Func<Exception, bool> predicate, string errorMessage)
        {
            return error =>
            {
                bool result = predicate(error);
                if (result)
                {
                    MessageBox.Show(errorMessage);
                }
                return result;
            };
        }

        private static bool IsConnectionError(Exception exception)
        {
            return ((exception is WebException) && (exception as WebException).Status == WebExceptionStatus.ConnectFailure);
        }

        #endregion
    }
}