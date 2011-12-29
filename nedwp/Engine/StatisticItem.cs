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
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace NedEngine
{

    public enum StatisticType
    {
        BROWSE_LIBRARY_OPEN, // Library to open id as 'Id'
        BROWSE_LIBRARY_BACK, // Not used
        BROWSE_CATALOG_OPEN, // Selected (navigated to) item id as 'Id'
        BROWSE_CATALOG_BACK, // Current catalogue id as 'Id'
        BROWSE_CATEGORY_OPEN, // Selected (navigated to) item id as 'Id'
        BROWSE_CATEGORY_BACK, // Current item parent id as 'Id'
        BROWSE_MEDIAITEM_BACK, // Current item parent id as 'Id'
        DOWNLOAD_ADD,
        DOWNLOAD_REMOVE,
        DOWNLOAD_START,
        DOWNLOAD_END,
        DOWNLOAD_COMPLETED,
        LIBRARY_ADD,
        LIBRARY_REMOVED,
        PLAY_ITEM_START,
        PLAY_ITEM_END,
        LINK_OPEN,
        DETAILS_SHOW,
        SHOW_LINKS,
        DELETE_ITEM,
        SEARCH_ITEM,
        USER_LOGGED,
        USER_DELETE,
        APP_EXIT,
        UNKNOWN
    }

    public abstract class StatisticItem
    {
        public StatisticType Type { get; protected set; }
        public string Username { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public Dictionary<string, string> Details { get; protected set; }
        public Guid UpdateId { get; set; }
        
        protected StatisticItem(StatisticType type, string username)
        {
            Type = type;
            Username = username;
            Timestamp = DateTime.UtcNow;
            Details = null;
            UpdateId = Guid.Empty;
        }

        protected StatisticItem(XElement xElement)
        {
            Type = (StatisticType)Enum.Parse(typeof(StatisticType), xElement.Attribute(Tags.StatisticType).Value, true);
            Username = xElement.Attribute(Tags.Username).Value;
            Timestamp = DateTime.Parse(xElement.Attribute(Tags.Timestamp).Value);
            Details = new Dictionary<string, string>(xElement.Elements().ToDictionary(xDetail => xDetail.Name.LocalName, xDetail => xDetail.Value));
            UpdateId = Guid.Empty;
        }

        public virtual string GetServerFormatedString()
        {
            var paramItemsStrings = Details.Select(x => String.Format("{0}={1}", x.Key, x.Value));
            return string.Format("{0},{1},{2}", Type.ToString(), String.Join(";", paramItemsStrings.ToArray()), Timestamp.ToString("O"));
        }

        public XElement GetData()
        {
            return new XElement(Tags.Statistic,
                        new XAttribute(Tags.StatisticType, Type.ToString()),
                        new XAttribute(Tags.Username, Username.ToString()),
                        new XAttribute(Tags.Timestamp, Timestamp.ToString()),
                        GetDetails());
        }

        private XElement[] GetDetails()
        {
            return (from detail in Details select new XElement(detail.Key, detail.Value)).ToArray();
        }
    }

    public class MediaItemStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string MediaId = "Id";
            public const string MediaTitle = "Title";
            public const string MediaType = "Type";
        }

        private MediaItemStatisticItem(StatisticType type, string username, MediaItemsListModelItem mediaItem)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.MediaId, mediaItem.Id },
                { DetailKeys.MediaTitle, mediaItem.Title },
                { DetailKeys.MediaType, mediaItem.ItemType.ToString() },
            };
        }
        
        public MediaItemStatisticItem(XElement xElement) : base(xElement) { }

        public static MediaItemStatisticItem CreateMediaPlayItemStartEvent(User user, MediaItemsListModelItem startedMediaItem)
        {
            return new MediaItemStatisticItem(StatisticType.PLAY_ITEM_START, user.Username, startedMediaItem);
        }

        public static MediaItemStatisticItem CreateMediaStopItemStartEvent(User user, MediaItemsListModelItem stoppedMediaItem)
        {
            return new MediaItemStatisticItem(StatisticType.PLAY_ITEM_END, user.Username, stoppedMediaItem);
        }

        public static MediaItemStatisticItem CreateMediaShowDetailsEvent(User user, MediaItemsListModelItem mediaItem)
        {
            return new MediaItemStatisticItem(StatisticType.DETAILS_SHOW, user.Username, mediaItem);
        }

        public static MediaItemStatisticItem CreateMediaShowLinksEvent(User user, MediaItemsListModelItem mediaItem)
        {
            return new MediaItemStatisticItem(StatisticType.SHOW_LINKS, user.Username, mediaItem);
        }
    }

    public class NavigationStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string ItemId = "Id";
        }

        private NavigationStatisticItem(StatisticType type, string username, string itemId)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.ItemId, itemId },
            };
        }

        public NavigationStatisticItem(XElement xElement)
            : base(xElement)
        {
        }

        public static NavigationStatisticItem CreateLibraryOpenEvent(User user, string libraryId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_LIBRARY_OPEN, user.Username, libraryId);
        }

        public static NavigationStatisticItem CreateCatalogueOpenEvent(User user, string catalogueId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_CATALOG_OPEN, user.Username, catalogueId);
        }

        public static NavigationStatisticItem CreateCategoryOpenEvent(User user, string categoryId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_CATEGORY_OPEN, user.Username, categoryId);
        }
        
        public static NavigationStatisticItem CreateMediaDeleteItemEvent(User user, String mediaId)
        {
            return new NavigationStatisticItem(StatisticType.DELETE_ITEM, user.Username, mediaId);
        }

        public static NavigationStatisticItem CreateMediaItemBackEvent(User user, string mediaId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_MEDIAITEM_BACK, user.Username, mediaId);
        }

        public static NavigationStatisticItem CreateCategoryBackEvent(User user, string categoryId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_CATEGORY_BACK, user.Username, categoryId);
        }

        public static NavigationStatisticItem CreateCatalogueBackEvent(User user, string catalogId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_CATALOG_BACK, user.Username, catalogId);
        }

        public static NavigationStatisticItem CreateLibraryBackEvent(User user, string libraryId)
        {
            return new NavigationStatisticItem(StatisticType.BROWSE_LIBRARY_BACK, user.Username, libraryId);
        }
    }

    public class AuthenticationStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string Username= "Username";
        }
        private AuthenticationStatisticItem(StatisticType type, string username, string username2)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.Username, username2 },
            };
        }

        public AuthenticationStatisticItem(XElement xElement)
            : base(xElement)
        {
        }

        public static AuthenticationStatisticItem CreateUserLoggedEvent(User user)
        {
            return new AuthenticationStatisticItem(StatisticType.USER_LOGGED, user.Username, user.Username);
        }

        public static AuthenticationStatisticItem CreateUserDeleteEvent(User user)
        {
            return new AuthenticationStatisticItem(StatisticType.USER_DELETE, user.Username, user.Username);
        }

        public static AuthenticationStatisticItem CreateAppExitEvent(User user)
        {
            return new AuthenticationStatisticItem(StatisticType.APP_EXIT, user.Username , String.Empty);
        }

        public override string GetServerFormatedString()
        {
            var paramItemsStrings = Details.Select(x => String.Format("{0}", x.Value));
            return string.Format("{0},{1},{2}", Type.ToString(), String.Join(";", paramItemsStrings.ToArray()), Timestamp.ToString("O"));
        }
    }

    public class DownloadStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string Url = "Url";
            public const string Progress = "Progress";
            public const string Status = "Status";
        }

        private DownloadStatisticItem(StatisticType type, string username, string url, string progress)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.Url, url },
                { DetailKeys.Progress , progress}
            };
        }

        private DownloadStatisticItem(StatisticType type, string username, string url, string progress, string status)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.Url, url },
                { DetailKeys.Progress , progress},
                { DetailKeys.Status , status}
            };
        }

        public DownloadStatisticItem(XElement xElement)
            : base(xElement)
        {
        }

        private static string GetDownloadPercentProgress(QueuedDownload download)
        {
            double percent = ((double)download.DownloadedBytes / (double)download.DownloadSize )* 100.0;
            return percent.ToString();
        }

        public static DownloadStatisticItem CreateDownloadAddEven(User user, Uri url, QueuedDownload download)
        {

            return new DownloadStatisticItem(StatisticType.DOWNLOAD_ADD, user.Username, url.ToString(), GetDownloadPercentProgress(download));
        }

        public static DownloadStatisticItem CreateDownloadRemoveEven(User user, Uri url, QueuedDownload download)
        {
            return new DownloadStatisticItem(StatisticType.DOWNLOAD_REMOVE, user.Username, url.ToString(), GetDownloadPercentProgress(download));
        }

        public static DownloadStatisticItem CreateDownloadStartEven(User user, Uri url, QueuedDownload download)
        {
            return new DownloadStatisticItem(StatisticType.DOWNLOAD_START, user.Username, url.ToString(), GetDownloadPercentProgress(download));
        }

        public static DownloadStatisticItem CreateDownloadEndEven(User user, Uri url, QueuedDownload download)
        {
            return new DownloadStatisticItem(StatisticType.DOWNLOAD_END, user.Username, url.ToString(), GetDownloadPercentProgress(download));
        }

        public static DownloadStatisticItem CreateDownloadCompletedEven(User user, Uri url, QueuedDownload download, string status)
        {
            return new DownloadStatisticItem(StatisticType.DOWNLOAD_COMPLETED, user.Username, url.ToString(), GetDownloadPercentProgress(download), status);
        }
    }

    public class LibraryStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string Id = "Id";
            public const string Titile = "Titile";
            public const string Version = "Ver";
        }

        private LibraryStatisticItem(StatisticType type, string username, string id, string title, string version)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.Id, id},
                { DetailKeys.Titile , title},
                { DetailKeys.Version , version}
            };
        }

        public LibraryStatisticItem(XElement xElement)
            : base(xElement)
        {
        }

        public static LibraryStatisticItem CreateAddLibraryEven(User user, Library library)
        {
            return new LibraryStatisticItem(StatisticType.LIBRARY_ADD, user.Username, library.ServerId, library.Name, library.Version.ToString() );
        }

        public static LibraryStatisticItem CreateRemoveLibraryEven(User user, Library library)
        {
            return new LibraryStatisticItem(StatisticType.LIBRARY_REMOVED, user.Username, library.ServerId, library.Name, library.Version.ToString());
        }
    }

    public class VariousDetailsStatisticItem : StatisticItem
    {
        public static class DetailKeys
        {
            public const string SearchFor = "SearchFor";
            public const string Link = "Link";
        }

        private VariousDetailsStatisticItem(StatisticType type, string username, string searchFor)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.SearchFor, searchFor },
            };
        }

        private VariousDetailsStatisticItem(StatisticType type, string username, Uri link)
            : base(type, username)
        {
            Details = new Dictionary<string, string>() { 
                { DetailKeys.Link, link.ToString() },
            };
        }


        public VariousDetailsStatisticItem(XElement xElement)
            : base(xElement)
        {
        }

        public static VariousDetailsStatisticItem CreateOpenLinkEvent(User user, Uri link)
        {
            return new VariousDetailsStatisticItem(StatisticType.LINK_OPEN, user.Username, link);
        }

        public static VariousDetailsStatisticItem CreateSearchEvent(User user, string searchFor)
        {
            return new VariousDetailsStatisticItem(StatisticType.SEARCH_ITEM, user.Username, searchFor);
        }


        public override string GetServerFormatedString()
        {
            if (Type == StatisticType.LINK_OPEN)
            {
                var paramItemsStrings = Details.Select(x => String.Format("{0}", x.Value));
                return string.Format("{0},{1},{2}", Type.ToString(), String.Join(";", paramItemsStrings.ToArray()), Timestamp.ToString("O"));
            }
            else
            {
                return base.GetServerFormatedString();
            }
        }
    }
}
