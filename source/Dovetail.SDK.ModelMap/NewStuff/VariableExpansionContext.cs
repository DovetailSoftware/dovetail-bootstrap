using FubuCore;

namespace Dovetail.SDK.ModelMap.NewStuff
{
	public class VariableExpansionContext
	{
		private readonly IServiceLocator _services;
		private readonly string _key;
		private readonly ModelData _data;

		public VariableExpansionContext(IServiceLocator services, string key, ModelData data)
		{
			_services = services;
			_key = key;
			_data = data;
		}

		public string Key
		{
			get { return _key; }
		}

		public ModelData Data
		{
			get { return _data; }
		}

		public bool Matches(string key)
		{
			return _key.EqualsIgnoreCase(key);
		}

		public TService Service<TService>()
		{
			return _services.GetInstance<TService>();
		}
	}
}