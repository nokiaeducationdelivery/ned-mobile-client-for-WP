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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Phone.Reactive;
using NedWp;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;

namespace NedEngine
{
    public class DownloadManager : IDownloadEvents
    {
        private interface IDownloadCancelHandle
        {
            void Cancel();
        }

        private class PendingDownloadCancelHandle : IDownloadCancelHandle
        {
            private readonly Transport.PendingDownload download;

            public PendingDownloadCancelHandle(Transport.PendingDownload download)
            {
                this.download = download;
            }

            public void Cancel()
            {
                download.Cancel();
            }
        }

        private class ActiveDownloadCancelHandle : IDownloadCancelHandle
        {
            private readonly BackgroundWorker downloadWorker;

            public ActiveDownloadCancelHandle(BackgroundWorker downloadWorker)
            {
                this.downloadWorker = downloadWorker;
            }

            public void Cancel()
            {
                downloadWorker.CancelAsync();
            }
        }

        private readonly IList<QueuedDownload> queuedDownloads = new List<QueuedDownload>();
        private readonly IDictionary<QueuedDownload, IDownloadCancelHandle> startedDownloads = new Dictionary<QueuedDownload, IDownloadCancelHandle>();

        private const int KMaxSimultaneousDownloads = 2;
        private const int KProgressUpdateInterval = 250;

        private readonly Transport Transport;

        public DownloadManager(Transport transport)
        {
            this.Transport = transport;

            DownloadDoneEvent.Subscribe(_ => StartEnqueuedItemsIfPossible());
        }

        #region Events

        private Subject<QueuedDownload> _downloadEnqueuedEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadEnqueuedEvent { get { return _downloadEnqueuedEvent; } }

        private Subject<QueuedDownload> _downloadStartedEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadStartedEvent { get { return _downloadStartedEvent; } }

        private Subject<QueuedDownload> _downloadStopPendingEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadStopPendingEvent { get { return _downloadStopPendingEvent; } }

        private Subject<QueuedDownload> _downloadStoppedEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadStoppedEvent { get { return _downloadStoppedEvent; } }

        private Subject<QueuedDownload> _downloadErrorEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadErrorEvent { get { return _downloadErrorEvent; } }

        private Subject<QueuedDownload> _downloadCompletedEvent = new Subject<QueuedDownload>();
        public IObservable<QueuedDownload> DownloadCompletedEvent { get { return _downloadCompletedEvent; } }

        private IObservable<QueuedDownload> DownloadDoneEvent { get { return DownloadStoppedEvent.Merge(DownloadErrorEvent).Merge(DownloadCompletedEvent); } }

        #endregion

        public IObservable<QueuedDownload> CancelAllDownloads()
        {
            return CancelDownloads(queuedDownloads.Concat(startedDownloads.Keys).ToList());
        }

        public IObservable<QueuedDownload> CancelDownloads(IEnumerable<QueuedDownload> downloads)
        {
            foreach (QueuedDownload download in downloads)
            {
                queuedDownloads.Remove(download);
            }

            IObservable<QueuedDownload> allDownloadsCancelled = Observable.Empty<QueuedDownload>();
            foreach (var startedDownload in startedDownloads.Keys.Intersect(downloads))
            {
                allDownloadsCancelled = allDownloadsCancelled.Merge(StopDownload(startedDownload));
            }
            return allDownloadsCancelled;
        }

        public void InitializeQueue()
        {
            foreach (var download in App.Engine.LoggedUser.Downloads.Where(dl => dl.State != QueuedDownload.DownloadState.Paused))
            {
                StartDownload(download);
            }
        }

        private void StartEnqueuedItemsIfPossible()
        {
            while (startedDownloads.Count < KMaxSimultaneousDownloads && queuedDownloads.Count > 0)
            {
                QueuedDownload download = queuedDownloads.PopFirst();
                IDownloadCancelHandle cancelHandle = StartEnqueuedDownload(download);
                startedDownloads.Add(download, cancelHandle);
            }
        }

        public void StartDownload(QueuedDownload download)
        {
            if (queuedDownloads.Contains(download) || startedDownloads.Keys.Contains(download))
            {
                return;
            }

            queuedDownloads.Add(download);
            _downloadEnqueuedEvent.OnNext(download);

            StartEnqueuedItemsIfPossible();
        }

        public IObservable<QueuedDownload> StopDownload(QueuedDownload download)
        {
            _downloadStopPendingEvent.OnNext(download);

            if (!queuedDownloads.Contains(download) && !startedDownloads.Keys.Contains(download))
            {
                return Observable.Return<QueuedDownload>(download);
            }

            IObservable<QueuedDownload> result = DownloadDoneEvent.Where(dl => dl == download).Take(1);

            if (queuedDownloads.Remove(download))
            {
                _downloadStoppedEvent.OnNext(download);
            }
            else
            {
                // item was not waiting in queue, so it's already downloading
                startedDownloads[download].Cancel();
                result.Subscribe(dl =>
                    {
                        startedDownloads.Remove(dl);
                        StartEnqueuedItemsIfPossible();
                    });
            }

            return result;
        }

