﻿using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Clarify;

namespace Bootstrap.Web.Handlers.api.gbst.show
{
    public class get_Name_handler
    {
        private readonly IListCache _listCache;

        public get_Name_handler(IListCache listCache)
        {
            _listCache = listCache;
        }

        public GbstShowModel Execute(GbstShowRequest request)
        {
            var list = _listCache.GetGbstList(request.Name);

            var result = new GbstShowModel {Name = request.Name};

            if (list == null)
                return result;

            var defaultTitle = (list.DefaultElement != null) ? list.DefaultElement.Title : "n/a";

            result.Elements = list.Elements
                .OrderBy(s => s.Rank)
                .Select(e => new ListElement
                                 {
                                     Id = e.ObjectID,
                                     Title = e.Title,
                                     Rank = e.Rank,
                                     IsActive = e.State != 1,
                                     IsDefault = e.Title == defaultTitle
                                 });
            
            return result;
        }
    }
    
    public class GbstShowRequest
    {
        public string Name { get; set; }
        public string[] Levels { get; set; }
    }

    public class GbstShowModel 
    {
        public GbstShowModel()
        {
            Elements = new ListElement[0];
        }

        public string Name { get; set; }
        public IEnumerable<ListElement> Elements { get; set; }
    }

    public class ListElement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Rank { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive{ get; set; }
    }
}