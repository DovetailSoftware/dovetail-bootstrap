namespace Dovetail.SDK.ModelMap.Transforms
{
	public interface IMappingTransform
	{
		object Execute(TransformContext context);
	}
}