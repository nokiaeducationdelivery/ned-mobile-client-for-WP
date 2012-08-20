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
using System.Xml.Linq;
using System.Linq;
using NedWp;

namespace NedEngine
{
    public class Settings : PropertyNotifierBase
    {
        private bool _automaticStatisticsUpload;
        public bool AutomaticStatisticsUpload
        {
            get
            {
                return _automaticStatisticsUpload;
            }
            set
            {
                if (value != _automaticStatisticsUpload)
                {
                    _automaticStatisticsUpload = value;
                    OnPropertyChanged("AutomaticStatisticsUpload");
                }
            }
        }

        private bool _automaticDownloads;
        public bool AutomaticDownloads
        {
            get
            {
                return _automaticDownloads;
            }
            set
            {
                if (value != _automaticDownloads)
                {
                    _automaticDownloads = value;
                    if (!value && App.Engine.LoggedUser != null )
                    {
                        var queuedDownloads = from download in App.Engine.LoggedUser.Downloads where download.State == QueuedDownload.DownloadState.Queued select download;
                        foreach (QueuedDownload download in queuedDownloads)
                        {
                            download.State = QueuedDownload.DownloadState.Paused;
                            App.Engine.StopDownload(download);
                        }
                    }
                    OnPropertyChanged("AutomaticDownloads");
                }
            }
        }

        public Settings()
        {
            AutomaticStatisticsUpload = true;
            AutomaticDownloads = true;
        }

        public Settings(XElement xElement)
        {
            AutomaticStatisticsUpload = Convert.ToBoolean(xElement.Attribute(Tags.AutoStatUpload).Value);
            AutomaticDownloads = Convert.ToBoolean(xElement.Attribute(Tags.AutoDownload).Value);
        }

        public XElement Data
        {
            get
            {
                return new XElement(Tags.Settings,
                        new XAttribute(Tags.AutoStatUpload, Convert.ToString(AutomaticStatisticsUpload)),
                        new XAttribute(Tags.AutoDownload, Convert.ToString(AutomaticDownloads)));
            }
        }

    }
}
