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
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Phone.Info;
using System.Collections.ObjectModel;
using Microsoft.Phone.Reactive;
using Microsoft.Phone.Controls;

namespace NedEngine
{
    public static class Utils
    {
        public static void RecursivelyDeleteDirectory(this IsolatedStorageFile directory, string directoryName)
        {
            string directorySearchPath = Path.Combine(directoryName, "*");

            foreach (var file in directory.GetFileNames(directorySearchPath))
            {
                directory.DeleteFile(Path.Combine(directoryName, file));
            }

            foreach (var subdir in directory.GetDirectoryNames(directorySearchPath))
            {
                directory.RecursivelyDeleteDirectory(Path.Combine(directoryName, subdir));
            }

            directory.DeleteDirectory(directoryName);
        }

        public static string GetMediaIcon(MediaItemType type)
        {
            string mediaIconPath = String.Empty;
            switch (type)
            {
                case MediaItemType.Undefined:
                    mediaIconPath = "../Resources/MediaItemIcons/small_unknown_data_icon.png";
                    break;
                case MediaItemType.Text:
                    mediaIconPath = "../Resources/MediaItemIcons/small_text_icon.png";
                    break;
                case MediaItemType.Picture:
                    mediaIconPath = "../Resources/MediaItemIcons/small_photo_icon.png";
                    break;
                case MediaItemType.Audio:
                    mediaIconPath = "../Resources/MediaItemIcons/small_music_icon.png";
                    break;
                case MediaItemType.Video:
                    mediaIconPath = "../Resources/MediaItemIcons/small_video_icon.png";
                    break;
                default:
                    Debug.Assert(false, "Unknown media type");
                    break;
            }
            return mediaIconPath;
        }

        public static string LibraryDirPath(User user, Library library)
        {
            return Path.Combine(user.LocalId.ToString(), library.LocalId.ToString());
        }

        public static string LibraryXmlPath(User user, Library library)
        {
            return Path.Combine(LibraryDirPath(user, library), NedWp.Constants.KLibraryXmlFilename);
        }

        public static string LibraryXmlPreviousPath(User user, Library library)
        {
            return Path.Combine(LibraryDirPath(user, library), NedWp.Constants.KLibraryPreviousFilename);
        }

        public static string LibraryXmlDiffPath(User user, Library library)
        {
            return Path.Combine(LibraryDirPath(user, library), NedWp.Constants.KLibraryDiffFilename);
        }

        private static Library FindLibraryById(User user, string libraryId)
        {
            return (from lib in user.Libraries where lib.ServerId == libraryId select lib).First();
        }

        private static string MediaFilePath(User user, string libraryid, string localFilename)
        {
            return Path.Combine(LibraryDirPath(user, FindLibraryById(user, libraryid)), localFilename);
        }

        public static string MediaFilePath(User user, QueuedDownload file)
        {
            return MediaFilePath(user, file.LibraryId, file.LocalFilename);
        }

