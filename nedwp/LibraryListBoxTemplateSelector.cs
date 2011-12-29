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
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using NedEngine;

namespace NedWp
{
    public class LibraryListBoxTemplateSelector : ContentControl
    {
        public DataTemplate CatalogueTemplate { get; set; }
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate MediaItemsListTemplate { get; set; }

        public DataTemplate SelectTemplate( object item, DependencyObject container)        
        {
            if (item != null)
            {
                if (item is CatalogueModelItem)
                    return CatalogueTemplate;
                else if (item is CategoryModelItem)
                    return CategoryTemplate;
                else if (item is MediaItemsListModelItem)
                    return MediaItemsListTemplate;
            }
            Debug.Assert(false);
            return null;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }
    }
}
