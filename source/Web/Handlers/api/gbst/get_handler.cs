using System.ComponentModel;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Fubu.Authentication.Token;

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

            var lists = listGeneric.DataRows().Select(s => s.AsString("title")).ToArray();

            return new GbstListsModel {Lists = lists};
        }
    }

    [Description("A listing of gbst lists")]
    public class GbstListsRequest : IApi 
    {
    }

    public class GbstListsModel 
    {
        public string[] Lists { get; set; }
    }
}