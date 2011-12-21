using System.Collections.Generic;
using System.Linq;

namespace Dovetail.SDK.ModelMap.Clarify
{
	public static class ClarifyListExtensions
	{
		public static IClarifyList SetSelection(this IClarifyList list, int selectionObjId)
		{
			var element = list.FirstOrDefault(f => f.DatabaseIdentifier == selectionObjId);
			if (element != null) element.IsSelected = true;

			return list;
		}

		public static IClarifyList PopulateDisplayTitles(this IClarifyList list, IDictionary<string, string> map)
		{
			foreach (var element in list.Where(element => map.ContainsKey(element.Title)))
			{
				element.DisplayTitle = map[element.Title];
			}

			return list;
		}
	}
}