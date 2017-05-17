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

		public static string GetLocalizedTitleByRank(this IListCache listCache, string listName, int rank)
		{
			var globalStringElementCollection = listCache.GetGbstListElements(listName, true);
			if (globalStringElementCollection != null)
			{
				var globalStringElement = globalStringElementCollection.Find(t => t.Rank == rank);
				return (globalStringElement == null) ? "" : globalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);
			}

			var hierarchicalStringElement = listCache.GetHgbstList(listName, true).Find(t => t.Rank == rank);
			return (hierarchicalStringElement == null) ? "" : hierarchicalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);
		}

		public static string GetLocalizedTitleByObjid(this IListCache listCache, string listName, int objid)
		{
			var globalStringElementCollection = listCache.GetGbstListElements(listName, true);
			if (globalStringElementCollection != null)
			{
				var globalStringElement = globalStringElementCollection.Find(t => t.ObjectID == objid);
				return (globalStringElement == null) ? "" : globalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);
			}

			var hierarchicalStringElement = listCache.GetHgbstList(listName, true).Find(t => t.ObjectID == objid);
			return (hierarchicalStringElement == null) ? "" : hierarchicalStringElement.GetLocalizedTitle(CultureInfo.CurrentCulture);
		}
	}
}