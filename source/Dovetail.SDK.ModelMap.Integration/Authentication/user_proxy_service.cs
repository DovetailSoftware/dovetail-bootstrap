using Dovetail.SDK.ModelMap.Integration.Session;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Authentication
{
	public class user_proxy_service
	{
		[TestFixtureSetUp]
		public void beforeAll()
		{

			var container = bootstrap_ioc.getContainer(c =>
			{
				
			});

		}
	}
}
