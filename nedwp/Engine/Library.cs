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
using System.Linq;
using System.Collections.Generic;
using NedWp;

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

        public static void PrepareDiffXml(Library library, User owner)
        {
            XDocument oldLib;
            XDocument newLib;
            XDocument oldDiff = null;
            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if(!appDirectory.FileExists(Utils.LibraryXmlPreviousPath(owner, library)))
                {
                    return;
                }
                using (var stream = appDirectory.OpenFile(Utils.LibraryXmlPreviousPath(owner, library), FileMode.Open, FileAccess.Read))
                {
                    oldLib = XDocument.Load(stream);
                }
                using (var stream = appDirectory.OpenFile(Utils.LibraryXmlPath(owner, library), FileMode.Open, FileAccess.Read))
                {
                    newLib = XDocument.Load(stream);
                }
                if(appDirectory.FileExists(Utils.LibraryXmlDiffPath(owner, library)))
                {
                    using (var stream = appDirectory.OpenFile(Utils.LibraryXmlDiffPath(owner, library), FileMode.Open, FileAccess.Read))
                    {
                        oldDiff = XDocument.Load(stream);
                    }
                }
            }

            XElement changesRoot = new XElement("changes");
            XDocument changes = new XDocument(changesRoot);
            
            List<XElement> newNodes = new List<XElement>(newLib.Descendants(LibraryModel.NedNodeTag));

            //step 1 - leave in newLib xml only changed elements
            for( int i = newNodes.Count - 1; i>= 0; i --)
            {
                XElement newNode = newNodes[i];
                var oldNodesSearch =
                    from oldElements in oldLib.Descendants(LibraryModel.NedNodeTag)
                    where (string)oldElements.Attribute(LibraryModel.NedNodeIdAttribute) == (string)newNode.Attribute(LibraryModel.NedNodeIdAttribute)
                    select oldElements;

                if (oldNodesSearch.Count() > 0)
                {
                    if (areItemsEqual(newNode, oldNodesSearch.First()) &&  newNode.Descendants(LibraryModel.NedNodeTag).Count() == 0)
                    {
                        if(oldDiff == null || (from oldDiffNode in oldDiff.Root.Descendants(LibraryModel.NedNodeTag) 
                                                   where oldDiffNode.Attribute(LibraryModel.NedNodeIdAttribute).Value == newNode.Attribute(LibraryModel.NedNodeIdAttribute).Value
                                                   select oldDiff).Count() <= 0)
                        newNode.Remove();
                    }
                }
            }

            updateXml(newLib, library, owner);

            
        }


        private static void updateXml(XDocument newXml, Library library, User owner)
        {
            //step 2 cleanup empty categories
            var emptyCategories =
                from newElement in newXml.Descendants(LibraryModel.NedNodeTag)
                where newElement.Attribute(LibraryModel.NedNodeTypeAttribute).Value == LibraryModel.CategoryTagType
                    && newElement.Descendants(LibraryModel.NedNodeTag).Count() == 0
                select newElement;
            for ( int i = emptyCategories.Count() - 1; i >= 0; i--)
            {
                XElement category = emptyCategories.ElementAt(i);
                string categoryToDeleteId = category.Attribute(LibraryModel.NedNodeIdAttribute).Value;
                foreach (CategoryModelItem categoryModelItem in App.Engine.LibraryModel.CategoryItems)
                {
                    if (categoryModelItem.Id == categoryToDeleteId)
                    {
                        categoryModelItem.IsChanged = false;
                        break;
                    }
                }
                category.Remove();
            }

            //step 3 cleanup empty catalogues
            var emptyCatalogs =
                from newElement in newXml.Descendants(LibraryModel.NedNodeTag)
                where newElement.Attribute(LibraryModel.NedNodeTypeAttribute).Value == LibraryModel.CatalogueTagType
                    && newElement.Descendants(LibraryModel.NedNodeTag).Count() == 0
                select newElement;

            for (int i = emptyCatalogs.Count() - 1; i >= 0; i--)
            {
                XElement catalogue = emptyCatalogs.ElementAt(i);
                string catalogueToDeleteId = catalogue.Attribute(LibraryModel.NedNodeIdAttribute).Value;
                foreach (CatalogueModelItem catalogueModelItem in App.Engine.LibraryModel.CatalogueItems)
                {
                    if (catalogueModelItem.Id == catalogueToDeleteId)
                    {
                        catalogueModelItem.IsChanged = false;
                        break;
                    }
                }
                catalogue.Remove();
            }


            //step 4 save diff xml and remove previous library xml
            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (newXml.Root != null)
                {
                    using (var stream = appDirectory.OpenFile(Utils.LibraryXmlDiffPath(owner, library), FileMode.Create))
                    {
                        newXml.Save(stream);
                    }
                }
                else
                {
                    if (appDirectory.FileExists(Utils.LibraryXmlDiffPath(owner, library)))
                    {
                        appDirectory.DeleteFile(Utils.LibraryXmlDiffPath(owner, library));
                    }
                }
                if (appDirectory.FileExists(Utils.LibraryXmlPreviousPath(owner, library)))
                {
                    appDirectory.DeleteFile(Utils.LibraryXmlPreviousPath(owner, library));
                }
            }
        }

        public static void markItemWatched(string id)
        {
            XDocument currentDiff = App.Engine.LibraryModel.ChangedContent;
            var itemToDelete = from watchedElement  in  currentDiff.Root.Descendants(LibraryModel.NedNodeTag)
                               where (string) watchedElement.Attribute(LibraryModel.NedNodeIdAttribute) == id
                               select watchedElement;

            if ( itemToDelete.Count() > 0 )
            {
                itemToDelete.First().Remove();
            }
            updateXml(currentDiff, App.Engine.LibraryModel.ActiveLibrary, App.Engine.LoggedUser);
        }


        private static bool areItemsEqual(XElement newItem, XElement oldItem)
        {
            return safeEquals(newItem.Element(LibraryModel.TitleTag), oldItem.Element(LibraryModel.TitleTag)) &&
                   safeEquals(newItem.Attribute(LibraryModel.NedNodeTypeAttribute), oldItem.Attribute(LibraryModel.NedNodeTypeAttribute)) &&
                   safeEquals(newItem.Attribute(LibraryModel.NedNodeDataAttribute), oldItem.Attribute(LibraryModel.NedNodeDataAttribute)) &&
                   safeEquals(newItem.Element(LibraryModel.NedNodeDescriptionTag), oldItem.Element(LibraryModel.NedNodeDescriptionTag)) &&
                   safeEquals((from linkElement in newItem.Elements(LibraryModel.NedNodeLinkTag) select linkElement.Value).ToList(), (from linkElement in oldItem.Elements(LibraryModel.NedNodeLinkTag) select linkElement.Value).ToList()) &&
                   safeEquals((from keywordElement in newItem.Elements(LibraryModel.NedNodeKeywordTag) select keywordElement.Value).ToList(), (from keywordElement in oldItem.Elements(LibraryModel.NedNodeKeywordTag) select keywordElement.Value).ToList());
        }

        private static bool safeEquals(List<string> list, List<string> list_2)
        {
            if(list.Count != list_2.Count)
            {
                return false;
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != list_2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool safeEquals(XElement xElement, XElement xElement_2)
        {
            return (xElement == null && xElement_2 == null) ||
                    (xElement != null && xElement_2 != null && xElement.Value == xElement_2.Value);
        }

        private static bool safeEquals(XAttribute xAttribute, XAttribute xAttribute_2)
        {
            return (xAttribute == null && xAttribute_2 == null) ||
                    (xAttribute != null && xAttribute_2 != null && xAttribute.Value == xAttribute_2.Value);
        }

        public static XDocument GetChangedContent(Library library, User user)
        {
            using (IsolatedStorageFile appDirectory = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (appDirectory.FileExists(Utils.LibraryXmlDiffPath(user, library)))
                {
                    using (var stream = appDirectory.OpenFile(Utils.LibraryXmlDiffPath(user, library), FileMode.Open, FileAccess.Read))
                    {
                        return XDocument.Load(stream);
                    }
                }
                return null;
            }
        }
    }
}
