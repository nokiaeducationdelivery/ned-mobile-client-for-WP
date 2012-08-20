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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NedWp
{
    public static class Constants
    {
        public const String KMotdSetting = "MOTD";
        public const int KSplashScreenDuration = 1000;
        public static TimeSpan KFakeNetworkLatency = new TimeSpan(0, 0, 2);
        public const string KLibraryXmlFilename = "library.xml";
        public const string KLibraryPreviousFilename = "library.bak";
        public const string KLibraryDiffFilename = "diff.xml";
        public const string KPdfExt = ".pdf";
    }
}
