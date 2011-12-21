using System.Collections.Generic;

namespace Dovetail.SDK.ModelMap.Registration.DSL
{
	public class GlobalListConfigurationExpression
	{
		public GlobalListConfigurationExpression(string listName)
		{
			ListName = listName;
		}

		public string ListName { get; set; }
		public string SelectionFromRelation { get; set; }
		public IDictionary<string,string> TitleToDisplayTitleMap { get; set; }
	}
}