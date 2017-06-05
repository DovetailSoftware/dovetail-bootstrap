namespace Dovetail.SDK.ModelMap.Transforms
{
	public interface ITransformArgument
	{
		string Name { get; }
		object Resolve(ModelData data);
	}
}