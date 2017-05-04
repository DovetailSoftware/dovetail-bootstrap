using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class LocalizedListItemTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var listName = context.Arguments.Get<string>("listName");
			var title = context.Arguments.Get<string>("listValue");

			var lists = context.Service<IListCache>();
			return lists.GetLocalizedTitle(listName, title);
		}
	}

	public class ToUpperTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			return context.Arguments.Get<string>("value").ToUpper();
		}
	}
}