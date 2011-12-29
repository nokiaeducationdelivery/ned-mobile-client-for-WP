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
using System.Text;

namespace NedEngine
{
    public interface IDownloadEvents
    {
        IObservable<QueuedDownload> DownloadEnqueuedEvent { get; }
        IObservable<QueuedDownload> DownloadStartedEvent { get; }
        IObservable<QueuedDownload> DownloadStopPendingEvent { get; }
        IObservable<QueuedDownload> DownloadStoppedEvent { get; }
        IObservable<QueuedDownload> DownloadErrorEvent { get; }
        IObservable<QueuedDownload> DownloadCompletedEvent { get; }
    }
}
