namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public interface ITransformArgument
	{
		string Name { get; }
		object Resolve(ModelData data);
	}
}