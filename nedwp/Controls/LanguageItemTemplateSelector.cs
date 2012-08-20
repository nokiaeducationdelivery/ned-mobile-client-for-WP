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

using System.Windows.Controls;
using System.Windows;
using NedEngine;
using System.Diagnostics;
namespace NedWp
{
    public class LanguageItemTemplateSelector : ContentControl
    {
        public DataTemplate DefaultLanguageTemplate { get; set; }
        public DataTemplate DownloadedLanguageTemplate { get; set; }

        public DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is LanguageInfo)
                {
                    LanguageInfo info = item as LanguageInfo;
                    if (info.IsDefault)
                    {
                        return DefaultLanguageTemplate;
                    }
                    return DownloadedLanguageTemplate;
                }
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
