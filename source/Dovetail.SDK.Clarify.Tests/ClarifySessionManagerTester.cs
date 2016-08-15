using System;
using System.Collections.Generic;
using FChoice.Foundation.Clarify;
using NUnit.Framework;

namespace Dovetail.SDK.Clarify.Tests
{
    [TestFixture]
    public class ClarifySessionManagerTester
    {
        private IClarifySessionManager theManager;
        private StubListener theListener;
        private IClarifySession theSession;
        private ILogger theLogger;

        [SetUp]
        public void SetUp()
        {
            theLogger = new NulloLogger();
            theListener = new StubListener();
            theSession = new StubSession();
            theManager = new ClarifySessionManager(new[] {theListener, theListener}, theLogger);
        }

        [Test]
        public void configuring_a_session_calls_created_then_started_on_the_listeners()
        {
            theManager.Configure(theSession);

            theListener.Calls[0].ShouldEqual("Created 1");
            theListener.Calls[1].ShouldEqual("Created 2");

            theListener.Calls[2].ShouldEqual("Started 3");
            theListener.Calls[3].ShouldEqual("Started 4");
        }

        [Test]
        public void ejecting_a_session_calls_closed_on_the_listeners()
        {
            theManager.Eject(theSession);

            theListener.Calls[0].ShouldEqual("Closed 1");
            theListener.Calls[1].ShouldEqual("Closed 2");
        }

        private class StubListener : IClarifySessionListener
        {
            private int _count;
            public readonly List<string> Calls = new List<string>();

            public void Created(IClarifySession session)
            {
                Calls.Add("Created " + ++_count);
            }

            public void Started(IClarifySession session)
            {
                Calls.Add("Started " + ++_count);
            }

            public void Closed(IClarifySession session)
            {
                Calls.Add("Closed " + ++_count);
            }
        }

        private class StubSession : IClarifySession
        {
            public Guid Id { get; }
            public string UserName { get; }
            public int SessionEmployeeId { get; }
            public int SessionUserId { get; }
            public string SessionEmployeeSiteId { get; }
            public IEnumerable<string> Permissions { get; }
            public IEnumerable<string> DataRestriction { get; }
            public ClarifyDataSet CreateDataSet()
            {
                throw new NotImplementedException();
            }

            public void RefreshContext()
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                throw new NotImplementedException();
            }
        }
    }
}