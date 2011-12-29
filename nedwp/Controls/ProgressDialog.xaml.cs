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
using System.Windows.Data;
using System.Globalization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace NedWp
{
    public partial class ProgressDialog : UserControl
    {
        public Brush BackgroundColor 
        {
            get
            {
                if ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                    return new SolidColorBrush(Colors.Black);
                else
                    return new SolidColorBrush(Colors.White);
            }
        }
        public ProgressDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Show(string text)
        {
            SetApplicationBarEnabled(false);
            ProgressTitle.Text = text;
            Progress.IsIndeterminate = true;
            Visibility = Visibility.Visible;
        }

        public void Close()
        {
            if (Visibility == Visibility.Collapsed)
                return;

            Visibility = Visibility.Collapsed;
            Progress.IsIndeterminate = false;
            SetApplicationBarEnabled(true);
        }

        public bool IsOpen()
        {
            return Visibility == Visibility.Visible;
        }

        private void SetApplicationBarEnabled( bool enabled )
        {
            ApplicationBar appBar = ((PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content).ApplicationBar as ApplicationBar;
            
            appBar.IsMenuEnabled = enabled;
            foreach (ApplicationBarIconButton buttons in appBar.Buttons)
            {
                buttons.IsEnabled = enabled;
            }
        }
    }
}
