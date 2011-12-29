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
using System.Diagnostics;
using NedEngine;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;

namespace NedWp
{
    public partial class DownloadListItemControl : UserControl, IDataContextChangedHandler<DownloadListItemControl> 
    {
        private const String KIndeterminateDownloadSize = "? / ?";

        public static readonly DependencyProperty DownloadedProgressTextProperty = DependencyProperty.RegisterAttached("DownloadedProgressText", typeof(string), typeof(DownloadListItemControl), new PropertyMetadata(KIndeterminateDownloadSize));
        public string DownloadedProgressText
        {
            get { return (string)GetValue(DownloadedProgressTextProperty); }
            set { SetValue(DownloadedProgressTextProperty, value); }
        }

        public DownloadListItemControl()
        {
            InitializeComponent();
            DataContextChangedHelper<DownloadListItemControl>.Bind(this); 
        }

        // In sliverligh there is no readily available change event for DataContext.
        // To create it I had to write an interface and a static class. 
        // Interface contains one method with is implemented below, this 
        // method is called be the control itself. To made this, I had to 
        // register my control in static class 'DataContextChangedHelper'(in constructor)
        // with call method when DataContext will be changed. 
        public void DataContextChanged(DownloadListItemControl sender, DependencyPropertyChangedEventArgs e)
        {
            QueuedDownload model = (DataContext as QueuedDownload);
            model.PropertyChanged += OnDownloadedProgressTextChanged;
            SetDownloadedProgressText(model);
        }

        private void OnDownloadMenuActivated(object sender, RoutedEventArgs args)
        {
            MenuItem menuItem = sender as MenuItem;
            Debug.Assert(menuItem != null);
            QueuedDownload downloadItem = menuItem.CommandParameter as QueuedDownload;
            Debug.Assert(downloadItem != null);

            switch (menuItem.Tag.ToString())
            {
                case "Cancel":
                    App.Engine.CancelDownload(downloadItem);
                    break;
            }
        }

        private void SetDownloadedProgressText(QueuedDownload model)
        {
            if (model.DownloadSize == long.MaxValue)
            {
                DownloadedProgressText = KIndeterminateDownloadSize;
            }
            else
            {
                DownloadedProgressText = String.Format("{0} / {1} kB", model.DownloadedBytes / 1024, model.DownloadSize / 1024);
            }
        }

        private void OnDownloadedProgressTextChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "DownloadedBytes" || args.PropertyName == "DownloadSize")
            {
                SetDownloadedProgressText(DataContext as QueuedDownload);
            }
        }
    }

    // Solution to handle DataContextChanged event.
    #region DataContextChangedSolution

    public interface IDataContextChangedHandler<T> where T : FrameworkElement
    {
        void DataContextChanged(T sender, DependencyPropertyChangedEventArgs e);
    }


    public static class DataContextChangedHelper<T> where T : FrameworkElement, IDataContextChangedHandler<T>
    {
        private const string INTERNAL_CONTEXT = "InternalDataContext";

        public static readonly DependencyProperty InternalDataContextProperty =
            DependencyProperty.Register(INTERNAL_CONTEXT,
                                        typeof(Object),
                                        typeof(T),
                                        new PropertyMetadata(_DataContextChanged));

        private static void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            T control = (T)sender;
            control.DataContextChanged(control, e);
        }

        public static void Bind(T control)
        {
            control.SetBinding(InternalDataContextProperty, new Binding());
        }
    }
    #endregion DataContextChangedSolution
}
