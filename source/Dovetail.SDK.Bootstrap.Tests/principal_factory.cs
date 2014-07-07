using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	public class when_creating_principal
	{
		[TestFixture]
		public class happy_path : Context<PrincipalFactory>
		{
			private IIdentity _identity;

			public override void Given()
			{
				_identity = MockFor<IIdentity>();
				_identity.Stub(s => s.Name).Return("annie");

				var clarifySession = MockFor<IClarifySession>();

				MockFor<IClarifySessionCache>().Expect(s => s.GetSession(_identity.Name)).Return(clarifySession);
				_cut.CreatePrincipal(_identity);
			}

			[Test]
			public void should_get_identified_user_session()
			{
				MockFor<IClarifySessionCache>().VerifyAllExpectations();
			}
		}

		[TestFixture]
		public class login_does_not_exist : Context<PrincipalFactory>
		{
			private IIdentity _identity;
			private IPrincipal _result;

			public override void Given()
			{
				_identity = MockFor<IIdentity>();
				_identity.Stub(s => s.Name).Return("doesnotexist");

				MockFor<IClarifySessionCache>().Stub(s => s.GetSession(_identity.Name)).Throw(new FCInvalidLoginException(10101, _identity.Name));
				_result = _cut.CreatePrincipal(_identity);
			}

			[Test]
			public void should_return_null()
			{
				_result.ShouldBeNull();
			}

			[Test]
			public void should_sign_user_out()
			{
				MockFor<IFormsAuthenticationService>().AssertWasCalled(a => a.SignOut());
			}

		}
	}
}