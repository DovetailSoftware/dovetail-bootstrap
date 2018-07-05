using Dovetail.SDK.ModelMap.Transforms;

namespace Dovetail.SDK.History
{
	public abstract class CancellationPolicy : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			return ShouldCancel(context);
		}

		public abstract bool ShouldCancel(TransformContext context);
	}
}