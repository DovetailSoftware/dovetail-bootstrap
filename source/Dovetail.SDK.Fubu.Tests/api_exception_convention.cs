using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Tests;
using Dovetail.SDK.Fubu.Actions;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using NUnit.Framework;

namespace Dovetail.SDK.Fubu.Tests
{
    [TestFixture]
    public class api_exception_convention
    {
        [Test]
        public void handles_api_actions()
        {
            var call = ActionCall.For<Action>(x => x.ApiMethod(null));

            APIExceptionConvention<Error500Request>.Handles(call).ShouldBeTrue();
        }

        [Test]
        public void handles_unauthenticated_api_actions()
        {
            var call = ActionCall.For<Action>(x => x.UnauthenticatedApiMethod(null));

            APIExceptionConvention<Error500Request>.Handles(call).ShouldBeTrue();
        }
        
        [Test]
        public void does_not_handle_non_actions()
        {
            var call = ActionCall.For<Action>(x => x.NonApiMethod(null));

            APIExceptionConvention<Error500Request>.Handles(call).ShouldBeFalse();
        }

        [Test]
        public void wraps_handled_actions_with_exception_wrapper()
        {
			var registry = new FubuRegistry(r => r.Policies.Add<APIExceptionConvention<Error500Request>>());
            registry.Actions.IncludeType<Action>();

			var graph = BehaviorGraph.BuildFrom(registry);

            graph.BehaviorFor<Action>(x => x.ApiMethod(null)).IsWrappedBy(typeof(ActionExceptionWrapper<Error500Request>)).ShouldBeTrue();
        }

    }

    public class Action
    {
        public Result ApiMethod(ApiRequest request)
        {
            return new Result();
        }

        public Result NonApiMethod(Request request)
        {
            return new Result();
        }

        public Result UnauthenticatedApiMethod(UnauthenticatedApiRequest request)
        {
            return new Result();
        }
    }

    public class Request { }
    public class ApiRequest : IApi { }
    public class UnauthenticatedApiRequest : IUnauthenticatedApi { }
    public class Result { }

    public class Error500Request : IServerErrorRequest 
    {
        public Error500Request()
        {
        }

        public Exception Exception { get; set; }
    }
}