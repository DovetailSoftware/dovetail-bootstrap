using System.Linq;

namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class StringConcatTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var args = context
				.Arguments
				.Where(_ => _.Key.ToLower().StartsWith("arg"))
				.OrderBy(_ => _.Key.ToLower())
				.Select(_ => _.Value)
				.ToArray();

			return string.Concat(args);
		}
	}
}