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
using System.Collections.Generic;

namespace NedEngine
{
    public class LibraryModelItem : PropertyNotifierBase
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string LibraryId { get; set; }
        public string Title { get; set; }
        private string mSubtitle;
        public string Subtitle {
            get { return mSubtitle; }
            set
            {
                if (value != mSubtitle)
                {
                    mSubtitle = value;
                    OnPropertyChanged("Subtitle");
                }
            }
        }
    }
}
