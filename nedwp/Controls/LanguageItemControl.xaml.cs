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
using Microsoft.Phone.Controls;
using NedEngine;
using System.Windows.Data;
using System.Globalization;
using Coding4Fun.Phone.Controls;

namespace NedWp
{
    public partial class LanguageItemControl : UserControl
    {
        private const string DeleteTag = "DeleteTag";
        private const string DownloadNowTag = "DownloadNowTag";
        private const string AddToQueueTag = "AddToQueueTag";
        private const string ShowLinksTag = "ShowLinksTag";
        private const string ShowDescriptionTag = "ShowDescriptionTag";

        public static readonly DependencyProperty IsDownloadedProperty = DependencyProperty.Register("IsDownloaded", typeof(bool), typeof(LanguageItemControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsDownloadedChangedChanged)));
        public bool IsDownloaded
        {
            get { return (bool)GetValue(IsDownloadedProperty); }
            set
            {
                SetValue(IsDownloadedProperty, value);
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
        }

        private static void OnIsDownloadedChangedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            LanguageItemControl userControl = sender as LanguageItemControl;
            userControl.IsDownloaded = (bool)args.NewValue;
        }
    }

}