using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Authentication.Principal;
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
			private IPrincipal _result;

			public override void Given()
			{
				var clarifySession = MockFor<IClarifySession>();
				clarifySession.Stub(s => s.Permissions).Return(new[] {"permission1", "permission2"});

				const string username = "annie";
				MockFor<IClarifySessionCache>().Expect(s => s.GetSession(username)).Return(clarifySession);
				_result = _cut.CreatePrincipal(username);
			}

			[Test]
			public void should_get_identified_user_session()
			{
				MockFor<IClarifySessionCache>().VerifyAllExpectations();
			}

			[Test]
			public void should_populate_principal_roles_from_permissions()
			{
				_result.IsInRole("permission1").ShouldBeTrue();
				_result.IsInRole("permission2").ShouldBeTrue();
				_result.IsInRole("permission3").ShouldBeFalse();
			}
		}

		[TestFixture]
		public class login_does_not_exist : Context<PrincipalFactory>
		{
			private IPrincipal _result;

			public override void Given()
			{
				const string username = "doesnotexist";

				MockFor<IClarifySessionCache>().Stub(s => s.GetSession(username)).Throw(new FCInvalidLoginException(10101, username));
				_result = _cut.CreatePrincipal(username);
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