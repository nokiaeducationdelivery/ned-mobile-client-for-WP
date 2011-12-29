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
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;
using Microsoft.Phone.Reactive;


namespace NedEngine
{
    public abstract class LinqToXmlDatabase
    {
        private Subject<Unit> _saveEvent = new Subject<Unit>();
        private IObservable<Unit> SaveEvent { get { return _saveEvent; } }
        private bool _waitingForSaveFlag;

        public LinqToXmlDatabase()
        {
            var throtleObj = SaveEvent.Throttle<Unit>(TimeSpan.FromSeconds(0.5), Scheduler.ThreadPool);

            throtleObj.Subscribe(
                var =>
                    {
                        Save(true);
                        _waitingForSaveFlag = false;
                    });
            
        }
        ~LinqToXmlDatabase()
        {
            if (_waitingForSaveFlag)
            {
                Save(true);
            }
        }

        protected abstract XElement Data { get; }
        protected abstract string PathToFile { get; }

        public XDocument Open()
        {
            XDocument doc = null;
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists(PathToFile))
                {
                    using (IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(PathToFile, FileMode.Open, file))
                    {
                        doc = XDocument.Load(isfStream);
                    }
                }
                else
                {
                    doc = new XDocument();
                }
            }
            return doc;
        }

        private bool _savingStoped;
        public void StopSaving()
        {
            _savingStoped = true;
        }

        public void Save(bool immediately)
        {
            if (!_savingStoped)
            {
                if (!immediately)
                {
                    _waitingForSaveFlag = true;
                    _saveEvent.OnNext(new Unit());
                }
                else
                {
                    try
                    {
                        using (IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(PathToFile, FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication()))
                        {
                            XDocument doc = new XDocument(Data);
                            doc.Save(isfStream);
                        }
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Failed to save data to file: {0}", PathToFile));
                    }
                }
            }
        }
    }
}
