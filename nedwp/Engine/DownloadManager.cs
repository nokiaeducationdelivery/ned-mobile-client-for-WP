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
using Microsoft.Phone.BackgroundTransfer;

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

        public void StartDownload(QueuedDownload download, bool resume = false)
        {
            if (queuedDownloads.Contains(download) || startedDownloads.Keys.Contains(download))
            {
                return;
            }
            if (resume)
            {
                queuedDownloads.Insert(0, download);
            }
            else
            {
                queuedDownloads.Add(download);
                _downloadEnqueuedEvent.OnNext(download);
            }
            
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
                        .Subscribe<Transport.RunningDownload>(
                                activeDownload =>
                                {
                                    if (activeDownload.Download.DownloadSize == long.MaxValue || activeDownload.Download.DownloadSize == 0)
                                    {
                                        activeDownload.Download.DownloadSize = activeDownload.ContentLength;
                                    }
                                    BackgroundWorker worker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

                                    // change cancel handle
                                    startedDownloads[activeDownload.Download] = new ActiveDownloadCancelHandle(worker);

                                    worker.DoWork += (sender, e) =>
                                        {
                                            Transport.RunningDownload dl = (Transport.RunningDownload)e.Argument;
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

                                            if (dl is Transport.ActiveDownload)
                                            {
                                                string filePath = Utils.MediaFilePath(App.Engine.LoggedUser, dl.Download);
                                                using (Stream writer = new IsolatedStorageFileStream(filePath, FileMode.Append, IsolatedStorageFile.GetUserStoreForApplication()))
                                                {
                                                    using (Stream reader = ((Transport.ActiveDownload)dl).Stream)
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
                                            }
                                            if (dl is Transport.BackgroundDownload)
                                            {
                                                BackgroundTransferRequest downloadRequest = ((Transport.BackgroundDownload)dl).Request;
                                                IObservable< IEvent <BackgroundTransferEventArgs> > requestObserver = Observable.FromEvent<BackgroundTransferEventArgs>(downloadRequest, "TransferStatusChanged");
                                                if (downloadRequest.TransferStatus != TransferStatus.Completed)
                                                {
                                                    if (downloadRequest.TransferStatus == TransferStatus.None)
                                                    {
                                                             downloadRequest.DownloadLocation = new Uri(Utils.BackgroundFilePath(App.Engine.LoggedUser, dl.Download), UriKind.RelativeOrAbsolute);
                                                             downloadRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
                                                             e.Result = activeDownload.Download;
                                                             BackgroundTransferService.Add(downloadRequest);
                                                    }
                                                    downloadRequest.TransferProgressChanged += (senderBackground, eventBackground) =>
                                                        {
                                                            if (activeDownload.Download.DownloadSize == long.MaxValue || activeDownload.Download.DownloadSize == 0)
                                                            {
                                                                activeDownload.Download.DownloadSize = 
                                                                    activeDownload.ContentLength == -1 ?
                                                                    0:
                                                                    activeDownload.ContentLength;
                                                            }
                                                            uiUpdates.OnNext(eventBackground.Request.BytesReceived);
                                                        };
                                                    IDisposable cancelOnStop =  DownloadStopPendingEvent.Subscribe( stoppedDownload => 
                                                                        {
                                                                            if (dl.Download == stoppedDownload)
                                                                            {
                                                                                BackgroundTransferService.Remove(downloadRequest);
                                                                                dl.Download.State = QueuedDownload.DownloadState.Stopped;
                                                                                dl.Download.DownloadedBytes = 0;
                                                                            }
                                                                        });
                                                    bw.ReportProgress(0,
                                                             requestObserver
                                                            .First()
                                                            .EventArgs.Request.BytesReceived);
                                                    cancelOnStop.Dispose();
                                                }
                                                e.Result = activeDownload.Download;
                                            }
                                        };
                                    worker.ProgressChanged += (sender, e) =>
                                        {
                                            activeDownload.Download.DownloadedBytes = (long)e.UserState;
                                        };
                                    worker.RunWorkerCompleted += (sender, e) =>
                                        {
                                            if (activeDownload is Transport.ActiveDownload)
                                            {
                                                startedDownloads.Remove(activeDownload.Download);

                                                if (!IsFileDownloadSuccessfulOrResumed(e,activeDownload.Download))
                                                {
                                                    _downloadStoppedEvent.OnNext(activeDownload.Download);
                                                }
                                            }
                                            else
                                            {
                                                if (!e.Cancelled && e.Error == null)
                                                {
                                                    startedDownloads.Remove(activeDownload.Download);
                                                    QueuedDownload result = (QueuedDownload)e.Result;
                                                    if (IsBackgroundTransferSuccesfull(((Transport.BackgroundDownload)activeDownload).Request, result))
                                                    {
                                                        _downloadCompletedEvent.OnNext(activeDownload.Download);
                                                        App.Engine.StatisticsManager.LogDownloadCompleted(activeDownload.Download, "Completed");

                                                    }
                                                }
                                                else if (e.Error != null)
                                                {
                                                    startedDownloads.Remove(download);
                                                    _downloadErrorEvent.OnNext(download);
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        BackgroundTransferService.Remove((activeDownload as Transport.BackgroundDownload).Request);
                                                        _downloadStoppedEvent.OnNext(activeDownload.Download);
                                                        startedDownloads.Remove(download);
                                                    }
                                                    catch (InvalidOperationException)
                                                    {
                                                        startedDownloads.Remove(download);
                                                    }
                                                }
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

        private bool IsFileDownloadSuccessfulOrResumed(RunWorkerCompletedEventArgs e, QueuedDownload queuedDownload)
        {
            if (!e.Cancelled && e.Error == null)
            {
                QueuedDownload result = (QueuedDownload)e.Result;
                if (result.DownloadedBytes == result.DownloadSize)
                {
                    _downloadCompletedEvent.OnNext(queuedDownload);
                    App.Engine.StatisticsManager.LogDownloadCompleted(queuedDownload, "Completed");
                }
                else
                {
                    StartDownload(result, true);
                }
                return true;
            }
            return false;

        }

        private bool IsBackgroundTransferSuccesfull(BackgroundTransferRequest backgroundTransferRequest, QueuedDownload result)
        {
            foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
            {
                if (request.RequestUri.ToString().EndsWith(result.Filename))
                {
                    if (request.TransferStatus == TransferStatus.Completed)
                    {
                        if (request.StatusCode == 200 || request.StatusCode == 206)
                        {
                            if (request.TransferError == null)
                            {
                                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                                {
                                    string filename = Uri.UnescapeDataString(request.DownloadLocation.RemoveTransferPath().ToString());
                                    if (isoStore.FileExists(filename))
                                    {
                                        isoStore.DeleteFile(filename);
                                    }
                                    isoStore.MoveFile(request.DownloadLocation.OriginalString, filename);
                                }
                                BackgroundTransferService.Remove(request);
                                request.Dispose();
                                return true;
                            }
                            else
                            {
                                //move request to standard queue
                                BackgroundTransferService.Remove(request);
                                request.Dispose();
                                result.ForceActiveDownload = true;
                                StartDownload(result);
                                return false;
                            }
                        }
                        else
                        {
                            BackgroundTransferService.Remove(request);
                            request.Dispose();
                            _downloadStoppedEvent.OnNext(result);
                            return false;
                        }
                    }
                    else if (request.TransferStatus == TransferStatus.WaitingForExternalPower
                        || request.TransferStatus == TransferStatus.WaitingForNonVoiceBlockingNetwork
                        || request.TransferStatus == TransferStatus.WaitingForWiFi
                        || request.TransferStatus == TransferStatus.Waiting
                        || request.TransferStatus == TransferStatus.WaitingForExternalPowerDueToBatterySaverMode)
                    {
                        //move request to standard queue
                        BackgroundTransferService.Remove(request);
                        request.Dispose();
                        result.ForceActiveDownload = true;
                        StartDownload(result);
                        return false;
                    }
                }
                request.Dispose();
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
