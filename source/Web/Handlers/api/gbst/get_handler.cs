using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;

namespace Bootstrap.Web.Handlers.api.gbst
{
    public class get_handler
    {
        private readonly IClarifySession _userSession;

        public get_handler(IClarifySession userSession)
        {
            _userSession = userSession;
        }

        public GbstListsModel Execute(GbstListsRequest request)
        {
            var dataSet = _userSession.CreateDataSet();
            var listGeneric = dataSet.CreateGeneric("gbst_lst");
            listGeneric.DataFields.Add("title");
            listGeneric.AppendSort("title", true);
            listGeneric.Query();

            var lists = listGeneric.DataRows().Select(s => s.AsString("title"));

            return new GbstListsModel {Lists = lists};
        }
    }
    
    public class GbstListsRequest
    {
    }

    public class GbstListsModel 
    {
        public IEnumerable<string> Lists { get; set; }
    }
}