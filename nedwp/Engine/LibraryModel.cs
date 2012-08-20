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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NedWp;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Reactive;
using NedWp.Resources.Languages;

namespace NedEngine
{

    public class LibraryModel : PropertyNotifierBase
    {
        public const string NedNodeTag = "NED_NODE";
        public const string TitleTag = "TITLE";
        public const string NedNodeChildrenTag = "CHILDS";
        public const string NedNodeDescriptionTag = "DESCRIPTION";
        public const string NedNodeLinkTag = "LINK";
        public const string NedNodeKeywordTag = "KEYWORD";
        public const string NedNodeTypeAttribute = "type";
        public const string NedNodeIdAttribute = "id";
        public const string NedNodeDataAttribute = "data";
        public const string LibraryTagType = "Library";
        public const string CatalogueTagType = "Catalog";
        public const string CategoryTagType = "Category";

        public enum LibraryLevel
        {
            Unknown = -1,
            Catalogue = 1,
            Category,
            MediaItemsList,
        }
        private XDocument LibraryDocument { get; set; }
        public XDocument ChangedContent { get; set; }
        public string LibraryName { get; private set; }
        public string LibraryId { get; private set; }
        public Library ActiveLibrary { get; private set; }
        public ObservableCollection<CatalogueModelItem> CatalogueItems { get; private set; }
        public ObservableCollection<CategoryModelItem> CategoryItems { get; private set; }
        public ObservableCollection<MediaItemsListModelItem> MediaItems { get; private set; }

        public LibraryModel( IDownloadEvents downloadEvents, IObservable<QueuedDownload> downloadEnqueuedEvent )
        {
            LibraryItemRemoved += UpdateParentOnLibraryItemRemoved;

            FilterEvents( downloadEnqueuedEvent ).Subscribe( item => item.ItemState = MediaItemState.Downloading );
            FilterEvents( downloadEvents.DownloadStopPendingEvent ).Subscribe( item => item.ItemState = MediaItemState.Remote );
            FilterEvents( downloadEvents.DownloadErrorEvent ).Subscribe( item => item.ItemState = MediaItemState.Remote );
            FilterEvents( downloadEvents.DownloadCompletedEvent ).Subscribe( item => item.ItemState = MediaItemState.Local );
        }

        private IObservable<MediaItemsListModelItem> FilterEvents( IObservable<QueuedDownload> downloadEvents )
        {
            return downloadEvents.Select( DownloadToMediaItem ).Where( item => item != null );
        }

        private MediaItemsListModelItem DownloadToMediaItem( QueuedDownload download )
        {
            if( ActiveLibrary == null )
                return null;

            return MediaItems.FirstOrDefault( item => item.Id == download.Id );
        }

        public static int GetCatalogueCount( string libraryContents )
        {
            return XDocument.Load( new StringReader( libraryContents ) )
                            .Element( NedNodeTag )
                            .Element( NedNodeChildrenTag )
                            .Elements( NedNodeTag )
                            .Count();
        }

        public string LoadLibrary( string library )
        {
            Library libraryToLoad = App.Engine.LoggedUser.Libraries.First( lib => lib.ServerId == library );
            return LoadLibrary( libraryToLoad );
        }

