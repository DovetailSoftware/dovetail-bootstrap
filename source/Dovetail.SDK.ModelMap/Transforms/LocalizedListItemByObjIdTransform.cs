using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.ModelMap.Transforms
{
	public class LocalizedListItemByObjIdTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var listName = context.Arguments.Get<string>("listName");
			var objId = context.Arguments.Get<int>("objId");

			var lists = context.Service<IListCache>();
			return lists.GetLocalizedTitleByObjid(listName, objId);
		}
	}
}