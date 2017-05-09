namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class StringConcatTransform : IMappingTransform
	{
		public object Execute(TransformContext context)
		{
			var arg1 = context.Arguments.Get<string>("arg1");
			var arg2 = context.Arguments.Get<string>("arg2");

			return string.Concat(arg1, arg2);
		}
	}
}