        // Load library and return it's id
        public string LoadLibrary( Library library )
        {
            ActiveLibrary = library;

            LibraryDocument = Library.GetLibraryContents( ActiveLibrary, App.Engine.LoggedUser );
            LibraryDocument.Changed += OnLibraryDocumentChanged;

            ChangedContent = Library.GetChangedContent( ActiveLibrary, App.Engine.LoggedUser );

            LibraryId = LibraryDocument.Root.Attribute( NedNodeIdAttribute ).Value;

            var mediaItemsQuery =
                from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                from nedNodeChildren in nedNodeElements.Element( NedNodeChildrenTag ).Elements()
                where (string)nedNodeElements.Attribute( NedNodeTypeAttribute ) == CategoryTagType
                select new MediaItemsListModelItem()
                {
                    Id = nedNodeChildren.Attribute( NedNodeIdAttribute ).Value,
                    LibraryId = this.LibraryId,
                    ParentId = nedNodeElements.Attribute( NedNodeIdAttribute ).Value,
                    Title = nedNodeChildren.Element( TitleTag ).Value,
                    FileName = nedNodeChildren.Attribute( NedNodeDataAttribute ) != null ? nedNodeChildren.Attribute( NedNodeDataAttribute ).Value : String.Empty,
                    ItemType = MediaItemsListModelItem.GetTypeFromString( nedNodeChildren.Attribute( NedNodeTypeAttribute ).Value ),
                    Description = nedNodeChildren.Element( NedNodeDescriptionTag ) != null ? nedNodeChildren.Element( NedNodeDescriptionTag ).Value : String.Empty,
                    ExternalLinks = ( from linkElement in nedNodeChildren.Elements( NedNodeLinkTag ) select linkElement.Value ).ToList(),
                    Keywords = ( from keywordElement in nedNodeChildren.Elements( NedNodeKeywordTag ) select keywordElement.Value ).ToList(),
                    IsChanged = ChangedContent != null ? ( from changedElements in ChangedContent.Root.Descendants( NedNodeTag )
                                                           where changedElements.Attribute( NedNodeIdAttribute ).Value == nedNodeChildren.Attribute( NedNodeIdAttribute ).Value
                                                           select changedElements ).Count() > 0 : false
                };
            ObservableCollection<MediaItemsListModelItem> AllMediaItemsTemp = new ObservableCollection<MediaItemsListModelItem>();
            foreach( MediaItemsListModelItem item in mediaItemsQuery )
            { // To avoid multiple observers notifications on initialization
                AllMediaItemsTemp.Add( item );
            }
            MediaItems = AllMediaItemsTemp;

            var categoryItemsQuery =
                from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                from nedNodeChildren in nedNodeElements.Element( NedNodeChildrenTag ).Elements()
                where (string)nedNodeElements.Attribute( NedNodeTypeAttribute ) == CatalogueTagType
                select new CategoryModelItem()
                {
                    Id = nedNodeChildren.Attribute( NedNodeIdAttribute ).Value,
                    ParentId = nedNodeElements.Attribute( NedNodeIdAttribute ).Value,
                    Title = nedNodeChildren.Element( TitleTag ).Value,
                    IsChanged = ChangedContent != null ? ( from changedElements in ChangedContent.Root.Descendants( NedNodeTag )
                                                           where changedElements.Attribute( NedNodeIdAttribute ).Value == nedNodeChildren.Attribute( NedNodeIdAttribute ).Value
                                                           select changedElements ).Count() > 0 : false
                };
            ObservableCollection<CategoryModelItem> AllCategoryItemsTemp = new ObservableCollection<CategoryModelItem>();
            foreach( CategoryModelItem item in categoryItemsQuery )
            { // To avoid multiple observers notifications on initialization
                AllCategoryItemsTemp.Add( item );
            }
            CategoryItems = AllCategoryItemsTemp;
            foreach( CategoryModelItem catItem in CategoryItems )
            {
                catItem.AddChildren( ( from catChild in MediaItems where catChild.ParentId == catItem.Id select catChild ).ToList() );
            }

            var catalogueItemsQuery =
                from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                from nedNodeChildren in nedNodeElements.Element( NedNodeChildrenTag ).Elements()
                where (string)nedNodeElements.Attribute( NedNodeTypeAttribute ) == LibraryTagType
                select new CatalogueModelItem()
                {
                    Id = nedNodeChildren.Attribute( NedNodeIdAttribute ).Value,
                    ParentId = nedNodeElements.Attribute( NedNodeIdAttribute ).Value,
                    Title = nedNodeChildren.Element( TitleTag ).Value,
                    Subtitle = CatalogueModelItem.GetSubtitleString( nedNodeChildren.Element( NedNodeChildrenTag ).Elements().Count<XElement>() ),
                    IsChanged = ChangedContent != null ? ( from changedElements in ChangedContent.Root.Descendants( NedNodeTag )
                                                           where changedElements.Attribute( NedNodeIdAttribute ).Value == nedNodeChildren.Attribute( NedNodeIdAttribute ).Value
                                                           select changedElements ).Count() > 0 : false
                };
            ObservableCollection<CatalogueModelItem> CatalogueItemsTemp = new ObservableCollection<CatalogueModelItem>();
            foreach( CatalogueModelItem item in catalogueItemsQuery )
            { // To avoid multiple observers notifications on initialization
                CatalogueItemsTemp.Add( item );
            }
            CatalogueItems = CatalogueItemsTemp;
            CatalogueItems.CollectionChanged += OnCatalogueItemsPropertyChanged;

            LibraryName = LibraryDocument.Root.Element( TitleTag ).Value;
            UpdateMediaItemsDownloadedStatus();

            return LibraryId;
        }

