namespace Dovetail.SDK.Bootstrap.Http
{
	public interface IHttpApplicationStorage
	{
		bool Has(string key);
		void Store(string key, object value);
		object Get(string key);
		void Remove(string key);
	}
}
