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
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using NedWp.Resources.Languages;

namespace NedEngine
{
    public class CategoryModelItem : LibraryModelItem
    {
        private ObservableCollection<MediaItemsListModelItem> ChildrenItems { get; set; }

        public CategoryModelItem()
        {
            ChildrenItems = new ObservableCollection<MediaItemsListModelItem>();
        }

        public void AddChild( MediaItemsListModelItem child )
        {
            AddChildToCollection( child );
            UpdateSubtitleString();
        }

        public void AddChildren( List<MediaItemsListModelItem> children )
        {
            foreach( MediaItemsListModelItem child in children )
                AddChildToCollection( child );
            UpdateSubtitleString();
        }

        public void RemoveChild( MediaItemsListModelItem child )
        {
            RemoveChildFromCollection( child );
            UpdateSubtitleString();
        }

        public List<MediaItemsListModelItem> Children()
        {
            return ChildrenItems.ToList();
        }

        private void AddChildToCollection( MediaItemsListModelItem child )
        {
            child.PropertyChanged += OnChildPropertyChanged;
            ChildrenItems.Add( child );
        }

        private void RemoveChildFromCollection( MediaItemsListModelItem child )
        {
            ChildrenItems.Remove( child );
            child.PropertyChanged -= OnChildPropertyChanged;
        }

        private void UpdateSubtitleString()
        {
            int childrenCount = ChildrenItems.Count;
            int local = ( from downloadedChildren in ChildrenItems where downloadedChildren.IsItemDownloaded select downloadedChildren ).Count();
            int remote = childrenCount - local;
            Subtitle = String.Format( FileLanguage.MEDIA_ITEMS, childrenCount, local, remote );
        }

        private void OnChildPropertyChanged( object sender, PropertyChangedEventArgs args )
        {
            if( args.PropertyName == "IsItemDownloaded" )
            {
                UpdateSubtitleString();
            }
        }
    }
}