        private void UpdateMediaItemsDownloadedStatus()
        {
            using( IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication() )
            {
                string searchpath = Path.Combine( Utils.LibraryDirPath( App.Engine.LoggedUser, ActiveLibrary ), "*.*" );
                IEnumerable<string> filenamesInLibraryDir = appDirectory.GetFileNames( searchpath ).Cast<string>();
                IEnumerable<string> filenamesOnDownloadList =
                    from downloads in App.Engine.LoggedUser.Downloads
                    where downloads.LibraryId == ActiveLibrary.ServerId
                    select downloads.LocalFilename;
                IEnumerable<string> downloadedFilenames = filenamesInLibraryDir.Except( filenamesOnDownloadList );

                foreach( MediaItemsListModelItem item in MediaItems )
                {
                    if( downloadedFilenames.Contains( Utils.FilenameToLocalFilename( item.FileName ) ) )
                    {
                        item.ItemState = MediaItemState.Local;
                    }
                    else if( filenamesOnDownloadList.Contains( Utils.FilenameToLocalFilename( item.FileName ) ) )
                    {
                        item.ItemState = MediaItemState.Downloading;
                    }
                }
            }
        }

        public IEnumerable<MediaItemsListModelItem> GetAllMediaItemsUnderId( string id )
        {
            return
                from mediaItem in MediaItems
                where (
                    from rootNode in LibraryDocument.Descendants( NedNodeTag )
                    where rootNode.Attribute( NedNodeIdAttribute ).Value == id
                    from mediaNode in rootNode.Descendants( NedNodeTag )
                    let childType = mediaNode.Attribute( NedNodeTypeAttribute ).Value
                    where childType != CategoryTagType && childType != CatalogueTagType && childType != LibraryTagType
                    select mediaNode.Attribute( NedNodeIdAttribute ).Value
                ).Contains( mediaItem.Id )
                select mediaItem;
        }

        private void SaveLibrary()
        {
            Library.SaveLibraryContents( LibraryDocument.ToString(), ActiveLibrary, App.Engine.LoggedUser );
        }

        public void DeleteItem( LibraryModelItem item )
        {
            XElement node =
                ( from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                  where (string)nedNodeElements.Attribute( NedNodeIdAttribute ) == item.Id
                  select nedNodeElements ).FirstOrDefault();
            if( node != null )
            {
                node.Remove();
            }

            if( item is CatalogueModelItem )
            {
                CatalogueItems.Remove( item as CatalogueModelItem );
                var query = from categoriesToRemove in CategoryItems where categoriesToRemove.ParentId == item.Id select categoriesToRemove;
                List<CategoryModelItem> catTempList = query.ToList<CategoryModelItem>();
                foreach( LibraryModelItem itemToRemove in catTempList )
                {
                    DeleteItem( itemToRemove );
                }
            }
            else if( item is CategoryModelItem )
            {
                CategoryItems.Remove( item as CategoryModelItem );
                foreach( LibraryModelItem miToRemove in ( item as CategoryModelItem ).Children() )
                {
                    DeleteItem( miToRemove );
                }
            }
            else if( item is MediaItemsListModelItem )
            {
                MediaItemsListModelItem mediaItem = item as MediaItemsListModelItem;
                MediaItems.Remove( mediaItem );
                App.Engine.DeleteMediaItem( mediaItem );
            }
            else
            {
                Debug.Assert( false, FileLanguage.LibraryModel_RemovingUnknowTypeError );
            }
            App.Engine.StatisticsManager.LogItemDeleted( item.Id );
            OnLibraryItemRemoved( new LibraryRemovedEventArgs() { RemovedItem = item } );
        }

        public delegate void LibraryItemRemovedEventHandler( object sender, LibraryRemovedEventArgs args );
        public event LibraryItemRemovedEventHandler LibraryItemRemoved;

        protected virtual void OnLibraryItemRemoved( LibraryRemovedEventArgs args )
        {
            if( LibraryItemRemoved != null )
            {
                LibraryItemRemoved( this, args );
            }
        }

