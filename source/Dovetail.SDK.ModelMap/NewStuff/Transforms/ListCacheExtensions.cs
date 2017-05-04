using System.Globalization;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public static class ListCacheExtensions
	{
		public static string GetLocalizedTitle(this IListCache listCache, string listName, string title)
		{
			var gbstListElements = listCache.GetGbstListElements(listName, true);
			if (gbstListElements != null)
			{
				var globalStringElement = gbstListElements.Find(t => t.Title == title);
				if (globalStringElement != null)
					return globalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);

				return title;
			}

			var hierarchicalStringElement = listCache.GetHgbstList(listName, true).Find(t => t.Title == title);
			if (hierarchicalStringElement != null)
				return hierarchicalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);

			return title;
		}
	}
}