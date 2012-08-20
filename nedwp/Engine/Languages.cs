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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NedWp;

namespace NedEngine
{
    public class LanguageInfo : PropertyNotifierBase, IEquatable<LanguageInfo>
    {
        public string Id { get; set; }

        public LanguageInfo( string id, string languageName, string locale, bool isLocal, string remoteName )
        {
            Id = id;
            LangName = languageName;
            Locale = locale;
            IsLocal = isLocal;
        }

        public LanguageInfo( string languageName, string locale, bool isLocal )
        {
            LangName = languageName;
            Locale = locale;
            IsLocal = isLocal;
        }

        public LanguageInfo()
        {
            LangName = "";
            Locale = "";
            IsLocal = true;
            ItemState = MediaItemState.Local;
            Id = "";
        }

        public LanguageInfo( XElement element )
        {
            _langName = element.Element( Tags.LanguageName ).Value;
            Locale = element.Element( Tags.LanguageLocale ).Value;
            Boolean.TryParse( element.Attribute( Tags.LanguageIsLocal ).Value, out _isLocal );
            ItemState = _isLocal ?
                MediaItemState.Local :
                MediaItemState.Remote;
            Id = element.Element( Tags.LanguageId ).Value;
        }

        private string _langName;
        public string LangName
        {
            get
            {
                return _langName;
            }
            set
            {
                if( value != _langName )
                {
                    _langName = value;
                    OnPropertyChanged( "LangName" );
                }
            }
        }
        public string Locale { get; set; }

        private bool _isLocal;
        public bool IsLocal
        {
            get
            {
                return _isLocal;
            }
            set
            {
                if( value != _isLocal )
                {
                    _isLocal = value;
                    ItemState = _isLocal ?
                        MediaItemState.Local :
                        MediaItemState.Remote;

                }
            }
        }

        private MediaItemState _itemState;
        public MediaItemState ItemState
        {
            get
            {
                return _itemState;
            }
            set
            {
                if( value != _itemState )
                {
                    _itemState = value;
                    OnPropertyChanged( "ItemState" );
                }
            }
        }

        public XElement Data
        {
            get
            {
                return new XElement( Tags.Language,
                        new XElement( Tags.LanguageName, new XText( LangName ) ),
                        new XElement( Tags.LanguageLocale, new XText( Locale ) ),
                        new XElement( Tags.LanguageId, new XText( Id ) ),
                        new XAttribute( Tags.LanguageIsLocal, IsLocal.ToString() ) );
            }
        }

        public bool Equals( LanguageInfo other )
        {
            return Id == other.Id;
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get
            {
                return _isCurrent;
            }
            set
            {
                if( value != IsCurrent )
                {
                    if( Id != App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage && value == true )
                    {
                        App.Engine.ApplicationSettings.AvailableLanguages.CurrentLanguage = Id;
                    }
                    _isCurrent = value;
                    OnPropertyChanged( "IsCurrent" );
                }
            }
        }

        internal void SetCurrentInternal( bool p )
        {
            _isCurrent = p;
        }
    }

    public class Languages : LinqToXmlDatabase, INotifyPropertyChanged
    {
        public ObservableCollectionEx<LanguageInfo> LanguageList { get; private set; }

        public Languages()
        {
            XDocument doc = Open();
            LanguageList = new ObservableCollectionEx<LanguageInfo>( from u in doc.Descendants( Tags.Language ) select new LanguageInfo( u ) );
            LanguageList.Insert( 0, defaultLanguageInfo() );
            _currentLanguage = "0";
            if( doc != null && doc.Root != null && doc.Root.Attribute( Tags.LanguageCurrent ) != null )
            {
                _currentLanguage = doc.Root.Attribute( Tags.LanguageCurrent ).Value;

                foreach( LanguageInfo info in LanguageList )
                {
                    if( info.Id == CurrentLanguage )
                    {
                        info.SetCurrentInternal( true );
                        break;
                    }
                }
            }
            else
            {
                LanguageList.First().SetCurrentInternal( true );
            }
        }


        protected override XElement Data
        {
            get
            {
                XElement root = new XElement( Tags.Languages, from language in LanguageList where language.Id != "0" select language.Data );
                root.SetAttributeValue( Tags.LanguageCurrent, CurrentLanguage );
                return root;
            }
        }

        protected override string PathToFile
        {
            get { return "languages.xml"; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _currentLanguage;
        public string CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            set
            {
                if( value != _currentLanguage )
                {
                    if( _currentLanguage != null && value != null )
                    {
                        LanguageInfo previous = null;
                        LanguageInfo current = null;
                        foreach( LanguageInfo info in LanguageList )
                        {
                            if( info.Id == _currentLanguage )
                            {
                                previous = info;
                            }
                            if( info.Id == value )
                            {
                                current = info;
                            }
                        }
                        _currentLanguage = value;
                        previous.IsCurrent = false;
                        current.IsCurrent = true;
                    }
                    else
                    {
                        _currentLanguage = value;
                    }
                }
            }
        }

        public List<LanguageInfo> parseRemote( string languagesXml )
        {
            XDocument languageDoc = XDocument.Load( new StringReader( languagesXml ) );
            var languageItemsQuery =
                from nedNodeElements in languageDoc.Root.Descendants( Tags.Language )
                select new LanguageInfo()
                {
                    Id = nedNodeElements.Element( Tags.LanguageId ).Value,
                    IsLocal = false,
                    LangName = nedNodeElements.Element( Tags.LanguageName ).Value,
                    Locale = nedNodeElements.Element( Tags.LanguageLocale ).Value,
                };
            List<LanguageInfo> allRemoteLanguages = new List<LanguageInfo>();
            foreach( LanguageInfo item in languageItemsQuery )
            {
                allRemoteLanguages.Add( item );
            }
            return allRemoteLanguages;
        }

        internal void LoadNewList( List<LanguageInfo> languageList )
        {
            if( languageList.Count > 1 )
            {

                foreach( LanguageInfo remoteLang in languageList )
                {
                    int oldIndex = LanguageList.IndexOf( remoteLang );
                    if( oldIndex >= 0 )
                    {
                        remoteLang.IsLocal = LanguageList.ElementAt( oldIndex ).IsLocal;
                        remoteLang.ItemState = LanguageList.ElementAt( oldIndex ).ItemState;
                    }
                }

                LanguageList.Clear();
                LanguageList.Add( defaultLanguageInfo() );

                foreach( LanguageInfo remoteLang in languageList )
                {
                    LanguageList.Add( remoteLang );
                }

                Save( true );
            }

            LanguageList.First().SetCurrentInternal( true );
        }

        private LanguageInfo defaultLanguageInfo()
        {
            return new LanguageInfo()
            {
                Id = "0",
                IsLocal = true,
                ItemState = MediaItemState.Local,
                LangName = "English ( Default )",
                Locale = "en-us"
            };
        }
    }


}
