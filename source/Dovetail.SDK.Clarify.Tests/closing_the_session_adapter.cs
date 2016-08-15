using FChoice.Foundation.Clarify;
using NUnit.Framework;

namespace Dovetail.SDK.Clarify.Tests
{
    [TestFixture]
    public class closing_the_session_adapter
    {
        private ClarifySession theSession;
        private StubManager theManager;
        private ClarifySessionAdapter theAdapter;

        [SetUp]
        public void SetUp()
        {
            theSession = SessionUtilities.CreateEmptySession();
            theManager = new StubManager();
            theAdapter = new ClarifySessionAdapter(theSession, theManager);
        }

        [TearDown]
        public void TearDown()
        {
            ClarifySessionManager.Reset();
        }

        [Test]
        public void closes_the_underlying_clarify_session()
        {
            var closed = false;
            ClarifySessionManager.CloseWith((session) => closed = session.Equals(theSession));
            theAdapter.Close();

            closed.ShouldBeTrue();
        }

        [Test]
        public void invokes_the_manager()
        {
            ClarifySessionManager.CloseWith((session) => { });
            theAdapter.Close();

            theManager.Ejected.ShouldBeTrue();
        }

        private class StubManager : IClarifySessionManager
        {
            public bool Ejected { get; set; }

            public void Configure(IClarifySession session)
            {
                throw new System.NotImplementedException();
            }

            public void Eject(IClarifySession session)
            {
                Ejected = true;
            }
        }
    }
}