        private IDownloadCancelHandle StartEnqueuedDownload(QueuedDownload download)
        {
            _downloadStartedEvent.OnNext(download);
            App.Engine.StatisticsManager.LogDownloadStart(download);

            Transport.PendingDownload pendingDownload = Transport.StartQueuedDownload(download);

            pendingDownload.Response
                        .ObserveOnDispatcher()
                        .Subscribe<Transport.ActiveDownload>(
                                activeDownload =>
                                {
                                    if (activeDownload.Download.DownloadSize == long.MaxValue)
                                    {
                                        activeDownload.Download.DownloadSize = activeDownload.ContentLength;
                                    }
                                    BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

                                    // change cancel handle
                                    startedDownloads[activeDownload.Download] = new ActiveDownloadCancelHandle(worker);

                                    worker.DoWork += (sender, e) =>
                                        {
                                            Transport.ActiveDownload dl = (Transport.ActiveDownload)e.Argument;
                                            BackgroundWorker bw = (BackgroundWorker)sender;
                                            long bytesRead = dl.Download.DownloadedBytes;

                                            // limited number of progress bar updates
                                            var uiUpdates = new Subject<long>();
                                            var cancelUiUpdates = uiUpdates
                                                .Take(1)
                                                .Merge(Observable.Empty<long>().Delay(TimeSpan.FromMilliseconds(KProgressUpdateInterval)))
                                                .Repeat()
                                                .Subscribe<long>(progress =>
                                                {
                                                    if (bw.IsBusy)
                                                    {
                                                        bw.ReportProgress(0, progress);
                                                    }
                                                });

                                            string filePath = Utils.MediaFilePath(App.Engine.LoggedUser, dl.Download);
                                            using (Stream writer = new IsolatedStorageFileStream(filePath, FileMode.Append, IsolatedStorageFile.GetUserStoreForApplication()))
                                            {
                                                using (Stream reader = dl.Stream)
                                                {
                                                    byte[] buffer = new byte[16 * 1024];
                                                    int readCount;
                                                    while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0)
                                                    {
                                                        bytesRead += readCount;
                                                        writer.Write(buffer, 0, readCount);
                                                        uiUpdates.OnNext(bytesRead);

                                                        if (bw.CancellationPending)
                                                        {
                                                            pendingDownload.Cancel();
                                                            e.Cancel = true;
                                                            break;
                                                        }
                                                    }
                                                    bw.ReportProgress(0, bytesRead);
                                                    e.Result = activeDownload.Download;
                                                }
                                            }
                                            cancelUiUpdates.Dispose();
                                        };
                                    worker.ProgressChanged += (sender, e) =>
                                        {
                                            activeDownload.Download.DownloadedBytes = (long)e.UserState;
                                        };
                                    worker.RunWorkerCompleted += (sender, e) =>
                                        {
                                            startedDownloads.Remove(activeDownload.Download);

                                            if (IsFileDownloadSuccessful(e))
                                            {
                                                _downloadCompletedEvent.OnNext(activeDownload.Download);
                                                App.Engine.StatisticsManager.LogDownloadCompleted(activeDownload.Download, "Completed" );
                                                
                                            }
                                            else
                                            {
                                                _downloadStoppedEvent.OnNext(activeDownload.Download);
                                            }
                                        };
                                    worker.RunWorkerAsync(activeDownload);
                                },
                                error =>
                                {
                                    startedDownloads.Remove(download);

                                    if (IsFileMissingFromServer(error))
                                    {
                                        _downloadErrorEvent.OnNext(download);
                                        App.Engine.StatisticsManager.LogDownloadCompleted(download, "Error");
                                    }
                                    else
                                    {
                                        _downloadStoppedEvent.OnNext(download);
                                    }
                                }
                            );
            return new PendingDownloadCancelHandle(pendingDownload);
        }

        // When network fails during file download loop no exception is
        // thrown, the Stream.Read method returns 0 just like in case of
        // successfully finished download. The only way to handle error
        // is to check if the file was completely downloaded.
        //
        // The cancellation and DoWork exception cases are also checked for
        // in this method.
        //
        // This method assumes that e.Result contains QueuedDownload object
        // if the worker wasn't cancelled and no exception was thrown in
        // DoWork.
        private bool IsFileDownloadSuccessful(RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                QueuedDownload result = (QueuedDownload)e.Result;
                if (result.DownloadedBytes == result.DownloadSize)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsFileMissingFromServer(Exception error)
        {
            if (error is WebException)
            {
                WebException wex = error as WebException;
                return (wex.Response != null && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound);
            }
            return false;
        }
    }
}
