/*******************************************************************************
* Copyright (c) 2011-2012 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
using System.Collections.Generic;
using System.Windows.Controls;
using System.Diagnostics;
using System;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using NedWp;
using NedWp.Resources.Languages;
namespace NedEngine
{

    public enum MediaItemState
    {
        Local,
        Downloading,
        Remote
    }

    public class MediaItemsListModelItem : LibraryModelItem
    {

        private MediaItemType mMediType;
        public MediaItemType ItemType
        {
            get
            {
                return mMediType;
            }
            set
            {
                mMediType = value;
                MediaIcon = Utils.GetMediaIcon( mMediType );
            }
        }
        private string mDescription;
        public string Description
        {
            get
            {
                if( String.IsNullOrEmpty( mDescription ) )
                    return FileLanguage.NO_DETAILS;
                else
                    return mDescription;
            }
            set
            {
                mDescription = value;
            }
        }

        private string mMediaIcon;
        public string MediaIcon
        {
            get { return mMediaIcon; }
            set
            {
                if( mMediaIcon != value )
                {
                    mMediaIcon = value;
                    OnPropertyChanged( "MediaIcon" );
                }
            }
        }

        public string FileName { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> ExternalLinks { get; set; }

        private MediaItemState mItemState;
        public MediaItemState ItemState
        {
            get { return mItemState; }
            set
            {
                if( mItemState != value )
                {
                    mItemState = value;
                    OnPropertyChanged( "ItemState" );
                    if( ItemState == MediaItemState.Local )
                        IsItemDownloaded = true;
                }

            }
        }

        private bool mIsItemDownloaded;
        public bool IsItemDownloaded
        {
            get { return mIsItemDownloaded; }
            set
            {
                if( mIsItemDownloaded != value )
                {
                    mIsItemDownloaded = value;
                    OnPropertyChanged( "IsItemDownloaded" );
                }
            }
        }

        public MediaItemsListModelItem()
        {
            Keywords = new List<string>();
            ExternalLinks = new List<string>();
            ItemType = MediaItemType.Undefined;
            ItemState = MediaItemState.Remote;
        }

        public static MediaItemType GetTypeFromString( string typeString )
        {
            MediaItemType resolvedType = MediaItemType.Undefined;
            try
            {
                resolvedType = (NedEngine.MediaItemType)Enum.Parse( typeof( NedEngine.MediaItemType ), typeString, true );
            }
            catch( ArgumentException )
            {
                System.Diagnostics.Debug.WriteLine( "Library parse error: Could not parse item type, setting as Undefined" );
            }
            return resolvedType;
        }

        public string GetMediaFileIsolatedStoragePath()
        {
            return Utils.MediaFilePath( App.Engine.LoggedUser, this );
        }
    }
}
