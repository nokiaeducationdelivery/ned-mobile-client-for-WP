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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using NedEngine;
using System.Windows;
using NedWp.Resources.Languages;

namespace NedWp
{
    public partial class HelpPage : PhoneApplicationPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo( NavigationEventArgs e )
        {
            bool helpLoaded = false;
            IDictionary<string, string> parameters = NavigationContext.QueryString;
            System.Diagnostics.Debug.Assert( parameters.ContainsKey( "type" ) );
            try
            {
                HelpPages helpPage = (HelpPages)Enum.Parse( typeof( HelpPages ), (string)parameters["type"], true );
                helpLoaded = App.Help.FetchHelpData( helpPage );
                DataContext = App.Help;
            }
            catch( Exception ) { }

            if( !helpLoaded )
            {
                System.Diagnostics.Debug.Assert( false, "Fix help data for statistics page" );
                MessageBox.Show( FileLanguage.MISSING_HELP );
            }
            base.OnNavigatedTo( e );
        }
    }
}