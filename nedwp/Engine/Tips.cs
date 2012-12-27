/*******************************************************************************
* Copyright (c) 2012 Nokia Corporation
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://www.eclipse.org/legal/epl-v10.html
*
* Contributors:
* Comarch team - initial API and implementation
*******************************************************************************/
using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using NedWp;
using NedWp.Resources.Languages;

namespace NedEngine
{
    public class Tips : PropertyNotifierBase
    {
        private List<String> _allTips = null;
        public Tips()
        {
            Random rand = new Random();
            _allTips = FileLanguage.AllTips();
            if (_allTips.Count > 0)
            {
                CurrentTip = _allTips[rand.Next(_allTips.Count)];
            }
            else
            {
                CurrentTip = "";
            }
        }

        private string _currentTip;
        public string CurrentTip 
        { 
            get
            {
                return _currentTip;
            }
            set
            {
                _currentTip = value;
                OnPropertyChanged("CurrentTip");
            }
        }

        public bool ShowTips
        {
            get
            {
                return true;
            }
        }

        public void RollNext()
        {
            if (_allTips != null && _allTips.Count > 1)
            {
                string newTip = CurrentTip;
                while (newTip == CurrentTip)
                {
                    Random rand = new Random();
                    newTip = _allTips[rand.Next(_allTips.Count)];
                }
                CurrentTip = newTip;
            }
        }
    }
}
