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

namespace NedWp
{
    public partial class TitlePanelControl : UserControl
    {
        public string TitleText 
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        public static readonly DependencyProperty TitleTextProperty =
                 DependencyProperty.Register("TitleText", typeof(string), typeof(TitlePanelControl), new PropertyMetadata(String.Empty, TitleText_PropertyChangedCallback));

        public TitlePanelControl()
        {
            InitializeComponent();
            PageTitle.SetBinding(TextBlock.TextProperty, new Binding() { Source = this, Path = new PropertyPath("TitleText") });

        }
        private static void TitleText_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
