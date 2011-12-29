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
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;

namespace NedEngine
{
    public class User :  LinqToXmlDatabase, INotifyPropertyChanged 
    {
        public Guid LocalId { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public Settings Settings { get; private set; }
        public ObservableCollectionEx<Library> Libraries { get; private set; }
        public ObservableCollectionEx<QueuedDownload> Downloads { get; private set; }
        protected override string PathToFile
        {
            get 
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if(!myIsolatedStorage.DirectoryExists(LocalId.ToString()))
                    myIsolatedStorage.CreateDirectory(LocalId.ToString());
                return System.IO.Path.Combine(LocalId.ToString(), "user.xml"); 
            }
        }

        private void SubscribeForComponentChanges()
        {
            Settings.PropertyChanged += (sender, e) => { OnPropertyChanged("Settings"); Save(false); };
            Libraries.CollectionChanged += (sender, e) => { OnPropertyChanged("Libraries"); Save(false); };
            ((INotifyPropertyChanged)Libraries).PropertyChanged += (sender, e) => { if (e.PropertyName != "Count" && e.PropertyName != "Item[]") OnPropertyChanged("LibraryElement"); Save(false); };
            Downloads.CollectionChanged += (sender, e) => { OnPropertyChanged("Downloads"); Save(false); };
            ((INotifyPropertyChanged)Downloads).PropertyChanged += (sender, e) => { if (e.PropertyName != "DownloadedBytes" && e.PropertyName != "Count" && e.PropertyName != "Item[]") { OnPropertyChanged("DownloadElement"); Save(false); } };
        }

        public User(string username, string password)
        {
            LocalId = Guid.NewGuid();
            Username = username;
            Password = password;
            Settings = new Settings();
            Libraries = new ObservableCollectionEx<Library>();
            Downloads = new ObservableCollectionEx<QueuedDownload>();
            Save(true);
            SubscribeForComponentChanges();
        }

        public User(XElement xElement)
        {
            LocalId = new Guid(xElement.Attribute(Tags.LocalId).Value);
            Username = xElement.Attribute(Tags.Username).Value;
            Password = xElement.Attribute(Tags.Password).Value;

            XDocument doc = Open();
            XElement root = doc.Element(Tags.User);
            Settings = new Settings(root.Element(Tags.Settings));
            Libraries = new ObservableCollectionEx<Library>(from lib in root.Element(Tags.Libraries).Elements(Tags.Library) select new Library(lib));
            Downloads = new ObservableCollectionEx<QueuedDownload>(from download in root.Element(Tags.Downloads).Elements(Tags.Download) select new QueuedDownload(download));
            SubscribeForComponentChanges();
        }

        public XElement UserCredentials
        {
            get
            {
                return new XElement(Tags.User,
                        new XAttribute(Tags.LocalId, LocalId.ToString()),
                        new XAttribute(Tags.Username, Username),
                        new XAttribute(Tags.Password, Password)
                   );
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override XElement Data
        {
            get
            {
                return new XElement(Tags.User,
                         Settings.Data,
                         new XElement(Tags.Libraries, from lib in Libraries select lib.Data),
                         new XElement(Tags.Downloads, from download in Downloads select download.Data)
                    );
            }
        }
    }
}
