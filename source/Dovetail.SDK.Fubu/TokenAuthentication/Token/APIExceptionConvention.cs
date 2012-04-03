using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Fubu.Actions;
using Dovetail.SDK.Fubu.TokenAuthentication.Token.Extensions;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
    public class APIExceptionConvention<TServerErrorRequest> : IConfigurationAction
        where TServerErrorRequest : class, IServerErrorRequest, new()
    {
        public void Configure(BehaviorGraph graph)
        {
            graph
                .Actions()
                .Where(Handles)
                .Each(action => action.WrapWith<ActionExceptionWrapper<TServerErrorRequest>>());
        }

        public static bool Handles(ActionCall action)
        {
            return action.InputType().IsAPIRequest();
        }
    }
}