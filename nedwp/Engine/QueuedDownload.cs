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
using System.Xml.Linq;

namespace NedEngine
{
    public class QueuedDownload : PropertyNotifierBase
    {
        public enum DownloadState
        {
            Queued,
            Downloading,
            Paused
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        private MediaItemType Type { get; set; }
        public string MediaIcon
        {
            get
            {
                return Utils.GetMediaIcon(Type);
            }
        }

        private DownloadState _state;
        public DownloadState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (value != _state)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public string LibraryId { get; private set; }
        public string Filename { get; private set; }

        public string LocalFilename
        {
            get
            {
                return Utils.FilenameToLocalFilename(Filename);
            }
        }

        private long _downloadedBytes;
        public long DownloadedBytes
        {
            get
            {
                return _downloadedBytes;
            }

            set
            {
                if (value != _downloadedBytes)
                {
                    _downloadedBytes = value;
                    OnPropertyChanged("DownloadedBytes");
                }
            }
        }

        private long _downloadSize;
        public long DownloadSize
        {
            get
            {
                return _downloadSize;
            }

            set
            {
                if (value != _downloadSize)
                {
                    _downloadSize = value;
                    OnPropertyChanged("DownloadSize");
                }
            }
        }

        public QueuedDownload(MediaItemsListModelItem mediaItem)
        {
            Id = mediaItem.Id;
            Type = mediaItem.ItemType;
            Title = mediaItem.Title;
            LibraryId = mediaItem.LibraryId;
            Filename = mediaItem.FileName;
            State = DownloadState.Queued;
            DownloadedBytes = 0;
            DownloadSize = long.MaxValue;
        }

        public QueuedDownload(XElement xElement)
        {
            Id = xElement.Attribute(Tags.ServerId).Value;
            Title = xElement.Attribute(Tags.Title).Value;
            Type = (MediaItemType)Enum.Parse(typeof(MediaItemType), xElement.Attribute(Tags.MediaType).Value, true);
            LibraryId = xElement.Attribute(Tags.Library).Value;
            Filename = xElement.Attribute(Tags.Filename).Value;
            State = (DownloadState)Enum.Parse(typeof(DownloadState), xElement.Attribute(Tags.DownloadState).Value, true);
            DownloadSize = Convert.ToInt64(xElement.Attribute(Tags.DownloadSize).Value);
        }

        public XElement Data
        {
            get
            {
                return new XElement(Tags.Download,
                        new XAttribute(Tags.ServerId, Id),
                        new XAttribute(Tags.Title, Title),
                        new XAttribute(Tags.MediaType, Type.ToString()),
                        new XAttribute(Tags.Library, LibraryId),
                        new XAttribute(Tags.Filename, Filename),
                        new XAttribute(Tags.DownloadState, State.ToString()),
                        new XAttribute(Tags.DownloadSize, Convert.ToString(DownloadSize))
                        );
            }
        }
    }
}
