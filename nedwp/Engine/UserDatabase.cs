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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace NedEngine
{
    public static class Tags
    {
        public const string Users = "Users";
        public const string User = "User";
        public const string Username = "Username";
        public const string Password = "Password";
        public const string Settings = "Settings";
        public const string AutoStatUpload = "AutoStatUpload";
        public const string AutoDownload = "AutoDownload";
        public const string Libraries = "Libraries";
        public const string Library = "Library";
        public const string LibraryName = "Name";
        public const string Visible = "Visible";
        public const string ServerId = "ServerId";
        public const string LocalId = "LocalId";
        public const string Version = "Version";
        public const string CatalogueCount = "CatalogueCount";
        public const string Statistic = "Statistic";
        public const string StatisticType = "StatisticType";
        public const string Timestamp = "Timestamp";
        public const string Statistics = "Statistics";
        public const string Download = "Download";
        public const string Title = "Title";
        public const string Downloads = "Downloads";
        public const string MediaType = "MediaType";
        public const string DownloadState = "DownloadState";
        public const string Filename = "Filename";
        public const string DownloadSize = "DownloadSize";
    }

    public class UserDatabase : LinqToXmlDatabase
    {
        protected override XElement Data
        {
            get { return new XElement(Tags.Users, from u in Users select u.UserCredentials); }
        }

        protected override string PathToFile { get { return "users.xml"; } }

        public UserDatabase()
        {
            XDocument doc = Open();
            Users = new ObservableCollectionEx<User>(from u in doc.Descendants(Tags.User) select new User(u));

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var user in Users)
                {
                    foreach (var download in user.Downloads)
                    {
                        string path = Utils.MediaFilePath(user, download);
                        if (isf.FileExists(path))
                        {
                            using (var fileStream = isf.OpenFile(path, FileMode.Open))
                            {
                                download.DownloadedBytes = fileStream.Length;
                            }
                        }
                    }
                }
            }

            Users.CollectionChanged += (sender, e) => { Save(true); };
        }


        public ObservableCollectionEx<User> Users { get; private set; }

        public User GetUser(string login)
        {
            return (from user in Users where user.Username == login select user).First();
        }

    }
}
