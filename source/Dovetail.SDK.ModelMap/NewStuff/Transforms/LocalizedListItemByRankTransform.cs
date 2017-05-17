using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class LocalizedListItemByRankTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var listName = context.Arguments.Get<string>("listName");
			var rank = context.Arguments.Get<int>("rank");

			var lists = context.Service<IListCache>();
			return lists.GetLocalizedTitleByRank(listName, rank);
		}
	}
}