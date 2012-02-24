using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
    public class when_creating_principal
    {
        [TestFixture]
        public class  happy_path : Context<PrincipalFactory>
        {
            private IIdentity _identity;
            private IPrincipal _result;

            public override void Given()
            {
                _identity = MockFor<IIdentity>();
                _identity.Stub(s => s.Name).Return("annie");

                var clarifySession = MockFor<IClarifySession>();
                clarifySession.Stub(s => s.Permissions).Return(new[] { "role1", "role2" });

                MockFor<IClarifySessionCache>().Expect(s => s.GetSession(_identity.Name)).Return(clarifySession);
                _result = _cut.CreatePrincipal(_identity);
            }

            [Test]
            public void should_get_identified_user_session()
            {
                MockFor<IClarifySessionCache>().VerifyAllExpectations();
            }

            [Test]
            public void should_return_principal_with_session_permissions_as_roles()
            {
                _result.IsInRole("role1").ShouldBeTrue();
                _result.IsInRole("role2").ShouldBeTrue();
                _result.IsInRole("notinrole").ShouldBeFalse();
            }
        }

        [TestFixture]
        public class  sad_path : Context<PrincipalFactory>
        {
            private IIdentity _identity;
            private IPrincipal _result;

            public override void Given()
            {
                _identity = MockFor<IIdentity>();
                _identity.Stub(s => s.Name).Return("annie");
                
                var clarifySession = MockFor<IClarifySession>();
                clarifySession.Stub(s => s.Permissions).Return(new[] {"role1", "role2"});

                MockFor<IClarifySessionCache>().Stub(s => s.GetSession(_identity.Name)).Return(clarifySession);
                _result = _cut.CreatePrincipal(_identity);
            }

            [Test]
            public void should_get_identified_user_session()
            {
                MockFor<IClarifySessionCache>().AssertWasCalled(s => s.GetSession(_identity.Name));
            }

            [Test]
            public void should_return_principal_with_session_permissions_as_roles()
            {
                _result.IsInRole("role1").ShouldBeTrue();
                _result.IsInRole("role2").ShouldBeTrue();
                _result.IsInRole("notinrole").ShouldBeFalse();
            }
        }
    }
}