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
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

namespace NedEngine
{
    public class Library : PropertyNotifierBase
    {
        public string Name { get; private set; }

        private bool _visible;
        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    OnPropertyChanged("Visible");
                }
            }
        }

        public string ServerId { get; private set; }
        public Guid LocalId { get; private set; }

        private int _version;
        public int Version
        {
            get
            {
                return _version;
            }
            set
            {
                if (value != _version)
                {
                    _version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        private int _catalogueCount;
        public int CatalogueCount
        {
            get
            {
                return _catalogueCount;
            }
            set
            {
                if (value != _catalogueCount)
                {
                    _catalogueCount = value;
                    OnPropertyChanged("CatalogueCount");
                }
            }
        }

        public Library(string id, string name, int version)
        {
            Name = name;
            ServerId = id;
            Visible = true;
            Version = version;
            LocalId = Guid.NewGuid();
            CatalogueCount = -1;
        }

        public Library(XElement xElement)
        {
            Name = xElement.Attribute(Tags.LibraryName).Value;
            Visible = Convert.ToBoolean(xElement.Attribute(Tags.Visible).Value);
            ServerId = xElement.Attribute(Tags.ServerId).Value;
            LocalId = new Guid(xElement.Attribute(Tags.LocalId).Value);
            Version = Convert.ToInt32(xElement.Attribute(Tags.Version).Value);
            CatalogueCount = Convert.ToInt32(xElement.Attribute(Tags.CatalogueCount).Value);
        }

        public XElement Data
        {
            get
            {
                return new XElement(Tags.Library,
                        new XAttribute(Tags.LibraryName, Name),
                        new XAttribute(Tags.Visible, Convert.ToString(Visible)),
                        new XAttribute(Tags.ServerId, ServerId),
                        new XAttribute(Tags.LocalId, LocalId.ToString()),
                        new XAttribute(Tags.Version, Convert.ToString(Version)),
                        new XAttribute(Tags.CatalogueCount, Convert.ToString(CatalogueCount)));
            }
        }

        public static void SaveLibraryContents(string updatedContents, Library libToUpdate, User owner)
        {
            using (IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string libraryDirectoryPath = Utils.LibraryDirPath(owner, libToUpdate);
                if (!isolatedStorage.DirectoryExists(libraryDirectoryPath))
                {
                    isolatedStorage.CreateDirectory(libraryDirectoryPath);
                    isolatedStorage.CreateDirectory("shared/transfers/" + libraryDirectoryPath);
                }

                using (StreamWriter writer = new StreamWriter(new IsolatedStorageFileStream(Utils.LibraryXmlPath(owner, libToUpdate), FileMode.Create, isolatedStorage)))
                {
                    writer.Write(updatedContents);
                }
            }
        }

        public static XDocument GetLibraryContents(Library libToLoad, User owner)
        {
            XDocument result = null;
            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = appDirectory.OpenFile(Utils.LibraryXmlPath(owner, libToLoad), FileMode.Open, FileAccess.Read))
            {
                result = XDocument.Load(stream);
            }
            return result;
        }
    }
}
