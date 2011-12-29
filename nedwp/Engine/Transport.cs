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

namespace NedEngine
{
    public class Transport
    {
        private const String MetadataPath = "NEDCatalogTool2";
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

        public class ActiveDownload
        {
            public ActiveDownload(WebResponse response, QueuedDownload download)
            {
                Response = response;
                Download = download;
            }

            private WebResponse Response { get; set; }
            public QueuedDownload Download { get; private set; }
            public Stream Stream
            {
                get
                {
                    return Response.GetResponseStream();
                }
            }
            public long ContentLength
            {
                get
                {
                    return Response.ContentLength;
                }
            }
        }

        public class PendingDownload
        {
            public PendingDownload(WebRequest request, IObservable<ActiveDownload> response)
            {
                Request = request;
                Response = response;
            }

            public void Cancel()
            {
                Request.Abort();
            }

            private WebRequest Request { get; set; }
            public IObservable<ActiveDownload> Response { get; set; }
        }

        public PendingDownload StartQueuedDownload(QueuedDownload queuedDownload)
        {
            HttpWebRequest request = WebRequest.CreateHttp(ApplicationSettings.ServerUrl.DownloadPath(queuedDownload));
            request.SetCredentials(LoggedUser);
            request.Headers["Range"] = "bytes=" + queuedDownload.DownloadedBytes.ToString() + "-";
            request.AllowReadStreamBuffering = false;

            return new PendingDownload(request,
                    Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, request.SaneEndGetResponse)()
                              .Select(response => new ActiveDownload(response, queuedDownload)));
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