        private void UpdateParentOnLibraryItemRemoved( object sender, LibraryRemovedEventArgs args )
        {
            LibraryModelItem item = args.RemovedItem as LibraryModelItem;
            if( args.RemovedItem is MediaItemsListModelItem )
            {
                CategoryModelItem parent = ( from parentItem in CategoryItems where parentItem.Id == item.ParentId select parentItem ).FirstOrDefault();
                if( parent != null )
                {
                    parent.RemoveChild( item as MediaItemsListModelItem );
                }
            }
            else if( args.RemovedItem is CategoryModelItem )
            {
                CatalogueModelItem parent = ( from parentItem in CatalogueItems where parentItem.Id == item.ParentId select parentItem ).FirstOrDefault();
                if( parent != null )
                {
                    int childrenCount = ( from categoryItem in CategoryItems where categoryItem.ParentId == item.ParentId select categoryItem ).Count();
                    parent.Subtitle = CatalogueModelItem.GetSubtitleString( childrenCount );
                }
            }
            // Currently there is no need to handle CatalogueModelItem removal
        }

        public LibraryLevel GetNodeType( string id )
        {
            var query = from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                        where (string)nedNodeElements.Attribute( NedNodeIdAttribute ) == id
                        select new { NodeType = (string)nedNodeElements.Attribute( NedNodeTypeAttribute ) };
            switch( query.First().NodeType )
            {
                case LibraryTagType:
                    return LibraryLevel.Catalogue;
                case CatalogueTagType:
                    return LibraryLevel.Category;
                case CategoryTagType:
                    return LibraryLevel.MediaItemsList;
                default:
                    return LibraryLevel.Unknown;
            }
        }

        public string GetNodeTitle( string id )
        {
            var query = from nedNodeElements in LibraryDocument.Descendants( NedNodeTag )
                        where (string)nedNodeElements.Attribute( NedNodeIdAttribute ) == id
                        select new { NodeTitle = (string)nedNodeElements.Element( TitleTag ) };
            return query.First().NodeTitle;
        }

        public object GetDataSourceForId( string id )
        {
            switch( GetNodeType( id ) )
            {
                case LibraryLevel.Catalogue:
                    return CatalogueItems;
                case LibraryLevel.Category:
                    return CategoryItems;
                case LibraryLevel.MediaItemsList:
                    return MediaItems;
                default:
                    return null;
            }
        }

        public MediaItemsListModelItem GetMediaItemForId( string id )
        {
            var mediaItemQuery = from items in MediaItems
                                 where items.Id == id
                                 select items;
            MediaItemsListModelItem resultItem = mediaItemQuery.First<MediaItemsListModelItem>();
            return resultItem != null ? resultItem : new MediaItemsListModelItem();
        }

        private void OnLibraryDocumentChanged( object sender, XObjectChangeEventArgs args )
        {
            if( args.ObjectChange == XObjectChange.Add || args.ObjectChange == XObjectChange.Remove )
            {
                SaveLibrary();
            }
        }

        private void OnCatalogueItemsPropertyChanged( object sedner, NotifyCollectionChangedEventArgs args )
        {
            if( ActiveLibrary != null && ( args.Action == NotifyCollectionChangedAction.Add || args.Action == NotifyCollectionChangedAction.Remove ) )
            {
                ActiveLibrary.CatalogueCount = CatalogueItems.Count();
            }
        }

        #region Tombstoning

        public bool IsLoaded()
        {
            return ( ActiveLibrary != null );
        }

        private const string CurrentLibraryIdKey = "CurrentLibraryId";
        public void LoadLibraryStateIfNotLoaded()
        {
            if( !IsLoaded() )
            {
                IDictionary<string, object> state = PhoneApplicationService.Current.State;
                if( state.ContainsKey( CurrentLibraryIdKey ) )
                {
                    LoadLibrary( (string)state[CurrentLibraryIdKey] );
                }
            }
        }

        public void SaveLibraryState()
        {
            IDictionary<string, object> state = PhoneApplicationService.Current.State;
            state[CurrentLibraryIdKey] = ActiveLibrary.ServerId;
        }

        #endregion Tombstoning
    }

    public class LibraryRemovedEventArgs : EventArgs
    {
        public LibraryModelItem RemovedItem { get; set; }
    }
}
