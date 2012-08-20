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
using System.IO;
using System.Net;
using Microsoft.Phone.Reactive;
using NedWp.Resources.Languages;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;


namespace NedEngine
{
    public class Transport
    {
        private const String MetadataPath = "NEDCatalogTool2";
        private const String LocalizationsPath = "Localizations?action=list";
        private const String LocalizationsDownload = "Localizations?action=do&";
        private const String LoginPath = "LoginServlet";
        private const String MotdPath = "MotdUpdateServlet";
        private const String LibraryPath = "XmlContentServlet";
        private const String StatisticsPath = "UploadStatServlet";

        public static event EventHandler NetworkRequestStarted;
        private static void OnNetworkRequestStarted()
        {
            if (NetworkRequestStarted != null)
                NetworkRequestStarted(null, new EventArgs());
        }

        private readonly ApplicationSettings ApplicationSettings;
        private readonly ILoggedUser LoggedUser;

        public Transport(ILoggedUser LoggedUser, ApplicationSettings ApplicationSettings)
        {
            this.LoggedUser = LoggedUser;
            this.ApplicationSettings = ApplicationSettings;
        }

        public IObservable<Unit> CheckServer(Uri uri)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uri.CombinePath(MetadataPath));
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.NotThrowingEndGetResponse)()
                             .Select(response =>
                             {
                                 HttpStatusCode responseCode = ((HttpWebResponse)response).StatusCode;
                                 if (responseCode == HttpStatusCode.OK || responseCode == HttpStatusCode.Unauthorized)
                                 {
                                     return new Unit();
                                 }
                                 else
                                 {
                                     throw new ArgumentException(AppResources.Error_InvalidServerAddress);
                                 }
                             })
                             .Finally(() => request.Abort());
        }

        public IObservable<Unit> CheckUser(string username, string password)
        {
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, LoginPath));
            request.Credentials = new NetworkCredential(username, password);
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.NotThrowingEndGetResponse)()
                             .Select(response =>
                             {
                                 HttpStatusCode responseCode = ((HttpWebResponse)response).StatusCode;
                                 switch (responseCode) {
                                     case HttpStatusCode.OK: return new Unit();
                                     case HttpStatusCode.Unauthorized: throw new ArgumentException(AppResources.Error_InvalidCredentials);
                                     default: throw new WebException(String.Empty, null, WebExceptionStatus.UnknownError, response);
                                 }
                             })
                             .Finally(() => request.Abort());
        }

        public IObservable<string> GetMotd()
        {
            OnNetworkRequestStarted();
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, MotdPath));
            request.SetCredentials(LoggedUser);
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                             .Select(response => { return response.GetUtf8Content(); })
                             .Finally(() => request.Abort());
        }

        public IObservable<List<LanguageInfo>> GetLanguges()
        {
            OnNetworkRequestStarted();
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, LocalizationsPath));
            request.SetCredentials(LoggedUser);
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                             .Select(response =>
                             {
                                 HttpWebResponse webResponse = (HttpWebResponse) response;
                                 string languagesXml = response.GetUtf8Content();
                                 return ApplicationSettings.AvailableLanguages.parseRemote(languagesXml);
                             })
                             .Finally(() => request.Abort());
        }

        public IObservable<bool> DownloadLocalization(string remoteFileId)
        {
            OnNetworkRequestStarted();
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, LocalizationsDownload + "id=" + remoteFileId + "&type=WP"));
            request.SetCredentials(LoggedUser);
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                             .Select(response =>
                             {
                                 HttpWebResponse webResponse = (HttpWebResponse)response;
                                 string localizationFile = response.GetUtf8Content();
                                 bool requestSuccesfull = false;
                                 try
                                 {
                                     using (IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(Utils.LocalizationsFilePath(remoteFileId), FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication()))
                                     {
                                         using (StreamWriter writeFile = new StreamWriter(isfStream))
                                         {
                                             writeFile.Write(localizationFile);
                                             writeFile.Close();
                                             requestSuccesfull = true;
                                         }
                                     }

                                 }
                                 catch (Exception ex)
                                 {
                                     System.Diagnostics.Debug.WriteLine(String.Format("Failed to save data to file: {0}", "/Localizations/" + remoteFileId + ".xml"));
                                 }
                                 return requestSuccesfull;
                             })
                             .Finally(() => request.Abort());
        }

        public IObservable<Engine.LibraryInfo> GetLibraryInfo(string libraryId)
        {
            OnNetworkRequestStarted();
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, LibraryPath));
            request.SetCredentials(LoggedUser);
            request.Headers["id"] = libraryId;
            request.Headers["nonrecursive"] = "true";
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                             .Select(response =>
                             {
                                 HttpWebResponse webResponse = (HttpWebResponse) response;
                                 if (webResponse.StatusCode == HttpStatusCode.OK && webResponse.Headers["Type"] == "Library")
                                 {
                                     return new Engine.LibraryInfo(libraryId, response.Headers["Title"], Convert.ToInt32(response.Headers["Version"]));
                                 }
                                 else
                                 {
                                     throw new ArgumentException(AppResources.Error_LibraryDoesNotExist);
                                 }
                             })
                             .Finally(() => request.Abort());
        }



        public IObservable<int> UploadStaticstics(IEnumerable<StatisticItem> statistics)
        {
            HttpWebRequest webRequest = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, StatisticsPath));
            webRequest.SetCredentials(LoggedUser);
            webRequest.Headers["Username"] = LoggedUser.LoggedUser.Username;
            webRequest.Headers["DeviceId"] = Utils.GetDeviceId();
            webRequest.Method = "POST";

            return (from request in Observable.Return(webRequest)
                    from requestStream in request.GetRequestStreamObservable()
                    from response in request.PostStatisticsAndGetResponseObservable(statistics, requestStream)
                    select ((HttpWebResponse)response).GetResponseStream().ReadByte());
        }

        public class LibraryUpdate
        {
            public LibraryUpdate(string contents, int version)
            {
                Contents = contents;
                Version = version;
            }

            public string Contents { get; private set; }
            public int Version { get; private set; }
        }

        public IObservable<LibraryUpdate> GetLibraryXml(string libraryId)
        {
            OnNetworkRequestStarted();
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.CombinePath(MetadataPath, LibraryPath));
            request.SetCredentials(LoggedUser);
            request.Headers["id"] = libraryId;

            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                             .Select(response => 
                                 {
                                     HttpWebResponse webResponse = (HttpWebResponse)response;
                                     if (webResponse.StatusCode == HttpStatusCode.OK)
                                     {
                                         return new LibraryUpdate(response.GetUtf8Content(), Convert.ToInt32(webResponse.Headers["Version"]));
                                     }
                                     else
                                     {
                                         throw new ArgumentException(AppResources.Error_LibraryDeletedFromServer);
                                     }
                                 })
                             .Finally(() => request.Abort());
        }

        public abstract class RunningDownload
        {
            public RunningDownload(QueuedDownload download)
            {
                Download = download;
            }

            public QueuedDownload Download { get; protected set; }
            public abstract long ContentLength { get; }
        }

        public class ActiveDownload : RunningDownload
        {
            public ActiveDownload(WebResponse response, QueuedDownload download)
                : base(download)
            {
                Response = response;
            }

            private WebResponse Response { get; set; }
            public Stream Stream
            {
                get
                {
                    return Response.GetResponseStream();
                }
            }
            public override long ContentLength
            {
                get
                {
                    return Response.ContentLength;
                }
            }
        }

        public class BackgroundDownload : RunningDownload
        {
            public BackgroundDownload(BackgroundTransferRequest request, QueuedDownload download)
                : base(download)
            {
                Request = request;
            }

            public BackgroundTransferRequest Request {get; set;}

            public override long ContentLength
            {
                get
                {
                    return Request.TotalBytesToReceive;
                }
            }

        }

        public abstract class PendingDownload
        {
            public PendingDownload(IObservable<RunningDownload> response)
            {
                Response = response;
            }

            public IObservable<RunningDownload> Response { get; set; }

            public abstract void Cancel();
        }

        public class PendingActiveDownload : PendingDownload
        {
            public PendingActiveDownload(WebRequest request, IObservable<RunningDownload> response) : base(response)
            {
                Request = request;
                Response = response;
            }

            public override void Cancel()
            {
                Request.Abort();
            }

            private WebRequest Request { get; set; }
        }

        public class PendingBackgroundDownload : PendingDownload
        {
            public PendingBackgroundDownload(BackgroundTransferRequest request, IObservable<RunningDownload> response)
                : base(response)
            {
                Request = request;
            }

            private BackgroundTransferRequest Request { get; set; }

            public override void Cancel()
            {
                try
                {
                    BackgroundTransferService.Remove(Request);
                    Request.Dispose();
                }
                catch (InvalidOperationException)
                {
                    // request is alreadty completed/ cancelled
                }
                
            }
        }

        public PendingDownload StartQueuedDownload(QueuedDownload queuedDownload)
        {
            return chooseDownloadMethod(queuedDownload);
        }

        private PendingDownload chooseDownloadMethod(QueuedDownload queuedDownload)
        {
            if(queuedDownload.ForceActiveDownload || queuedDownload.DownloadSize >= 20000000L && queuedDownload.DownloadSize != long.MaxValue)
            {
                HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.DownloadPath(queuedDownload));
                request.SetCredentials(LoggedUser);
                request.Headers["Range"] = "bytes=" + queuedDownload.DownloadedBytes.ToString() + "-";
                request.AllowReadStreamBuffering = false;
                return new PendingActiveDownload(request,
                  Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                 .Select(response => (RunningDownload)new ActiveDownload(response, queuedDownload)));
            }
            else
            {
                //first check if there is not tranfer request already added to service
                foreach( BackgroundTransferRequest request in BackgroundTransferService.Requests)
                {
                    if(request.RequestUri.Equals(ApplicationSettings.ServerUrl.DownloadPath(queuedDownload)))
                    {
                        return new PendingBackgroundDownload(request, Observable.Return( new BackgroundDownload(request, queuedDownload) as RunningDownload));
                    }
                    request.Dispose();
                }
                BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(ApplicationSettings.ServerUrl.DownloadPath(queuedDownload));
                return new PendingBackgroundDownload(transferRequest, Observable.Return(new BackgroundDownload(transferRequest, queuedDownload) as RunningDownload));
            }
        }


    }

    public static class TransportExtensions
    {
        private const String MediaPathPart2 = "nokiaecd";
        private const String MediaPathPart3 = "videos";

        public static Uri DownloadPath(this Uri serverUrl, QueuedDownload download)
        {
            return serverUrl.CombinePath(download.LibraryId, MediaPathPart2, MediaPathPart3, download.Filename);
        }

        public static void SetCredentials(this WebRequest request, ILoggedUser user)
        {
            request.Credentials = new NetworkCredential(user.LoggedUser.Username, user.LoggedUser.Password);
        }

        public static IObservable<WebResponse> GetResponseObservable(this WebRequest request)
        {
            return Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)();
        }

        public static IObservable<Stream> GetRequestStreamObservable(this WebRequest request)
        {
            return Observable.FromAsyncPattern<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream)();
        }

        public static IObservable<WebResponse> PostStatisticsAndGetResponseObservable(this WebRequest request, IEnumerable<StatisticItem> statistics, Stream stream)
        {
            using (stream)
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    foreach (StatisticItem item in statistics)
                    {
                        string data = item.GetServerFormatedString();
                        writer.WriteLine(data.ToCharArray(), 0, data.Length);
                    }
                }
            }
            return request.GetResponseObservable();
        }
    }
}
