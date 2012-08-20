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
using Microsoft.Phone.Tasks;

namespace NedWp
{
    public partial class LinksListPage : PhoneApplicationPage
    {
        private string ContentId { get; set; }
        private MediaItemsListModelItem MediaItemModel { get; set; }

        public LinksListPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs args)
        {
            IDictionary<string, string> parameters = NavigationContext.QueryString;
            Debug.Assert(parameters.ContainsKey("id"));
            ContentId = parameters["id"];

            App.Engine.LibraryModel.LoadLibraryStateIfNotLoaded();
            MediaItemModel = App.Engine.LibraryModel.GetMediaItemForId(ContentId);

            ContentPanel.DataContext = MediaItemModel.ExternalLinks;
            base.OnNavigatedTo(args);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs args)
        {
            App.Engine.LibraryModel.SaveLibraryState();
            base.OnNavigatedFrom(args);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            String url = MediaItemModel.ExternalLinks[LinksList.SelectedIndex]; 
            App.Engine.StatisticsManager.LogOpenLink(new Uri(url));
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(url);
            webBrowserTask.Show();
        }
    }
}