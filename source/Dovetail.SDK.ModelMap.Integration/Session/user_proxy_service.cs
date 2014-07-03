using Dovetail.SDK.Bootstrap.Authentication;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.Session
{
	[TestFixture]
	public class user_proxy_service : MapFixture
	{
		private IUserProxyService _cut;

		public override void beforeAll()
		{
			_cut = Container.GetInstance<IUserProxyService>();
		}

		[Test]
		public void cancelling_proxy_should_do_nothing_when_proxy_is_not_setup()
		{
//
//
//			UserProxyService.CreateProxyFor("annie", "hank");
//
//
//			_cut.CancelProxy("annie");
		}
	}
}