        public static string LocalizationsFilePath(string langId)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/Localizations"))
                {
                    isoStore.CreateDirectory("/Localizations");
                }
            }
            return Path.Combine("/Localizations", langId + ".xml");
        }

        public static string BackgroundFilePath(User user, QueuedDownload queuedDownload)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
            string basicPath = MediaFilePath(user, queuedDownload);
            return Path.Combine("/shared/transfers/", basicPath);
        }

        public static string MediaFilePath(User user, MediaItemsListModelItem item)
        {
            return MediaFilePath(user, item.LibraryId, FilenameToLocalFilename(item.FileName));
        }

        public static string FilenameToLocalFilename( string filename )
        {
            if (filename.EndsWith(".txt"))
            {
                return filename.Replace(".txt", ".html");
            }
            else
            {
                return filename;
            }
        }

        public static ObservableCollection<T> Remove<T>(this ObservableCollection<T> coll, IEnumerable<T> itemsToRemove)
        {
            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }
            return coll;
        }

        // 404 NotFound is returned in two cases: when file is missing, i.e.
        // when server actually responds with 404 not found; or when the
        // network connection goes down. These two cases can be discerned
        // only by checking the WebResponse object - in case of network
        // failure the fields like header or remote URI will contain null
        // or empty values.
        public static WebResponse SaneEndGetResponse(this WebRequest request, IAsyncResult asyncResult)
        {
            try
            {
                return request.EndGetResponse(asyncResult);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound)
                if (String.IsNullOrEmpty(wex.Response.ResponseUri.ToString()))
                {
                    throw new WebException("Network error", WebExceptionStatus.ConnectFailure);
                }
                throw;
            }
        }

        public static WebResponse NotThrowingEndGetResponse(this WebRequest request, IAsyncResult asyncResult)
        {
            try
            {
                return request.SaneEndGetResponse(asyncResult);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    return wex.Response;
                }
                throw;
            }
        }

        public static string GetUtf8Content(this WebResponse response)
        {
            if (response.Headers.AllKeys.Contains("Content-Length"))

            {
                byte[] buffer = new byte[response.ContentLength];
                int read = response.GetResponseStream().Read(buffer, 0, buffer.Length);
                System.Diagnostics.Debug.Assert(read == response.ContentLength);
                return new System.Text.UTF8Encoding().GetString(buffer, 0, buffer.Length);
            }
            else
            {
                MemoryStream content = new MemoryStream();
                using (BinaryWriter writer = new BinaryWriter(content))
                {
                    using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                    {
                        byte[] buffer = new byte[1024];
                        int readCount;
                        while ((readCount = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.Write(buffer, 0, readCount);
                        }
                    }
                }
                byte[] contentBuffer = content.GetBuffer();
                string result = new System.Text.UTF8Encoding().GetString(contentBuffer, 0, contentBuffer.Length).TrimEnd(new char[] {'\0'});
                return result;
            }
        }

        public static Uri CombinePath(this Uri uri, params string[] uriParts)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string part in uriParts)
            {
                builder.Append(part.Trim('/'));
                builder.Append("/");
            }
            return new Uri(uri, builder.ToString().Trim('/'));
        }

        public static Uri RemoveTransferPath(this Uri uri)
        {
            StringBuilder builder = new StringBuilder(uri.OriginalString);
            builder.Remove(0, "\\shared\\transfers".Length);
            return new Uri(builder.ToString().Replace('\\', '/').Trim('/'), UriKind.RelativeOrAbsolute);
        }

        public static T PopFirst<T>(this ICollection<T> collection)
        {
            T result = collection.First();
            collection.Remove(result);
            return result;
        }

        // This is NOT IMEI, there is no way of getting IMEI on WP7
        private const int KDatabaseDeviceIdMaxLength = 20;
        private const string KWinPhone7Prefix = "WP7:";

        public static string GetDeviceId()
        {
            string result = string.Empty;
            object deviceName;
            if (DeviceExtendedProperties.TryGetValue("DeviceUniqueId", out deviceName))
            {
                string temp = Convert.ToBase64String((byte[])deviceName);
                // NOTE: device id length has to be limited due to maxmimum field length in database
                if (temp.Length + KWinPhone7Prefix.Length > KDatabaseDeviceIdMaxLength)
                    temp = temp.Substring(temp.Length + KWinPhone7Prefix.Length - KDatabaseDeviceIdMaxLength);
                result = KWinPhone7Prefix + temp;
            }
            return result;
        }

        public static IObservable<Unit> FinishedToNext<T>(this IObservable<T> observable)
        {
            return Observable.CreateWithDisposable<Unit>(
                o =>
                {
                    return observable.Subscribe<T>(
                        result => { },
                        error => o.OnError(error),
                        () => o.OnNext(new Unit()));
                }
            ).Take(1);
        }

        public static void NavigateTo(string url)
        {
            var frame = NedWp.App.Current.RootVisual as PhoneApplicationFrame;
            if ((frame == null) || (url == frame.CurrentSource.ToString()))
                return;
            frame.Navigate(new Uri(url, UriKind.Relative));
        }
    }
}
