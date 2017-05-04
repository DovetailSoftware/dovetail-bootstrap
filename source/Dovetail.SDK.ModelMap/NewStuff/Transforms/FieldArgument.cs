namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class FieldArgument : ITransformArgument
	{
		private readonly string _name;
		private readonly ModelDataPath _path;

		public FieldArgument(string name, ModelDataPath path)
		{
			_name = name;
			_path = path;
		}

		public string Name
		{
			get { return _name; }
		}

		public object Resolve(ModelData data)
		{
			return _path.Get(data);
		}
	}
}