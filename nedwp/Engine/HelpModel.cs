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
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System;
using System.Xml;

namespace NedEngine
{
    public class HelpModel
    {
        public static string TAG_PAGES_ROOT = "Pages";
        public static string TAG_ITEMS_ROOT = "Items";
        public static string TAG_PAGE = "Page";
        public static string TAG_HELP_ITEM = "HelpItem";
        public static string TAG_TITLE = "Title";
        public static string TAG_DESCRIPTION = "Description";
        public static string TAG_HELP_ITEMS = "HelpItems";
        public static string ATTRIBUTE_NAME = "name";

        public string PageTitle { get; private set; }
        public string PageDescription { get; private set; }
        public List<HelpModelItem> HelpItemsList { get; private set; }
        public HelpPages CurrentHelpPage { get; private set; }

        public HelpModel()
        {
            CurrentHelpPage = HelpPages.EUnknownPage;
            PageTitle = String.Empty;
            PageDescription = String.Empty;
            HelpItemsList = new List<HelpModelItem>();
        }

        public bool FetchHelpData( HelpPages pageToFetch ) 
        {
            if (CurrentHelpPage == pageToFetch) // help data is up-to-date
                return true;

            CurrentHelpPage = pageToFetch;
            XDocument helpDoc = GetHelpDocument();
            var titleAndDescriptionQuery = from helpPages in helpDoc.Descendants(TAG_PAGES_ROOT).Elements()
                                           where helpPages.Attribute(ATTRIBUTE_NAME).Value == CurrentHelpPage.ToString()
                                           select new
                                           {
                                               Title = helpPages.Element(TAG_TITLE).Value,
                                               Description = helpPages.Element(TAG_DESCRIPTION).Value
                                           };
            if (titleAndDescriptionQuery.Count() == 0) // Page does not have help
                return false;
            PageTitle = titleAndDescriptionQuery.First().Title;
            PageDescription = titleAndDescriptionQuery.First().Description;
            var helpItemsQuery = from helpPages in helpDoc.Descendants(TAG_PAGES_ROOT).Elements()
                                 where helpPages.Attribute(ATTRIBUTE_NAME).Value == CurrentHelpPage.ToString()
                                 select new { HelpItems = (from helpItems in helpPages.Element(TAG_HELP_ITEMS).Elements() select helpItems.Attribute(ATTRIBUTE_NAME).Value) };
            var realHelpItemsQuery = from realHelpItems in helpDoc.Descendants(TAG_ITEMS_ROOT).Elements()
                                     select new { Name = realHelpItems.Attribute(ATTRIBUTE_NAME).Value, Item = new HelpModelItem { Title = realHelpItems.Element(TAG_TITLE).Value, Description = realHelpItems.Element(TAG_DESCRIPTION).Value } };
            
            var query = (from helpItems in helpItemsQuery.First().HelpItems
                         join realHelpItems in realHelpItemsQuery on helpItems equals realHelpItems.Name
                         into tempItems
                         from finalItems in tempItems.DefaultIfEmpty()
                         select finalItems.Item).ToList();
            HelpItemsList = query;
            return true;
        }

        private XDocument GetHelpDocument()
        {
            String cultureName = CultureInfo.CurrentUICulture.Name;
            XDocument documentXML;
            try
            {
                documentXML = XDocument.Load(String.Format("Resources/HelpData.{0}.xml", cultureName));
            }
            catch (XmlException)
            {
                documentXML = XDocument.Load("Resources/HelpData.xml");
            }
            return documentXML;
        }
    }

    public class HelpModelItem
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public HelpModelItem()
        {
        }
    }

    public enum HelpPages
    {
        EUnknownPage = -1,
        ESelectServerPage = 1,
        EUserLoginPage,
        ELibraryListPage,
        ELibraryManagerPage,
        ESettingsPage,
        EStatisticsPage,
        EDownloadsPage,
        ECataloguePage,
        ECategoryPage,
        EMediaItemsPage,
        EMediaItemPage,
        ESearchPage
    }
}
