namespace Dovetail.SDK.ModelMap.NewStuff.Transforms
{
	public class ValueArgument : ITransformArgument
	{
		private readonly string _name;
		private readonly string _value;

		public ValueArgument(string name, string value)
		{
			_name = name;
			_value = value;
		}

		public string Name
		{
			get { return _name; }
		}

		public object Resolve(ModelData data)
		{
			return _value;
		}
	}
}