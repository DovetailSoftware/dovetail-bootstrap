using System;
using System.Net;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Tests;
using Dovetail.SDK.Fubu.Actions;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Fubu.Tests
{
    [TestFixture]
    public class action_exception_behavior : Context<ActionExceptionWrapper<Error500Request>>
    {
        private IActionBehavior _insideBehavior;

        public override void Given()
        {
            AspNetSettings.IsCustomErrorsEnabled = false;
            _insideBehavior = MockFor<IActionBehavior>();
            _cut.InsideBehavior = _insideBehavior;
        }

        [Test]
        public void behavior_action_is_not_invoked_when_there_is_no_inside_behavior()
        {
            _cut.InsideBehavior = null;
            var actionInvoked = false;

            _cut.exceptionHandledBehavior(a=>actionInvoked = true);

            actionInvoked.ShouldBeFalse();
        }
        
        [Test]
        public void invoke_calls_invoke_on_the_inside_behavior()
        {
            _cut.Invoke();

            _insideBehavior.AssertWasCalled(a => a.Invoke());
        }

        [Test]
        public void invokepartial_calls_invokepartial_on_the_inside_behavior()
        {
            _cut.InvokePartial();

            _insideBehavior.AssertWasCalled(a => a.InvokePartial());
        }
    }

    [TestFixture]
    public class action_exception_behavior_for_exception_when_custom_errors_disabled : Context<ActionExceptionWrapper<Error500Request>>
    {
        private IActionBehavior _insideBehavior;
        private Exception _exception;
        
        public override void Given()
        {
            AspNetSettings.IsCustomErrorsEnabled = false;

            _exception = new Exception("bad things happened");

            _insideBehavior = MockFor<IActionBehavior>();
            _cut.InsideBehavior = _insideBehavior;
            _insideBehavior.Stub(s => s.Invoke()).Throw(_exception);
        }

        [Test]
        public void should_throw_the_exception()
        {
            _exception.GetType()
                .ShouldBeThrownBy(() => _cut.Invoke())
                .ShouldBeTheSameAs(_exception);
        }
    }

    [TestFixture, Ignore("Fubu 1.0 made partial invocations difficult to test")]
    public class action_exception_behavior_for_exception : Context<ActionExceptionWrapper<Error500Request>>
    {
        private IActionBehavior _insideBehavior;
        private Exception _exception;
        private IActionBehavior _partialBehavior;

	    public override void Given()
        {
            AspNetSettings.IsCustomErrorsEnabled = true;

			var graph = BehaviorGraph.BuildFrom(cfg =>
            {
				//cfg.Route(Guid.NewGuid().ToString()).Calls<ActionStatus500>(x => x.Execute(null));
            });

		    graph.AddActionFor("wee", typeof (ActionStatus500));
            _insideBehavior = MockFor<IActionBehavior>();
            _cut.InsideBehavior = _insideBehavior;

            _exception = new Exception("bad things happened");
            _insideBehavior.Stub(s => s.Invoke()).Throw(_exception);

            _partialBehavior = MockFor<IActionBehavior>();
            MockFor<IPartialFactory>().Stub(s => s.BuildPartial(null)).IgnoreArguments().Return(_partialBehavior);
            
            _cut.Invoke();
        }

        [Test]
        public void should_log_the_exception()
        {
            MockFor<ILogger>().AssertWasCalled(a => a.LogError("An API action threw an exception.", _exception));
        }

        [Test]
        public void should_invoke_the_error_request_partial()
        {
			_partialBehavior.AssertWasCalled(a => a.InvokePartial());
        }

        [Test]
        public void should_set_the_http_status_to_500()
        {
            MockFor<IOutputWriter>().AssertWasCalled(w=>w.WriteResponseCode(HttpStatusCode.InternalServerError));
        }
    }

	public class ActionStatus500
	{
		public string Execute(Error500Request error500Request)
		{
			return "boo!";
		}
	}
}