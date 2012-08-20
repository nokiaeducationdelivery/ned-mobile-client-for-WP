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
using NedWp.Resources.Languages;

namespace NedEngine
{
    public class CatalogueModelItem : LibraryModelItem
    {
        public static string GetSubtitleString( int childrenCount )
        {
            return String.Format( FileLanguage.CATEGORIES, childrenCount );
        }
    }
}
