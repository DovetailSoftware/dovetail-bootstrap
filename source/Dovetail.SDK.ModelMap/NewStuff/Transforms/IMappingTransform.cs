namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public interface IMappingTransform
	{
		object Execute(TransformContext context);
	}
}